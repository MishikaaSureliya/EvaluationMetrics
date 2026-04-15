using Microsoft.ML;
using Microsoft.ML.Data;
using EvolutionMetrics.Models;

namespace EvolutionMetrics.Services
{
    /// <summary>
    /// ML service responsible for training and predicting laptop prices.
    /// </summary>
    public class LaptopService : BaseMLService, ILaptopService
    {
        private readonly MLContext _mlContext;
        private readonly ILogger<LaptopService> _logger;
        private readonly string _modelPath = "MLModels/laptop_model.zip";
        private ITransformer? _model;

        public LaptopService(ILogger<LaptopService> logger)
        {
            _mlContext = new MLContext();
            _logger = logger;
        }

        /// <summary>
        /// Loads or trains the laptop ML model from the specified CSV data path.
        /// </summary>
        public void LoadOrTrain(string dataPath)
        {
            Train(dataPath);
        }

        /// <summary>
        /// Trains the laptop price ML model using data from the specified CSV file.
        /// </summary>
        public void Train(string dataPath)
        {
            try
            {
                _logger.LogInformation("Laptop training started");

                var data = _mlContext.Data.LoadFromTextFile<LaptopData>(
                    dataPath, hasHeader: true, separatorChar: ',');

                var split = _mlContext.Data.TrainTestSplit(data, testFraction: 0.2);

                var pipeline = _mlContext.Transforms.Categorical.OneHotEncoding("BrandEncoded", "Brand")
                    .Append(_mlContext.Transforms.Categorical.OneHotEncoding("ProcessorEncoded", "Processor"))
                    .Append(_mlContext.Transforms.Concatenate("Features",
                        "BrandEncoded",
                        "RAM",
                        "Storage",
                        "ProcessorEncoded",
                        "ScreenSize"))
                    .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
                    .Append(_mlContext.Regression.Trainers.LightGbm());

                _model = pipeline.Fit(split.TrainSet);

                _logger.LogInformation("Laptop model trained successfully");

                var predictions = _model.Transform(split.TestSet);
                var metrics = _mlContext.Regression.Evaluate(predictions);

                SetMetrics(
                    metrics.MeanAbsoluteError,
                    metrics.RootMeanSquaredError,
                    metrics.RSquared);

                Directory.CreateDirectory("MLModels");
                _mlContext.Model.Save(_model, data.Schema, _modelPath);

                _logger.LogInformation("Laptop model saved at {Path}", _modelPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Laptop training failed");
            }
        }

        /// <summary>
        /// Predicts the price for the given laptop input data.
        /// </summary>
        public float Predict(LaptopData input)
        {
            try
            {
                _logger.LogInformation(
                    "Laptop Predict - Brand={Brand}, RAM={RAM}, Storage={Storage}, CPU={CPU}, Screen={Screen}",
                    input.Brand, input.RAM, input.Storage, input.Processor, input.ScreenSize);

                if (_model == null)
                {
                    _logger.LogError("Laptop model is not loaded");
                    return 0;
                }

                var engine = _mlContext.Model.CreatePredictionEngine<LaptopData, LaptopPrediction>(_model);
                var result = engine.Predict(input).PredictedPrice;

                if (float.IsNaN(result) || float.IsInfinity(result))
                {
                    _logger.LogWarning("Invalid laptop prediction result");
                    return 0;
                }

                _logger.LogInformation("Laptop Prediction = {Price}", result);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Laptop prediction error");
                return 0;
            }
        }
    }
}

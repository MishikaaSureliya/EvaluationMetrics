using Microsoft.ML;
using Microsoft.ML.Data;
using EvolutionMetrics.Models;

namespace EvolutionMetrics.Services
{
    /// <summary>
    /// ML service responsible for training and predicting calories burned.
    /// </summary>
    public class CaloriesService : BaseMLService, ICaloriesService
    {
        private readonly MLContext _mlContext;
        private readonly ILogger<CaloriesService> _logger;
        private readonly string _modelPath = "MLModels/calories_model.zip";
        private ITransformer? _model;

        public CaloriesService(ILogger<CaloriesService> logger)
        {
            _mlContext = new MLContext();
            _logger = logger;
        }

        /// <summary>
        /// Loads or trains the calories ML model from the specified CSV data path.
        /// </summary>
        public void LoadOrTrain(string dataPath)
        {
            Train(dataPath);
        }

        /// <summary>
        /// Trains the calories ML model using data from the specified CSV file.
        /// </summary>
        public void Train(string dataPath)
        {
            try
            {
                _logger.LogInformation("Calories training started");

                var data = _mlContext.Data.LoadFromTextFile<CaloriesData>(
                    dataPath, hasHeader: true, separatorChar: ',');

                var split = _mlContext.Data.TrainTestSplit(data, testFraction: 0.2);

                var pipeline = _mlContext.Transforms.Categorical.OneHotEncoding("ExerciseTypeEncoded", "ExerciseType")
                    .Append(_mlContext.Transforms.Concatenate("Features",
                        "Weight", "Duration", "HeartRate", "ExerciseTypeEncoded"))
                    .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
                    .Append(_mlContext.Regression.Trainers.FastTree());

                _model = pipeline.Fit(split.TrainSet);

                _logger.LogInformation("Calories model trained successfully");

                var predictions = _model.Transform(split.TestSet);
                var metrics = _mlContext.Regression.Evaluate(predictions);

                SetMetrics(
                    metrics.MeanAbsoluteError,
                    metrics.RootMeanSquaredError,
                    metrics.RSquared);

                Directory.CreateDirectory("MLModels");
                _mlContext.Model.Save(_model, data.Schema, _modelPath);

                _logger.LogInformation("Calories model saved at {Path}", _modelPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Calories training failed");
            }
        }

        /// <summary>
        /// Predicts the calories burned for the given input data.
        /// </summary>
        public float Predict(CaloriesData input)
        {
            try
            {
                _logger.LogInformation(
                    "Calories Predict - Weight={Weight}, Duration={Duration}, HR={HR}, Type={Type}",
                    input.Weight, input.Duration, input.HeartRate, input.ExerciseType);

                if (_model == null)
                {
                    _logger.LogError("Calories model is not loaded");
                    return 0;
                }

                var engine = _mlContext.Model.CreatePredictionEngine<CaloriesData, CaloriesPrediction>(_model);
                var result = engine.Predict(input).PredictedCalories;

                if (float.IsNaN(result) || float.IsInfinity(result))
                {
                    _logger.LogWarning("Invalid calories prediction result");
                    return 0;
                }

                _logger.LogInformation("Calories Prediction = {Result}", result);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Calories prediction error");
                return 0;
            }
        }
    }
}

using EvolutionMetrics.Models;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace EvolutionMetrics.Services
{
    /// <summary>
    /// ML service responsible for training and predicting rainfall amounts.
    /// </summary>
    public class RainfallService : BaseMLService, IRainfallService
    {
        private readonly MLContext _mlContext;
        private readonly ILogger<RainfallService> _logger;
        private ITransformer? _model;

        /// <summary>
        /// Indicates whether the service is using fallback dummy training data.
        /// </summary>
        public bool IsUsingDummy { get; set; } = false;

        public RainfallService(ILogger<RainfallService> logger)
        {
            _mlContext = new MLContext();
            _logger = logger;
        }

        /// <summary>
        /// Loads or trains the rainfall ML model from the specified CSV data path.
        /// Falls back to dummy data if the file does not exist or is empty.
        /// </summary>
        public void LoadOrTrain(string dataPath)
        {
            if (!File.Exists(dataPath))
            {
                _logger.LogWarning("Rainfall dataset not found at {Path}. Using dummy data.", dataPath);
                IsUsingDummy = true;
                TrainFromList(GetDummyData());
                return;
            }

            var lines = File.ReadAllLines(dataPath);

            if (lines.Length <= 1)
            {
                _logger.LogWarning("Rainfall dataset is empty at {Path}. Using dummy data.", dataPath);
                IsUsingDummy = true;
                TrainFromList(GetDummyData());
                return;
            }

            IsUsingDummy = false;
            Train(dataPath);
        }

        /// <summary>
        /// Trains the rainfall ML model using data from the specified CSV file.
        /// </summary>
        public void Train(string dataPath)
        {
            _logger.LogInformation("Rainfall training started");

            var data = _mlContext.Data.LoadFromTextFile<RainfallData>(
                dataPath, hasHeader: true, separatorChar: ',');

            var split = _mlContext.Data.TrainTestSplit(data, testFraction: 0.3);

            var pipeline = _mlContext.Transforms.ReplaceMissingValues(
                    new[]
                    {
                        new InputOutputColumnPair(nameof(RainfallData.Temperature)),
                        new InputOutputColumnPair(nameof(RainfallData.Humidity)),
                        new InputOutputColumnPair(nameof(RainfallData.WindSpeed)),
                        new InputOutputColumnPair(nameof(RainfallData.Pressure))
                    })
                .Append(_mlContext.Transforms.Concatenate("Features",
                    nameof(RainfallData.Temperature),
                    nameof(RainfallData.Humidity),
                    nameof(RainfallData.WindSpeed),
                    nameof(RainfallData.Pressure)))
                .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
                .Append(_mlContext.Regression.Trainers.FastTree());

            _model = pipeline.Fit(split.TrainSet);

            _logger.LogInformation("Rainfall model trained successfully");

            var predictions = _model.Transform(split.TestSet);
            var metrics = _mlContext.Regression.Evaluate(predictions);

            SetMetrics(
                metrics.MeanAbsoluteError,
                metrics.RootMeanSquaredError,
                metrics.RSquared);

            Directory.CreateDirectory("MLModels");
            _mlContext.Model.Save(_model, data.Schema, "MLModels/rainfall_model.zip");

            _logger.LogInformation("Rainfall model saved at MLModels/rainfall_model.zip");
        }

        /// <summary>
        /// Trains the rainfall ML model using an in-memory list of dummy data records.
        /// </summary>
        private void TrainFromList(List<RainfallData> dataList)
        {
            var data = _mlContext.Data.LoadFromEnumerable(dataList);

            var split = _mlContext.Data.TrainTestSplit(data, testFraction: 0.2);

            var pipeline = _mlContext.Transforms.ReplaceMissingValues(
                    new[]
                    {
                        new InputOutputColumnPair(nameof(RainfallData.Temperature)),
                        new InputOutputColumnPair(nameof(RainfallData.Humidity)),
                        new InputOutputColumnPair(nameof(RainfallData.WindSpeed)),
                        new InputOutputColumnPair(nameof(RainfallData.Pressure))
                    })
                .Append(_mlContext.Transforms.Concatenate("Features",
                    nameof(RainfallData.Temperature),
                    nameof(RainfallData.Humidity),
                    nameof(RainfallData.WindSpeed),
                    nameof(RainfallData.Pressure)))
                .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
                .Append(_mlContext.Regression.Trainers.FastTree());

            _model = pipeline.Fit(split.TrainSet);

            var predictions = _model.Transform(split.TestSet);
            var metrics = _mlContext.Regression.Evaluate(predictions);

            SetMetrics(
                metrics.MeanAbsoluteError,
                metrics.RootMeanSquaredError,
                metrics.RSquared);
        }

        /// <summary>
        /// Generates a synthetic dummy dataset for fallback model training.
        /// </summary>
        private List<RainfallData> GetDummyData()
        {
            var dataList = new List<RainfallData>();

            for (int i = 0; i < 100; i++)
            {
                float temp = 20 + (i % 15);
                float humidity = 40 + (i % 50);
                float wind = 5 + (i % 15);
                float pressure = 990 + (i % 30);

                float rainfall = (humidity * 0.6f)
                                 - (temp * 0.3f)
                                 + (wind * 0.4f)
                                 + ((1010 - pressure) * 0.5f);

                dataList.Add(new RainfallData
                {
                    Temperature = temp,
                    Humidity = humidity,
                    WindSpeed = wind,
                    Pressure = pressure,
                    Rainfall = rainfall
                });
            }

            return dataList;
        }

        /// <summary>
        /// Predicts rainfall for the given weather parameters.
        /// Returns a fallback value if the model is unavailable.
        /// </summary>
        public (float prediction, bool isDummy) Predict(float temp, float humidity, float wind, float pressure)
        {
            try
            {
                _logger.LogInformation(
                    "Rainfall Predict - Temp={Temp}, Humidity={Humidity}, Wind={Wind}, Pressure={Pressure}",
                    temp, humidity, wind, pressure);

                if (_model == null)
                {
                    _logger.LogWarning("Rainfall model is not loaded. Returning fallback value.");
                    return (50.0f, true);
                }

                var engine = _mlContext.Model.CreatePredictionEngine<RainfallData, RainfallPrediction>(_model);

                var prediction = engine.Predict(new RainfallData
                {
                    Temperature = temp,
                    Humidity = humidity,
                    WindSpeed = wind,
                    Pressure = pressure
                });

                return (prediction.PredictedRainfall, IsUsingDummy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rainfall prediction error");
                return (50.0f, true);
            }
        }
    }
}
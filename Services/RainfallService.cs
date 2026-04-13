using EvolutionMetrics.Models;
using Microsoft.ML;
using Microsoft.ML.Data;
using Serilog;

namespace EvolutionMetrics.Services
{
    public class RainfallService : BaseMLService
    {
        private readonly MLContext _mlContext;
        private ITransformer _model;

        public bool IsUsingDummy { get; set; } = false;

        public RainfallService()
        {
            _mlContext = new MLContext();
        }

        // 🔥 Load or Train
        public void LoadOrTrain(string dataPath)
        {
            if (!File.Exists(dataPath))
            {
                IsUsingDummy = true;
                TrainFromList(GetDummyData());
                return;
            }

            // 🔥 CHECK IF FILE HAS DATA
            var lines = File.ReadAllLines(dataPath);

            if (lines.Length <= 1) // only header or empty
            {
                IsUsingDummy = true;
                TrainFromList(GetDummyData());
                return;
            }

            IsUsingDummy = false;
            Train(dataPath);
        }

        // 🔥 Train from CSV
        public void Train(string dataPath)
        {
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

            var predictions = _model.Transform(split.TestSet);
            var metrics = _mlContext.Regression.Evaluate(predictions);

            SetMetrics(
     metrics.MeanAbsoluteError,
     metrics.RootMeanSquaredError,
     metrics.RSquared
 );

            Directory.CreateDirectory("MLModels");
            _mlContext.Model.Save(_model, data.Schema, "MLModels/rainfall_model.zip");

            Console.WriteLine("✅ Rainfall model saved!");

            Log.Information("📊 Rainfall training started");

            _model = pipeline.Fit(split.TrainSet);

            Log.Information("✅ Model trained successfully");

            _mlContext.Model.Save(_model, data.Schema, "MLModels/rainfall_model.zip");

            Log.Information("💾 Model saved at MLModels/rainfall_model.zip");
        }

        // 🔥 Train from Dummy Data
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
                metrics.RSquared
            );
        }

        // 🔥 Dummy Dataset
        private List<RainfallData> GetDummyData()
        {
            var list = new List<RainfallData>();

            for (int i = 0; i < 100; i++)
            {
                float temp = 20 + (i % 15);
                float humidity = 40 + (i % 50);
                float wind = 5 + (i % 15);
                float pressure = 990 + (i % 30);

                // 🔥 REALISTIC FORMULA
                float rainfall = (humidity * 0.6f)
                                 - (temp * 0.3f)
                                 + (wind * 0.4f)
                                 + ((1010 - pressure) * 0.5f);

                list.Add(new RainfallData
                {
                    Temperature = temp,
                    Humidity = humidity,
                    WindSpeed = wind,
                    Pressure = pressure,
                    Rainfall = rainfall
                });
            }

            return list;
        }


        // 🔥 Prediction (with dummy flag)
        public (float prediction, bool isDummy) Predict(float temp, float humidity, float wind, float pressure)
        {
            try
            {
                if (_model == null)
                {
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
            catch
            {
                return (50.0f, true);
            }
            Log.Information("🔍 Predict called with Temp={Temp}, Humidity={Humidity}", temp, humidity);
        }
    }
}
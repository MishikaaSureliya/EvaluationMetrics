using Microsoft.ML;
using Microsoft.ML.Data;
using EvolutionMetrics.Models;
using System.Security.Cryptography.X509Certificates;
using Serilog;

namespace EvolutionMetrics.Services
{
    public class CaloriesService : BaseMLService
    {
        private readonly MLContext _mlContext;
        private ITransformer? _model;
        private readonly string modelPath = "MLModels/calories_model.zip";
     

        public CaloriesService()
        {
            _mlContext = new MLContext();
        }

        public void LoadOrTrain(string DataPath)
        {
            Train(DataPath);
        }
        public void Train(string DataPath)
        {
            try
            {
                Log.Information("🔥 Calories training started");

                var data = _mlContext.Data.LoadFromTextFile<CaloriesData>(
                DataPath, hasHeader: true, separatorChar: ',');

            var split = _mlContext.Data.TrainTestSplit(data, testFraction: 0.2);

            var pipeline = _mlContext.Transforms.Categorical.OneHotEncoding("ExerciseTypeEncoded", "ExerciseType")
                .Append(_mlContext.Transforms.Concatenate("Features",
                    "Weight", "Duration", "HeartRate", "ExerciseTypeEncoded"))
                .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
                .Append(_mlContext.Regression.Trainers.FastTree());

            _model = pipeline.Fit(split.TrainSet);
                Log.Information("✅ Calories model trained");

                var predictions = _model.Transform(split.TestSet);
            var metrics = _mlContext.Regression.Evaluate(predictions);

                SetMetrics(
         metrics.MeanAbsoluteError,
         metrics.RootMeanSquaredError,
         metrics.RSquared
     );

                Directory.CreateDirectory("MLModels");
            _mlContext.Model.Save(_model, data.Schema, modelPath);

                Log.Information("💾 Calories model saved at {Path}", modelPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Calories training failed");
            }

        }

        public float Predict(CaloriesData input)
        {
            try
            {
                Log.Information("📥 Calories Predict → Weight={Weight}, Duration={Duration}, HR={HR}, Type={Type}",
                    input.Weight, input.Duration, input.HeartRate, input.ExerciseType);

                if (_model == null)
                {
                    Log.Error("❌ Model is NULL");
                    return 0;
                }

                var engine = _mlContext.Model.CreatePredictionEngine<CaloriesData, CaloriesPrediction>(_model);

                var result = engine.Predict(input).PredictedCalories;

            
                if (float.IsNaN(result) || float.IsInfinity(result))
                {
                    Log.Warning("⚠️ Invalid Calories prediction");
                    return 0;
                }

                Log.Information("📤 Calories Prediction = {Result}", result);

                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Calories prediction error");
                return 0;
            }
        }
    }
}

using Microsoft.ML;
using Microsoft.ML.Data;
using EvolutionMetrics.Models;
using Serilog;

namespace EvolutionMetrics.Services
{
    public class LaptopService : BaseMLService
    {
        private readonly MLContext _mlContext;
        private ITransformer _model;
        private readonly string modelPath = "MLModels/laptop_model.zip";


        public LaptopService()
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
                Log.Information("🔥 Laptop training started");
                var data = _mlContext.Data.LoadFromTextFile<LaptopData>(
                DataPath, hasHeader: true, separatorChar: ',');

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

                Log.Information("✅ Laptop model trained");

                var predictions = _model.Transform(split.TestSet);
            var metrics = _mlContext.Regression.Evaluate(predictions);

                SetMetrics(
          metrics.MeanAbsoluteError,
          metrics.RootMeanSquaredError,
          metrics.RSquared
      );

                Directory.CreateDirectory("MLModels");
            _mlContext.Model.Save(_model, data.Schema, modelPath);
                Log.Information("💾 Laptop model saved at {Path}", modelPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Laptop training failed");
            }
        }
         public float Predict(LaptopData input)
         {
            try
            {
                Log.Information("📥 Laptop Predict → Brand={Brand}, RAM={RAM}, Storage={Storage}, CPU={CPU}, Screen={Screen}",
                    input.Brand, input.RAM, input.Storage, input.Processor, input.ScreenSize);
                var engine = _mlContext.Model.CreatePredictionEngine<LaptopData, LaptopPrediction>(_model);
            var result = engine.Predict(input).PredictedPrice;
            if (float.IsNaN(result) || float.IsInfinity(result))
                {
                    Log.Warning("⚠️ Invalid Laptop prediction");
                    return 0;
                }

                Log.Information("📤 Laptop Prediction = {Price}", result);

                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Laptop prediction error");
                return 0;
            }
        }
    }
}

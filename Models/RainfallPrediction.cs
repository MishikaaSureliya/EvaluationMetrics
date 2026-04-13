using Microsoft.ML.Data;

namespace EvolutionMetrics.Models
{
    public class RainfallPrediction
    {
        [ColumnName("Score")]
        public float PredictedRainfall { get; set; }
    }
}

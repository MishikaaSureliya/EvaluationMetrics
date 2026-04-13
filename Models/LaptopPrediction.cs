using Microsoft.ML.Data;

namespace EvolutionMetrics.Models
{
    public class LaptopPrediction
    {
        [ColumnName("Score")]
        public float PredictedPrice { get; set; }
    }
}

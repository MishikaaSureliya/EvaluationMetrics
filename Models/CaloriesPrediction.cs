using Microsoft.ML.Data;

namespace EvolutionMetrics.Models
{
    public class CaloriesPrediction
    {
        [ColumnName("Score")]
        public float PredictedCalories { get; set; }
    }
}

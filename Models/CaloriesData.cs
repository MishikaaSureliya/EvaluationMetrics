using Microsoft.ML.Data;

namespace EvolutionMetrics.Models
{
    public class CaloriesData
    {
        [LoadColumn(0)]
        public float Weight { get; set; }   
        [LoadColumn(1)]
        public float Duration { get; set; } 
        [LoadColumn(2)]
        public float HeartRate { get; set; }
        [LoadColumn(3)]
        public string ExerciseType { get; set; } = string.Empty;

        [LoadColumn(4)]
        [ColumnName("Label")]
        public float Calories { get; set; }

    }
}

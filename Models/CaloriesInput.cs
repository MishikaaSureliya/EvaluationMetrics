using Microsoft.AspNetCore.Routing.Constraints;

namespace EvolutionMetrics.Models
{
    public class CaloriesInput
    {
        public float Weight { get; set; }
        public float Duration { get; set; }
        public float HeartRate { get; set; }
        public string ExerciseType { get; set; } = string.Empty;
    }
}

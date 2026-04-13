using Microsoft.ML.Data;

namespace EvolutionMetrics.Models
{
    public class RainfallData
    {
        [LoadColumn(0)]
        public float Temperature { get; set; }
        [LoadColumn(1)]
        public float Humidity { get; set; }
        [LoadColumn(2)]
        public float WindSpeed { get; set; }
        [LoadColumn(3)]
        public float Pressure { get; set; }

        [LoadColumn(4)]
        [ColumnName("Label")]
        public float Rainfall { get; set; }

    }
}

using Microsoft.ML.Data;

namespace EvolutionMetrics.Models
{
    public class LaptopData
    {
        [LoadColumn(0)]
        public string Brand { get; set; } = string.Empty;
        [LoadColumn(1)]
        public float RAM { get; set; }
        [LoadColumn(2)]
        public float Storage { get; set; }
        [LoadColumn(3)]
        public string Processor { get; set; } = string.Empty;
        [LoadColumn(4)]
        public float ScreenSize { get; set; }

        [LoadColumn(5)]
        [ColumnName("Label")]
        public float Price { get; set; }
    }
}

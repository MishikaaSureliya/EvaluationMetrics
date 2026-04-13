using EvolutionMetrics.Models;

namespace EvolutionMetrics.Services
{
    public class BaseMLService
    {
        public MetricsModel Metrics { get; private set; } = new MetricsModel();

        protected void SetMetrics(double mae, double rmse, double r2)
        {
            Metrics.MAE = double.IsNaN(mae) ? 0 : mae;
            Metrics.RMSE = double.IsNaN(rmse) ? 0 : rmse;
            Metrics.R2 = double.IsNaN(r2) ? 0 : r2;
        }
    }
}
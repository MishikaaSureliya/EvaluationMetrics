using EvolutionMetrics.Models;

namespace EvolutionMetrics.Services
{
    /// <summary>
    /// Base class providing shared ML model evaluation metrics for derived services.
    /// </summary>
    public class BaseMLService
    {
        /// <summary>
        /// Gets the evaluation metrics (MAE, RMSE, R²) for the trained model.
        /// </summary>
        public MetricsModel Metrics { get; private set; } = new MetricsModel();

        /// <summary>
        /// Sets the evaluation metrics after model training, replacing NaN values with zero.
        /// </summary>
        protected void SetMetrics(double mae, double rmse, double r2)
        {
            Metrics.MAE = double.IsNaN(mae) ? 0 : mae;
            Metrics.RMSE = double.IsNaN(rmse) ? 0 : rmse;
            Metrics.R2 = double.IsNaN(r2) ? 0 : r2;
        }
    }
}
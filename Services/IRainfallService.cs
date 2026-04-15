using EvolutionMetrics.Models;

namespace EvolutionMetrics.Services
{
    /// <summary>
    /// Contract for the rainfall prediction ML service.
    /// </summary>
    public interface IRainfallService
    {
        /// <summary>
        /// Gets the evaluation metrics (MAE, RMSE, R²) of the trained model.
        /// </summary>
        MetricsModel Metrics { get; }

        /// <summary>
        /// Indicates whether the service is using fallback dummy training data.
        /// </summary>
        bool IsUsingDummy { get; set; }

        /// <summary>
        /// Loads or trains the ML model from the specified CSV data path.
        /// </summary>
        void LoadOrTrain(string dataPath);

        /// <summary>
        /// Predicts rainfall for the given weather parameters.
        /// </summary>
        (float prediction, bool isDummy) Predict(float temp, float humidity, float wind, float pressure);
    }
}

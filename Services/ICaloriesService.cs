using EvolutionMetrics.Models;

namespace EvolutionMetrics.Services
{
    /// <summary>
    /// Contract for the calories burned prediction ML service.
    /// </summary>
    public interface ICaloriesService
    {
        /// <summary>
        /// Gets the evaluation metrics (MAE, RMSE, R²) of the trained model.
        /// </summary>
        MetricsModel Metrics { get; }

        /// <summary>
        /// Loads or trains the ML model from the specified CSV data path.
        /// </summary>
        void LoadOrTrain(string dataPath);

        /// <summary>
        /// Predicts the calories burned for the given input data.
        /// </summary>
        float Predict(CaloriesData input);
    }
}

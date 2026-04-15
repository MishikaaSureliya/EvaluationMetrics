using EvolutionMetrics.Models;

namespace EvolutionMetrics.Services
{
    /// <summary>
    /// Contract for the laptop price prediction ML service.
    /// </summary>
    public interface ILaptopService
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
        /// Predicts the price for the given laptop input data.
        /// </summary>
        float Predict(LaptopData input);
    }
}

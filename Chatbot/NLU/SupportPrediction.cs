using Microsoft.ML.Data;

namespace Chatbot.NLU {
    /// <summary>
    /// Repræsenterer output fra ML.NET modellen.
    /// </summary>
    public class SupportPrediction {
        [ColumnName("PredictedLabel")]
        public string PredictedIntent { get; set; }
    }
}

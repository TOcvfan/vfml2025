using Microsoft.ML.Data;

namespace Chatbot.NLU {
    /// <summary>
    /// Inputmodel til ML.NET under træning og prediction.
    /// </summary>
    public class IntentModelInput {
        [LoadColumn(0)]
        public string Text { get; set; } = "";

        [LoadColumn(1)]
        public string Label { get; set; } = "";
    }

    /// <summary>
    /// Outputmodel som ML.NET returnerer efter prediction.
    /// </summary>
    public class IntentModelOutput {
        [ColumnName("PredictedLabel")]
        public string PredictedLabel { get; set; } = "";
    }
}
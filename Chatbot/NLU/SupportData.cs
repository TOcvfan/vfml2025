using Microsoft.ML.Data;

namespace Chatbot.NLU {
    /// <summary>
    /// Repræsenterer én datapost til supervised intent-træning.
    /// </summary>
    public class SupportData {
        [LoadColumn(0)]
        public string Intent { get; set; }

        [LoadColumn(1)]
        public string Text { get; set; }
    }
}

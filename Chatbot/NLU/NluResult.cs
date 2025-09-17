using System.Collections.Generic;

namespace Chatbot.NLU {
    public class NluResult {
        public string Intent { get; set; }
        public Dictionary<string, string> Entities { get; set; }

        // Til debug eller fallback
        public string RawResponse { get; set; }

        public NluResult() {
            Entities = new Dictionary<string, string>();
        }

        public NluResult(string intent, Dictionary<string, string> entities = null) {
            Intent = intent;
            Entities = entities ?? new Dictionary<string, string>();
        }
    }
}

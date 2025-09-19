using System.Threading.Tasks;

namespace Chatbot.NLU {
    public class CombinedNlu : INluEngine {
        private readonly INluEngine _hybrid;
        private readonly INluEngine _external;

        public CombinedNlu(INluEngine hybrid, INluEngine external) {
            _hybrid = hybrid;
            _external = external;
        }

        public async Task<NluResult> PredictAsync(string input) {
            var result = await _hybrid.PredictAsync(input);

            if (result == null || result.Intent == "Unknown" || string.IsNullOrWhiteSpace(result.Intent)) {
                // fallback til ekstern AI
                result = await _external.PredictAsync(input);
            }

            return result;
        }
    }
}

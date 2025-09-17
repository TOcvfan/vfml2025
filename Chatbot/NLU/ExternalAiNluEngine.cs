using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Chatbot.NLU {
    public class ExternalAiNluEngine : INluEngine {
        private readonly HttpClient _http;

        public ExternalAiNluEngine(HttpClient http) {
            _http = http;
        }

        public async Task<NluResult> PredictAsync(string userInput) {
            // Systemprompt for at tvinge dansk
            string systemPrompt = "Du er en hjælper, der altid svarer på flydende dansk. " +
                                  "Svar aldrig på svensk eller norsk, og brug kun dansk i alle svar.";

            var request = new {
                model = "mistral", // her kan du også bruge "phi3:mini", "llama2", osv.
                prompt = $"{systemPrompt}\n\nBrugerens input: {userInput}"
            };

            var response = await _http.PostAsJsonAsync("http://localhost:11434/api/generate", request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            return new NluResult {
                Intent = "ExternalFallback", // her kan du senere parse json til intent/entities
                Entities = new Dictionary<string, string>(),
                RawResponse = json
            };
        }
    }
}

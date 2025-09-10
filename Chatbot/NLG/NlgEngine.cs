using Chatbot.Models;

namespace Chatbot.NLG;

public interface INlgEngine {
    string GenerateResponse(NluResult result);
}

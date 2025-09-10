using Chatbot.Models;

namespace Chatbot.NLU;

public interface INluEngine {
    NluResult Predict(string input);
}
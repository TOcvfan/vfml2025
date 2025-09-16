using Chatbot.Models;

namespace Chatbot.NLG;

/// <summary>
/// Interface for Natural Language Generation engine.
/// </summary>
public interface INlgEngine {
    /// <summary>
    /// Generates a response string based on the current session state.
    /// </summary>
    /// <param name="state">The session state containing intent and collected entities.</param>
    /// <returns>A string response to display to the user.</returns>
    string GenerateResponse(SessionState state);
}
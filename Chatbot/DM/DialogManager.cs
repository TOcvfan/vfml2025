using System.Collections.Generic;
using Chatbot.Models;
using Chatbot.NLU;

namespace Chatbot.DM;

public class DialogManager {
    private readonly INluEngine _nlu;
    private readonly Dictionary<string, SessionState> _sessions = new();

    public DialogManager(INluEngine nlu) {
        _nlu = nlu;
    }

    public async Task<SessionState> HandleInput(string userInput, string sessionId) {
        if (!_sessions.ContainsKey(sessionId)) {
            var nluResult = await _nlu.PredictAsync(userInput); // <-- nu async
            _sessions[sessionId] = new SessionState {
                CurrentIntent = nluResult.Intent,
                CollectedEntities = new Dictionary<string, string>(nluResult.Entities),
                CurrentStep = "Start"
            };
        }

        var state = _sessions[sessionId];
        var intent = state.CurrentIntent;

        switch (intent) {
            case "Password":
                return HandlePassword(state, userInput);
            case "Login":
                return HandleLogin(state, userInput);
            case "Bruger":
                return HandleBruger(state, userInput);
            default:
                state.CurrentStep = "Unknown";
                return state;
        }
    }

    private SessionState HandlePassword(SessionState state, string userInput) {
        var step = state.CurrentStep;
        var entities = state.CollectedEntities;

        if (step == "Start") {
            if (entities.TryGetValue("Action", out var action)) {
                if (action == "Change") state.CurrentStep = "ForklarSkift";
                else if (action == "Reset") state.CurrentStep = "AskLoggedInd";
            }
        } else if (step == "AskLoggedInd") {
            if (userInput.ToLower() == "ja") state.CurrentStep = "forklarLogudChange";
            else if (userInput.ToLower() == "nej") state.CurrentStep = "forklarChange";
        }

        return state;
    }

    private SessionState HandleLogin(SessionState state, string userInput) {
        var step = state.CurrentStep;

        if (step == "Start") {
            state.CurrentStep = "AskFejlmeddelse";
        } else if (step == "AskFejlmeddelse") {
            if (userInput.ToLower().Contains("forkert password")) {
                state.CurrentStep = "SkiftPassword";
            } else if (userInput.ToLower().Contains("bruger findes ikke")) {
                state.CurrentStep = "NyBruger";
            }
        } else if (step == "NyBruger") {
            state.CurrentStep = "Start";
            state.CurrentIntent = "Bruger";
        }

        return state;
    }

    private SessionState HandleBruger(SessionState state, string userInput) {
        var step = state.CurrentStep;

        if (step == "Start") {
            state.CurrentStep = "AskBrugernavn";
        } else if (step == "AskBrugernavn") {
            state.CollectedEntities["Brugernavn"] = userInput;
            state.CurrentStep = "AskEmail";
        } else if (step == "AskEmail") {
            state.CollectedEntities["Email"] = userInput;
            state.CurrentStep = "AskNavn";
        } else if (step == "AskNavn") {
            state.CollectedEntities["Navn"] = userInput;
            state.CurrentStep = "Confirm";
        }

        return state;
    }
}

using System.Collections.Generic;
/**
* DialogManager constructor
* @param nlu An instance of an NLU engine
* @param nlg An instance of an NLG engine
*/
using Chatbot.Models;
using Chatbot.NLU;

namespace Chatbot.DM;

public class DialogManager {
    private readonly INluEngine _nlu;

    // Sessions per user ID (in real apps: per JWT or user session)
    private readonly Dictionary<string, SessionState> _sessions = new();

    public DialogManager(INluEngine nlu) {
        _nlu = nlu;
    }

    public SessionState HandleInput(string userInput, string sessionId) {
        if (!_sessions.ContainsKey(sessionId)) {
            // Første gang: brug beslutningstræ (intent-analyse)
            var nluResult = _nlu.Predict(userInput);
            _sessions[sessionId] = new SessionState {
                CurrentIntent = nluResult.Intent,
                CollectedEntities = new Dictionary<string, string>(nluResult.Entities),
                CurrentStep = "Start"
            };
        }

        var state = _sessions[sessionId];
        var intent = state.CurrentIntent;

        // STATE MACHINE pr intent
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

        if (step == "start") {
            if (entities["Action"] == "Change") {
                state.CurrentStep = "ForklarSkift";
            } else if (entities["Action"] == "Reset") {
                state.CurrentStep = "AskLoggedInd";
            }
        } else if(step == "AskLoggedInd") {
            if(userInput == "ja") {
                state.CurrentStep = "forklarLogudChange";
            } else if(userInput == "nej") {
                state.CurrentStep = "forklarChange";
            }
        }

        return state;
    }

    private SessionState HandleLogin(SessionState state, string userInput) {
        var step = state.CurrentStep;

        if(step == "start") {
            state.CurrentStep = "AskFejlmeddelse";
        } else if (step == "AskFejlmeddelse") {
            if(userInput == "forkert password"){
                state.CurrentStep = "SkiftPassword";
            } else if(userInput == "bruger findes ikke") {
                state.CurrentStep = "NyBruger";
            }
        } else if(step == "NyBruger") {
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
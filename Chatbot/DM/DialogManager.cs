using Chatbot.NLU;
using Chatbot.NLG;

namespace Chatbot.DM;
/**
* DialogManager class that integrates NLU and NLG engines
*/
public class DialogManager {
    private readonly INluEngine _nlu;
    private readonly INlgEngine _nlg;

    /**
    * DialogManager constructor
    * @param nlu An instance of an NLU engine
    * @param nlg An instance of an NLG engine
*/
    public DialogManager(INluEngine nlu, INlgEngine nlg) {
        _nlu = nlu;
        _nlg = nlg;
    }

    public string HandleInput(string userInput) {
        var nluResult = _nlu.Predict(userInput);
        return _nlg.GenerateResponse(nluResult);
    }
}
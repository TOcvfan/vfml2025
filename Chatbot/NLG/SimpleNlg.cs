using Chatbot.Models;

namespace Chatbot.NLG;

public class SimpleNlg : INlgEngine {
    public string GenerateResponse(NluResult result) {
        return result.Intent switch {
            "BookRoom" =>
      $"Jeg har reserveret et værelse i {(result.Entities.ContainsKey("City") ? result.Entities["City"] : "ukendt by")} til {(result.Entities.ContainsKey("Guests") ? result.Entities["Guests"] : "1")} gæst(er).",

            "CheckFaq" =>
                "Her er ofte stillede spørgsmål: ...",

            "CheckSchedule" =>
                $"Du bad om skema for uge {(result.Entities.ContainsKey("Week") ? result.Entities["Week"] : "ukendt")}.",

            _ =>
                "Beklager, jeg forstod ikke din forespørgsel.",
        };
    }
}
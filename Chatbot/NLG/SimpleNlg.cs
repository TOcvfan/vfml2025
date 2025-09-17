using Chatbot.Models;
using System.Linq;

namespace Chatbot.NLG;

public class SimpleNlg : INlgEngine {
    public string GenerateResponse(SessionState state) {
        var step = state.CurrentStep;
        var entities = state.CollectedEntities;

        // Entity fallbacks
        entities.TryGetValue("Brugernavn", out var brugernavn);
        brugernavn ??= "brugernavn";

        entities.TryGetValue("Email", out var email);
        email ??= "en ukendt dato";

        entities.TryGetValue("Navn", out var navn);
        navn ??= "1";

        return state.CurrentIntent switch {
            "Bruger" => step switch {
                "AskBrugernavn" => "Hvilket brugernavn ønsker du at bruge",
                "AskEmail" => $"Hvilken email ønsker du at bruge {brugernavn}?",
                "AskNavn" => "Hvad er dit navn?",
                "Confirm" => $"Tak! Jeg vil oprette en bruger med disse oplysninger: Brugernavn: {brugernavn}, Email: {email} Navn: {navn}. Password vil blive sendt på en email, husk at skifte den snarest",
                _ => "Jeg forstod ikke din forespørgsel."
            },
            "Password" => step switch {
                "AskLoggedInd" => "Er logget ind?",
                "forklarReset" => $"Din booking med ID er nu aflyst.",
                "forklarChange" => $"Din booking med ID er nu aflyst.",
                "forklarLogudChange" => $"Din booking med ID er nu aflyst.",
                _ => "Beklager, noget gik galt."
            },
            "Login" => step switch {
                "AskFejlmeddelse" => "Hvilken fejlmedelse får du?",
                _ => "Beklager, noget gik galt."
            },
            _ => "Beklager, jeg forstod ikke hvad du mente."
        };
    }
}
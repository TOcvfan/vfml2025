using Chatbot.Models;
using Chatbot.NLG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
namespace Chatbot.NLU;

public class EnhancedNlu : INluEngine {
    private static readonly string[] LoginKeywords = ["login", "loginfejl"];
    private static readonly string[] KodeKeywords = ["password", "kode", "reset", "nulstille", "glemt"];
    private static readonly string[] BrugerKeywords = ["bruger", "opret", "oprette", "opretter"];
    //private static readonly string[] BilKeywords = ["bil", "opret"];

    private static bool ContainsAny(IEnumerable<string> tokens, string[] keywords) =>
        tokens.Any(t => keywords.Contains(t));

    public NluResult Predict(string input) {
        input = input.ToLower();
        var tokens = Tokenize(input);
        var entities = new Dictionary<string, string>();
        string intent = "Unknown";

        // Intent via token match
        if (ContainsAny(tokens, LoginKeywords)) intent = "Login";
        else if (ContainsAny(tokens, KodeKeywords)) intent = "Password";
        else if (ContainsAny(tokens, BrugerKeywords)) intent = "Bruger";

        // Entity: Navn
        var navnMatch = Regex.Match(input, @"(?:navn\s+|jeg hedder\s+)([a-zæøå]+)");
        if (navnMatch.Success)
            entities["Navn"] = navnMatch.Groups[1].Value;

        // Entity: Email
        var emailMatch = Regex.Match(input, @"([\w\.-]+@[\w\.-]+\.\w+)");
        if (emailMatch.Success)
            entities["Email"] = emailMatch.Value;

        // Entity: Brugernavn
        var brugernavnMatch = Regex.Match(input, @"brugernavn\s+([a-zæøå0-9_]+)");
        if (brugernavnMatch.Success)
            entities["Brugernavn"] = brugernavnMatch.Groups[1].Value;

        return new NluResult(intent, entities);
    }

    private static List<string> Tokenize(string input) {
        return input
            .ToLower()
            .Split([' ', ',', '.', '?', '!', ';'], StringSplitOptions.RemoveEmptyEntries)
            .ToList();
    }
}
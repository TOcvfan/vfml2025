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

        // Entity: City
        var cityMatch = Regex.Match(input, @"i\s+([a-zæøå]+)");
        if (cityMatch.Success)
            entities["City"] = cityMatch.Groups[1].Value;

        // Entity: Dates
        var dateMatches = Regex.Matches(input, @"\d{1,2}/\d{1,2}");
        if (dateMatches.Count >= 1) entities["FromDate"] = dateMatches[0].Value;
        if (dateMatches.Count >= 2) entities["ToDate"] = dateMatches[1].Value;

        // Entity: Guests
        var guestMatch = Regex.Match(input, @"til\s+(\d+)");
        if (guestMatch.Success)
            entities["Guests"] = guestMatch.Groups[1].Value;

        return new NluResult(intent, entities);
    }

    private static List<string> Tokenize(string input) {
        return input
            .ToLower()
            .Split([' ', ',', '.', '?', '!', ';'], StringSplitOptions.RemoveEmptyEntries)
            .ToList();
    }
}
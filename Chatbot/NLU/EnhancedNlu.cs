using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Chatbot.NLU {
    public class EnhancedNlu : INluEngine {
        private static readonly string[] LoginKeywords = new[] { "login", "loginfejl" };
        private static readonly string[] KodeKeywords = new[] { "password", "kode" };
        private static readonly string[] BrugerKeywords = new[] { "bruger", "opret", "oprette", "opretter" };
        private static readonly string[] GlemtKodeKeywords = new[] { "ændrer", "ændre", "skifte", "skifter" };
        private static readonly string[] ResetKodeKeywords = new[] { "reset", "mistet", "nulstiller", "glemt" };

        private static bool ContainsAny(IEnumerable<string> tokens, string[] keywords) =>
            tokens.Any(t => keywords.Contains(t));

        public Task<NluResult> PredictAsync(string input) {
            input = input.ToLower();
            var tokens = Tokenize(input);
            var entities = new Dictionary<string, string>();
            string intent = "Unknown";

            if (ContainsAny(tokens, LoginKeywords)) intent = "Login";
            else if (ContainsAny(tokens, KodeKeywords)) {
                if (ContainsAny(tokens, GlemtKodeKeywords)) {
                    intent = "Password";
                    entities["Action"] = "Change";
                } else if (ContainsAny(tokens, ResetKodeKeywords)) {
                    intent = "Password";
                    entities["Action"] = "Reset";
                }
            } else if (ContainsAny(tokens, BrugerKeywords)) intent = "Bruger";

            var navnMatch = Regex.Match(input, @"(?:navn\s+|jeg hedder\s+)([a-zæøå]+)");
            if (navnMatch.Success)
                entities["Navn"] = navnMatch.Groups[1].Value;

            var emailMatch = Regex.Match(input, @"([\w\.-]+@[\w\.-]+\.\w+)");
            if (emailMatch.Success)
                entities["Email"] = emailMatch.Value;

            var brugernavnMatch = Regex.Match(input, @"brugernavn\s+([a-zæøå0-9_]+)");
            if (brugernavnMatch.Success)
                entities["Brugernavn"] = brugernavnMatch.Groups[1].Value;

            return Task.FromResult(new NluResult(intent, entities));
        }

        private static List<string> Tokenize(string input) {
            return input
                .ToLower()
                .Split(new[] { ' ', ',', '.', '?', '!', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();
        }
    }
}

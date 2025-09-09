namespace Chatbot.Models {
    public static class BotLogic {
        public static string Handle(string input) {
            if (string.IsNullOrEmpty(input)) {
                return "Du har ikke skrevet noget!";
            }
            if (input.Contains("hej", StringComparison.OrdinalIgnoreCase)) {
                return "Hej! Hvordan kan jeg hjælpe jer?";
            } else return "Det forstod jeg ikke.";
        }
    }
}
using Chatbot.DM;
using Chatbot.NLU;

class Program {
    static async Task Main(string[] args) {
        Console.WriteLine("Starter chatbot demo...");

        var nlu = new HybridNlu();
        var dialogManager = new DialogManager(nlu);

        string sessionId = Guid.NewGuid().ToString();

        while (true) {
            Console.Write("\nDu: ");
            var userInput = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(userInput) || userInput.ToLower() == "exit")
                break;

            var state = await dialogManager.HandleInput(userInput, sessionId); // <-- await nu

            Console.WriteLine($"[Intent: {state.CurrentIntent}, Step: {state.CurrentStep}]");

            if (state.CollectedEntities.Any()) {
                Console.WriteLine("Entities:");
                foreach (var kv in state.CollectedEntities) {
                    Console.WriteLine($" - {kv.Key}: {kv.Value}");
                }
            }
        }
    }
}

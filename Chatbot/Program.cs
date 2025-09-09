using System;
using Chatbot.Models;

namespace Chatbot {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Chatbot backend running...");
            Console.WriteLine("Skriv 'exit' for at afslutte.\n");

            while (true) {
                Console.Write("Skriv til botten: ");
                string userInput = Console.ReadLine();

                if (string.Equals(userInput, "exit", StringComparison.OrdinalIgnoreCase)) {
                    Console.WriteLine("Programmet afsluttes...");
                    break;
                }

                string svar = BotLogic.Handle(userInput);
                Console.WriteLine("Bot: " + svar);
            }
        }
    }
}
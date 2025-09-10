using System.Collections.Generic;

namespace Chatbot.Models;

public record NluResult(string Intent, Dictionary<string, string> Entities);
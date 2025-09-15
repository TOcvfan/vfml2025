namespace Chatbot.NLU;
using Microsoft.ML.Data;
// Denne klasse repræsenterer dine træningsdata (tekst + ønsket intent)
public class SupportData {
    [LoadColumn(0)]
    public string Text { get; set; } = "";

    [LoadColumn(1)]
    public string Intent { get; set; } = "";
}
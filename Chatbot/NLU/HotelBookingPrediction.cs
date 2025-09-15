namespace Chatbot.NLU;
using Microsoft.ML.Data;
// Denne klasse bruges til at modtage ML.NETs output
public class HotelBookingPrediction {
    [ColumnName("PredictedLabel")]
    public string PredictedIntent { get; set; } = "";
}
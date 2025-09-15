using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
namespace Chatbot.NLU;

/**
* Hybrid NLU engine combining supervised learning with unsupervised fallback.
* Supervised: Multiclass classification using labeled intent data.
* Unsupervised: KMeans clustering for fallback when confidence is low.
*/


public class HybridNlu : INluEngine {
    private readonly MLContext _mlContext;
    private readonly ITransformer _trainedModel;
    private readonly PredictionEngine<HotelBookingData, HotelBookingPrediction> _predictor;

    private readonly IDataView _unsupervisedData;
    private readonly ITransformer _kmeansModel;
    private readonly Dictionary<uint, string> _clusterToIntent;

    /**
        * Constructor initializes and trains both supervised and unsupervised models.
        * Supervised model is trained on labeled intent data from "hotel_intents.csv".
        * Unsupervised KMeans model is also trained on the same data for fallback purposes.
        * Cluster IDs are heuristically mapped to intents for fallback explanations.
*/
    public HybridNlu() {

        _mlContext = new MLContext();

        // Supervised pipeline
        //Brug korrekt absolut sti
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "hotel_intents.csv");

        // Fejlbesked hvis fil ikke findes
        if (!File.Exists(path))
            throw new FileNotFoundException($"CSV-filen blev ikke fundet: {path}");

        // Load supervised træningsdata
        var data = _mlContext.Data.LoadFromTextFile<HotelBookingData>(
            path: path,
            hasHeader: true,
            separatorChar: ',');

        var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features", nameof(HotelBookingData.Text))
            .Append(_mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(HotelBookingData.Intent)))
            .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features"))
            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        _trainedModel = pipeline.Fit(data);
        _predictor = _mlContext.Model.CreatePredictionEngine<HotelBookingData, HotelBookingPrediction>(_trainedModel);

        // Unsupervised KMeans fallback
        _unsupervisedData = _mlContext.Data.LoadFromTextFile<HotelBookingData>(
     path: path, hasHeader: true, separatorChar: ',');
        var kmeansPipeline = _mlContext.Transforms.Text.FeaturizeText("Features", nameof(HotelBookingData.Text))
            .Append(_mlContext.Clustering.Trainers.KMeans("Features", numberOfClusters: 3));

        _kmeansModel = kmeansPipeline.Fit(_unsupervisedData);

        // Map cluster IDs to intent heuristically (for fallback explanation)
        _clusterToIntent = new Dictionary<uint, string>
        {
            { 0, "BookRoom" },
            { 1, "ChangeBooking" },
            { 2, "CancelBooking" }
        };
    }

    public NluResult Predict(string input) {
        var prediction = _predictor.Predict(new HotelBookingData { Text = input });

        if (!string.IsNullOrEmpty(prediction.PredictedIntent)) {
            return new NluResult(prediction.PredictedIntent, ExtractEntities(input));
        }

        // Fallback til clustering
        var vectorized = _mlContext.Data.LoadFromEnumerable(new[] {
            new HotelBookingData { Text = input }
        });

        var transformed = _kmeansModel.Transform(vectorized);
        var clusterColumn = transformed.GetColumn<uint>("PredictedLabel").FirstOrDefault();
        var fallbackIntent = _clusterToIntent.GetValueOrDefault(clusterColumn, "Unknown");

        return new NluResult(fallbackIntent, ExtractEntities(input));
    }

    private Dictionary<string, string> ExtractEntities(string input) {
        var entities = new Dictionary<string, string>();

        // Gæster
        var guests = Regex.Match(input, @"(\d+)\s*gæster?");
        if (guests.Success) entities["Guests"] = guests.Groups[1].Value;

        // By
        var city = Regex.Match(input, @"i\s+([a-zA-ZæøåÆØÅ]+)");
        if (city.Success) entities["City"] = city.Groups[1].Value;

        // Datoer
        var dates = Regex.Matches(input, @"\d{1,2}/\d{1,2}");
        if (dates.Count > 0) entities["FromDate"] = dates[0].Value;
        if (dates.Count > 1) entities["ToDate"] = dates[1].Value;

        return entities;
    }
}
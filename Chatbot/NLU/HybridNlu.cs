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
    private readonly PredictionEngine<SupportData, SupportPrediction> _predictor;

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
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "support_intents.csv");

        // Fejlbesked hvis fil ikke findes
        if (!File.Exists(path))
            throw new FileNotFoundException($"CSV-filen blev ikke fundet: {path}");

        // Load supervised træningsdata
        var data = _mlContext.Data.LoadFromTextFile<SupportData>(
            path: path,
            hasHeader: true,
            separatorChar: ',');

        var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features", nameof(SupportData.Text))
            .Append(_mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(SupportData.Intent)))
            .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features"))
            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        _trainedModel = pipeline.Fit(data);
        _predictor = _mlContext.Model.CreatePredictionEngine<SupportData, SupportPrediction>(_trainedModel);

        // Unsupervised KMeans fallback
        _unsupervisedData = _mlContext.Data.LoadFromTextFile<SupportData>(
     path: path, hasHeader: true, separatorChar: ',');
        var kmeansPipeline = _mlContext.Transforms.Text.FeaturizeText("Features", nameof(SupportData.Text))
            .Append(_mlContext.Clustering.Trainers.KMeans("Features", numberOfClusters: 3));

        _kmeansModel = kmeansPipeline.Fit(_unsupervisedData);

        // Map cluster IDs to intent heuristically (for fallback explanation)
        _clusterToIntent = new Dictionary<uint, string>
        {
            { 0, "Login" },
            { 1, "Password" },
            { 2, "Bruger" }
        };
    }

    public NluResult Predict(string input) {
        var prediction = _predictor.Predict(new SupportData { Text = input });

        if (!string.IsNullOrEmpty(prediction.PredictedIntent)) {
            return new NluResult(prediction.PredictedIntent, ExtractEntities(input));
        }

        // Fallback til clustering
        var vectorized = _mlContext.Data.LoadFromEnumerable(new[] {
            new SupportData { Text = input }
        });

        var transformed = _kmeansModel.Transform(vectorized);
        var clusterColumn = transformed.GetColumn<uint>("PredictedLabel").FirstOrDefault();
        var fallbackIntent = _clusterToIntent.GetValueOrDefault(clusterColumn, "Unknown");

        return new NluResult(fallbackIntent, ExtractEntities(input));
    }

    private Dictionary<string, string> ExtractEntities(string input) {
        var entities = new Dictionary<string, string>();
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
        

        return entities;
    }
}
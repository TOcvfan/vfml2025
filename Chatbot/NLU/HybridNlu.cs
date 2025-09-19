using Microsoft.ML;
using Microsoft.ML.Data;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace Chatbot.NLU;

/// <summary>
/// Hybrid NLU engine combining supervised ML.NET classification with KMeans fallback.
/// Ekstern AI er deaktiveret – denne version bruger kun lokale ML-modeller.
/// </summary>
public class HybridNlu : INluEngine {
    private readonly MLContext _mlContext;
    private readonly ITransformer _trainedModel;
    private readonly PredictionEngine<IntentModelInput, IntentModelOutput> _predictor;

    private readonly IDataView _unsupervisedData;
    private readonly ITransformer _kmeansModel;
    private readonly Dictionary<uint, string> _clusterToIntent;

    public HybridNlu() {
        _mlContext = new MLContext();

        // Brug absolut sti til træningsdata
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "support_intents.csv");

        if (!File.Exists(path))
            throw new FileNotFoundException($"CSV-filen blev ikke fundet: {path}");

        // Supervised træning
        var data = _mlContext.Data.LoadFromTextFile<IntentModelInput>(
            path: path,
            hasHeader: true,
            separatorChar: ',');

        /*var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features", nameof(IntentModelInput.Text))
            .Append(_mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(IntentModelInput.Label)))
            .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features"))
            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));*/
        var pipeline = _mlContext.Transforms.Conversion
    .MapValueToKey("Label", nameof(IntentModelInput.Label))
    .Append(_mlContext.Transforms.Text.FeaturizeText("Features", nameof(IntentModelInput.Text)))
    .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features"))
    .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        _trainedModel = pipeline.Fit(data);
        _predictor = _mlContext.Model.CreatePredictionEngine<IntentModelInput, IntentModelOutput>(_trainedModel);

        // Unsupervised fallback (KMeans clustering)
        _unsupervisedData = data;
        var kmeansPipeline = _mlContext.Transforms.Text.FeaturizeText("Features", nameof(IntentModelInput.Text))
            .Append(_mlContext.Clustering.Trainers.KMeans("Features", numberOfClusters: 3));

        _kmeansModel = kmeansPipeline.Fit(_unsupervisedData);

        // Map cluster IDs til intents (heuristik)
        _clusterToIntent = new Dictionary<uint, string>
        {
            { 0, "Login" },
            { 1, "Password" },
            { 2, "Bruger" }
        };
    }

    public async Task<NluResult> PredictAsync(string input) {
        // Supervised prediction
        var prediction = _predictor.Predict(new IntentModelInput { Text = input });

        // Hvis input er for kort eller small talk → giv Unknown
        if (string.IsNullOrWhiteSpace(input) || input.Length < 3 ||
            new[] { "hej", "tak", "goddag", "hello" }.Contains(input.ToLower())) {
            return new NluResult("Unknown", new Dictionary<string, string>());
        }

        // Fallback: KMeans clustering
        var vectorized = _mlContext.Data.LoadFromEnumerable(new[]
        {
        new IntentModelInput { Text = input }
    });

        var transformed = _kmeansModel.Transform(vectorized);
        var clusterColumn = transformed.GetColumn<uint>("PredictedLabel").FirstOrDefault();
        var fallbackIntent = _clusterToIntent.GetValueOrDefault(clusterColumn, "Unknown");

        // Hvis fallback er tvivlsomt → Unknown
        if (fallbackIntent == "Unknown") {
            return new NluResult("Unknown", new Dictionary<string, string>());
        }

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

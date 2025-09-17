using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Chatbot.NLU {
    /// <summary>
    /// Hybrid NLU engine: supervised ML.NET multiclass classifier + KMeans fallback.
    /// </summary>
    public class HybridNlu : INluEngine {
        private readonly MLContext _mlContext;
        private readonly ITransformer _trainedModel;
        private readonly PredictionEngine<SupportData, SupportPrediction> _predictor;

        private readonly ITransformer _kmeansModel;
        private readonly PredictionEngine<SupportData, KMeansPrediction> _kmeansPredictor;

        private readonly Dictionary<uint, string> _clusterToIntent;

        /// <param name="csvRelativePath">Relativ sti fra AppContext.BaseDirectory til CSV (default: Data/support_intents.csv)</param>
        public HybridNlu(string csvRelativePath = "Data/support_intents.csv") {
            _mlContext = new MLContext();

            var path = Path.Combine(AppContext.BaseDirectory, csvRelativePath);
            if (!File.Exists(path))
                throw new FileNotFoundException($"CSV-filen blev ikke fundet: {path}");

            // Load training data
            var data = _mlContext.Data.LoadFromTextFile<SupportData>(path, hasHeader: true, separatorChar: ',');

            // Supervised pipeline: text -> features -> label-key -> trainer -> map key back to label string
            var pipeline = _mlContext.Transforms.Text
                    .FeaturizeText(outputColumnName: "Features", inputColumnName: nameof(SupportData.Text))
                .Append(_mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "Label", inputColumnName: nameof(SupportData.Intent)))
                .Append(_mlContext.MulticlassClassification.Trainers
                    .SdcaMaximumEntropy(labelColumnName: "Label", featureColumnName: "Features"))
                .Append(_mlContext.Transforms.Conversion.MapKeyToValue(outputColumnName: "PredictedLabel", inputColumnName: "PredictedLabel"));

            _trainedModel = pipeline.Fit(data);
            _predictor = _mlContext.Model.CreatePredictionEngine<SupportData, SupportPrediction>(_trainedModel);

            // KMeans unsupervised fallback (same text -> features -> kmeans)
            var kmeansPipeline = _mlContext.Transforms.Text
                    .FeaturizeText(outputColumnName: "Features", inputColumnName: nameof(SupportData.Text))
                .Append(_mlContext.Clustering.Trainers.KMeans(featureColumnName: "Features", numberOfClusters: 3));

            _kmeansModel = kmeansPipeline.Fit(data);
            _kmeansPredictor = _mlContext.Model.CreatePredictionEngine<SupportData, KMeansPrediction>(_kmeansModel);

            // Heuristisk mapping af cluster-id -> intent (tilpas efter dit datasæt)
            _clusterToIntent = new Dictionary<uint, string>
            {
                { 0, "Login" },
                { 1, "Password" },
                { 2, "Bruger" }
            };
        }

        public Task<NluResult> PredictAsync(string input) {
            if (string.IsNullOrWhiteSpace(input))
                return Task.FromResult(new NluResult("Unknown"));

            // Supervised prediction
            var pred = _predictor.Predict(new SupportData { Text = input });
            var predictedIntent = pred?.PredictedIntent;

            if (!string.IsNullOrWhiteSpace(predictedIntent))
                return Task.FromResult(new NluResult(predictedIntent, ExtractEntities(input)));

            // Hvis supervised ikke gav et brugbart resultat -> fallback til KMeans
            var kRes = _kmeansPredictor.Predict(new SupportData { Text = input });
            var clusterId = kRes?.PredictedLabel ?? 0u;
            var fallbackIntent = _clusterToIntent.TryGetValue(clusterId, out var mapped) ? mapped : "Unknown";

            return Task.FromResult(new NluResult(fallbackIntent, ExtractEntities(input)));
        }

        private Dictionary<string, string> ExtractEntities(string input) {
            var entities = new Dictionary<string, string>();

            var navnMatch = Regex.Match(input, @"(?:navn\s+|jeg hedder\s+)([a-zæøå]+)", RegexOptions.IgnoreCase);
            if (navnMatch.Success) entities["Navn"] = navnMatch.Groups[1].Value;

            var emailMatch = Regex.Match(input, @"([\w\.-]+@[\w\.-]+\.\w+)");
            if (emailMatch.Success) entities["Email"] = emailMatch.Value;

            var brugernavnMatch = Regex.Match(input, @"brugernavn\s+([a-zæøå0-9_]+)", RegexOptions.IgnoreCase);
            if (brugernavnMatch.Success) entities["Brugernavn"] = brugernavnMatch.Groups[1].Value;

            return entities;
        }

        // ML.NET output for KMeans
        private class KMeansPrediction {
            [ColumnName("PredictedLabel")]
            public uint PredictedLabel { get; set; }

            // Score er normalt ikke nødvendig her, men kan være nyttig til debugging
            public float[] Score { get; set; }
        }
    }
}

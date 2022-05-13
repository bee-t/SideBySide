using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;

using Microsoft.ML;
using Microsoft.ML.Data;
using static Microsoft.ML.DataOperationsCatalog;
using System.IO;

namespace SideBySide
{
    public partial class MainPage : ContentPage
    {
        string Site = "";
        public MainPage()
        {
            InitializeComponent();

            string website = $"cnn.com";
            string keyword = $"Ukraine";

            string google = $"https://www.google.com/search?q=site%3A{website}+%22{keyword}";


            this.Site = $"site:{website} \"{keyword}\"";

            Webview.Source = new Uri(google, UriKind.Absolute);

            HtmlWeb web = new HtmlWeb();

            var htmlDoc = web.Load(google);

            var htmlNode = htmlDoc.DocumentNode.SelectSingleNode("//body");
            IEnumerable<string> parsedHtml = this.HtmlAgilityPackParse(htmlNode.OuterHtml);

            string URLs = null;

            foreach (var url in parsedHtml)
            {
                if (url.Contains("http"))
                {
                    URLs += url + "\n\n";
                }
            }

            Debug.WriteLine(URLs);


            //this.Entry.Text = URLs;

            //PerformSentimentAnalysis();

            var htmlDocFromURL = web.Load(parsedHtml.ElementAt(1));

            var htmlNodeFromURL = htmlDocFromURL.DocumentNode.SelectSingleNode("//body").InnerText;

            string[] splittedContent = htmlNodeFromURL.Split('.');
            int count = 0;

            foreach(var content in splittedContent)
            {
                if (content.Contains(keyword))
                {
                    splittedContent[count] = content;
                }
                count++;
            }

            // random 8 sentences
            int numberOfSentences = 8;
            string[] SelectedSentences = new string[numberOfSentences];
            Random random = new Random();
            
            // select randomly 5 sentences from the list of sentences we have
            for(int i = 0; i < numberOfSentences; i++)
            {
                if(random.Next(splittedContent.Length) < splittedContent.Length)
                {
                    SelectedSentences[i] = splittedContent[random.Next(splittedContent.Length)]; 
                }
            }

            // getting their sentiments
            List<SentimentData> sentiments = new List<SentimentData>();

            foreach (var sentences in SelectedSentences)
            {
                sentiments.Add( new SentimentData { SentimentText=sentences });
            }

            PerformSentimentAnalysis(sentiments);
            //var ss = ExtractText(htmlNodeFromURL.InnerHtml);
        }

        public static string ExtractText(string html)
        {
            if (html == null)
            {
                throw new ArgumentNullException("html");
            }

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            var chunks = new List<string>();

            foreach (var item in doc.DocumentNode.DescendantNodesAndSelf())
            {
                if (item.NodeType == HtmlNodeType.Text)
                {
                    if (item.InnerText.Trim() != "")
                    {
                        chunks.Add(item.InnerText.Trim());
                    }
                }
            }
            return String.Join(" ", chunks);
        }

        private void PerformSentimentAnalysis(IEnumerable<SentimentData> sentiments = null)
        {
            MLContext mlContext = new MLContext();
            TrainTestData splitDataView = LoadData(mlContext);

            // build - train model
            ITransformer model = BuildAndTrainModel(mlContext, splitDataView.TrainSet);
            Evaluate(mlContext, model, splitDataView.TestSet);
            UseModelWithSingleItem(mlContext, model);
            MySentimentAnalysis(mlContext, model, sentiments);

        }

        private TrainTestData LoadData(MLContext mlContext)
        {
            string _dataPath = Path.Combine(Environment.CurrentDirectory, "Data", "yelp_labelled.txt");
            IDataView dataView = mlContext.Data.LoadFromTextFile<SentimentData>(_dataPath, hasHeader: false);
            TrainTestData splitDataView = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
            return splitDataView;
        }

        private ITransformer BuildAndTrainModel(MLContext mlContext, IDataView splitTrainSet)
        {
            //Extract and transform the data
            var estimator = mlContext.Transforms.Text.
                FeaturizeText(
                outputColumnName: "Features", 
                inputColumnName: nameof(SentimentData.SentimentText))
                //Add a learning algorithm
                .Append(
                mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
                    labelColumnName: "Label", 
                    featureColumnName: "Features"));

            //Train the model
            Debug.WriteLine("=============== Create and Train the Model ===============");
            var model = estimator.Fit(splitTrainSet);
            Debug.WriteLine("=============== End of training ===============");
            Debug.WriteLine("\n");

            //Return the model trained to use for evaluation
            return model;
        }

        void Evaluate(MLContext mlContext, ITransformer model, IDataView splitTestSet)
        {
            Debug.WriteLine("=============== Evaluating Model accuracy with Test data===============");
            IDataView predictions = model.Transform(splitTestSet);

            CalibratedBinaryClassificationMetrics metrics = mlContext.BinaryClassification.Evaluate(predictions, "Label");
            Debug.WriteLine("\n");
            Debug.WriteLine("Model quality metrics evaluation");
            Debug.WriteLine("--------------------------------");
            Debug.WriteLine($"Accuracy: {metrics.Accuracy:P2}");
            Debug.WriteLine($"Auc: {metrics.AreaUnderRocCurve:P2}");
            Debug.WriteLine($"F1Score: {metrics.F1Score:P2}");
            Debug.WriteLine("=============== End of model evaluation ===============");
        }

        void UseModelWithSingleItem(MLContext mlContext, ITransformer model)
        {
            PredictionEngine<SentimentData, SentimentPrediction> predictionFunction = mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(model);

            SentimentData sampleStatement = new SentimentData
            {
                SentimentText = "This was a very bad steak"
            };

            var resultPrediction = predictionFunction.Predict(sampleStatement);

            Debug.WriteLine("\n");
            Debug.WriteLine("=============== Prediction Test of model with a single sample and test dataset ===============");

            Debug.WriteLine("\n");
            Debug.WriteLine($"Sentiment: {resultPrediction.SentimentText} | Prediction: {(Convert.ToBoolean(resultPrediction.Prediction) ? "Positive" : "Negative")} | Probability: {resultPrediction.Probability} ");

            Debug.WriteLine("=============== End of Predictions ===============");
            Debug.WriteLine("\n");
        }

        // run analysis on batched items
        void MySentimentAnalysis(MLContext mlContext, ITransformer model, IEnumerable<SentimentData> sentiments = null)
        {
            if(sentiments == null)
            {
                sentiments = new[]
                {
                    new SentimentData
                    {
                        SentimentText = "This was a horrible meal and I did not like it, but I will waste it as it is not good"
                    },
                    new SentimentData
                    {
                        SentimentText = "I love this spaghetti and Icecream and all of the people who are present here as I am very happy today"
                    }
                };
            }

            IDataView batchComments = mlContext.Data.LoadFromEnumerable(sentiments);

            IDataView predictions = model.Transform(batchComments);

            // Use model to predict whether comment data is Positive (1) or Negative (0).
            IEnumerable<SentimentPrediction> predictedResults = mlContext.Data.CreateEnumerable<SentimentPrediction>(predictions, reuseRowObject: false);
            Debug.WriteLine("\n");

            Debug.WriteLine("=============== Prediction Test of loaded model with multiple samples ===============");
            foreach (SentimentPrediction prediction in predictedResults)
            {
                Debug.WriteLine($"Sentiment: {prediction.SentimentText} | Prediction: {(Convert.ToBoolean(prediction.Prediction) ? "Positive" : "Negative")} | Probability: {prediction.Probability} ");
            }
            Debug.WriteLine("=============== End of predictions ===============");
        }

        // get all urls from html node
        public IEnumerable<string> HtmlAgilityPackParse(string html)
        {
            HtmlDocument htmlSnippet = new HtmlDocument();
            htmlSnippet.LoadHtml(html);

            List<string> hrefTags = new List<string>();

            foreach (HtmlNode link in htmlSnippet.DocumentNode.SelectNodes("//a[@href]"))
            {
                HtmlAttribute att = link.Attributes["href"];

                if (att.Value.Contains("http"))
                {
                    hrefTags.Add(att.Value);
                }
            }

            return hrefTags;
        }
    }
}

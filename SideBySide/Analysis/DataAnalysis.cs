using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using static Microsoft.ML.DataOperationsCatalog;

namespace SideBySide.Analysis
{
    public class DataAnalysis
    {
        public List<string> PredictionResult { get; set; }
        public ITransformer PredictionModel { get; set; }
        public MLContext MLContext { get; set; }
        public TrainTestData TrainTestData { get; set; }

        /// <summary>
        /// When this object is created it initializes model, context, trainandtest data objects
        /// </summary>
        public DataAnalysis()
        {
            this.PerformSentimentAnalysis();
        }

        private void PerformSentimentAnalysis(IEnumerable<SentimentData> sentiments = null)
        {
            MLContext = new MLContext();

            // load data
            TrainTestData = LoadData(MLContext);

            // build - train model
            this.PredictionModel = BuildAndTrainModel(MLContext, TrainTestData.TrainSet);
            Evaluate(MLContext, PredictionModel, TrainTestData.TestSet);
            UseModelWithSingleItem(MLContext, PredictionModel);
            AnalyseBatchOfData(MLContext, PredictionModel, sentiments);
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

        public void Evaluate(MLContext mlContext, ITransformer model, IDataView splitTestSet)
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

        /// <summary>
        /// returns string with prediction information
        /// requires context and model
        /// </summary>
        /// <param name="mlContext"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public string UseModelWithSingleItem(MLContext mlContext, ITransformer model)
        {
            PredictionEngine<SentimentData, SentimentPrediction> predictionFunction = mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(model);

            SentimentData sampleStatement = new SentimentData
            {
                SentimentText = "This was a very bad steak"
            };

            SentimentPrediction resultPrediction = predictionFunction.Predict(sampleStatement);

            Debug.WriteLine("\n");
            Debug.WriteLine("=============== Prediction Test of model with a single sample and test dataset ===============");

            Debug.WriteLine("\n");
            Debug.WriteLine($"Sentiment: {resultPrediction.SentimentText} | Prediction: {(Convert.ToBoolean(resultPrediction.Prediction) ? "Positive" : "Negative")} | Probability: {resultPrediction.Probability} ");

            Debug.WriteLine("=============== End of Predictions ===============");
            Debug.WriteLine("\n");

            return $"Sentiment: {resultPrediction.SentimentText} | Prediction: {(Convert.ToBoolean(resultPrediction.Prediction) ? "Positive" : "Negative")} | Probability: {resultPrediction.Probability} ";
        }

        /// <summary>
        /// This method needs batches of sentiments and it executes the prediction on them
        /// </summary>
        /// <param name="mlContext"></param>
        /// <param name="model"></param>
        /// <param name="sentiments"></param>
        public void AnalyseBatchOfData(MLContext mlContext, ITransformer model, IEnumerable<SentimentData> sentiments = null)
        {
            if (sentiments == null)
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

            PredictionResult = new List<string>();

            Debug.WriteLine("=============== Prediction Test of loaded model with multiple samples ===============");
            foreach (SentimentPrediction prediction in predictedResults)
            {
                PredictionResult.Add($"Sentiment: {prediction.SentimentText} | Prediction: {(Convert.ToBoolean(prediction.Prediction) ? "Positive" : "Negative")} | Probability: {prediction.Probability}");
                Debug.WriteLine($"Sentiment: {prediction.SentimentText} | Prediction: {(Convert.ToBoolean(prediction.Prediction) ? "Positive" : "Negative")} | Probability: {prediction.Probability} ");
            }
            Debug.WriteLine("=============== End of predictions ===============");
        }
    }
}

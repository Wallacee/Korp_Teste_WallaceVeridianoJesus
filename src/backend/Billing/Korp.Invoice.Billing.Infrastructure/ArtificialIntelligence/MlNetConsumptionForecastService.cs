using Korp.Invoice.Billing.Application.ExternalServices.AI;
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;
namespace Korp.Invoice.Billing.Infrastructure.ArtificialIntelligence;

public sealed class MlNetConsumptionForecastService : IConsumptionForecastService
{
    private
    const int MinimumHistorySize = 14;
    public ConsumptionForecastResult Predict(IReadOnlyCollection<float> history, int forecastDays)
    {
        if (history.Count < MinimumHistorySize)
        {
            return new ConsumptionForecastResult(
                [], false);
        }
        var mlContext = new MLContext(seed: 42);
        var data = history.Select(value => new ConsumptionData { Value = value }).ToList();
        var dataView = mlContext.Data.LoadFromEnumerable(data);
        var windowSize = Math.Min(7, Math.Max(2, history.Count / 4));
        var seriesLength = history.Count;
        var trainSize = history.Count;

        var pipeline = mlContext.Forecasting
            .ForecastBySsa(outputColumnName: nameof(ConsumptionPrediction.Forecast)
            , inputColumnName: nameof(ConsumptionData.Value)
            , windowSize: windowSize
            , seriesLength: seriesLength
            , trainSize: trainSize
            , horizon: forecastDays
            , confidenceLevel: 0.95f
            , confidenceLowerBoundColumn: nameof(ConsumptionPrediction.LowerBound)
            , confidenceUpperBoundColumn: nameof(ConsumptionPrediction.UpperBound));

        var model = pipeline.Fit(dataView);
        var engine = model.CreateTimeSeriesEngine<ConsumptionData, ConsumptionPrediction>(mlContext);
        var prediction = engine.Predict();
        var forecast = prediction.Forecast.Select(value => Math.Max(0, value)).ToArray();

        return new ConsumptionForecastResult(forecast, true);
    }
    private sealed class ConsumptionData
    {
        public float Value { get; set; }
    }
    private sealed class ConsumptionPrediction
    {
        public float[] Forecast { get; set; } = [];
        public float[] LowerBound { get; set; } = [];
        public float[] UpperBound { get; set; } = [];
    }
}

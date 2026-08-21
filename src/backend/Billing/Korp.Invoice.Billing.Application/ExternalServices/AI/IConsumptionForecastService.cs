namespace Korp.Invoice.Billing.Application.ExternalServices.AI;

public interface IConsumptionForecastService
{
    ConsumptionForecastResult Predict(IReadOnlyCollection<float> history, int forecastDays);
}

namespace ExpenseService.Application.Abstractions;

public interface IExchangeRateService
{
    Task<decimal> GetExchangeRateAsync(string currencyCode, CancellationToken cancellationToken);
}

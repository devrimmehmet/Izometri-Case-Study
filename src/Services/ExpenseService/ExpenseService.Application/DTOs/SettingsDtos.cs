namespace ExpenseService.Application.DTOs;

public sealed record ExchangeRateResponse(
    decimal? FixedUsdRate,
    decimal? FixedEurRate,
    decimal CurrentUsdRate,
    decimal CurrentEurRate);

public sealed record UpdateRatesRequest(decimal? FixedUsdRate, decimal? FixedEurRate);

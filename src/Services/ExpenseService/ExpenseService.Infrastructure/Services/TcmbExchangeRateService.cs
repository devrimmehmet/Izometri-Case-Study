using System.Globalization;
using System.Xml.Linq;
using ExpenseService.Application.Abstractions;
using ExpenseService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ExpenseService.Infrastructure.Services;

public sealed class TcmbExchangeRateService : IExchangeRateService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TcmbExchangeRateService> _logger;
    private readonly ICurrentUserContext _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private const string TcmbUrl = "https://www.tcmb.gov.tr/kurlar/today.xml";

    public TcmbExchangeRateService(HttpClient httpClient, ILogger<TcmbExchangeRateService> logger, ICurrentUserContext currentUser, IUnitOfWork unitOfWork)
    {
        _httpClient = httpClient;
        _logger = logger;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<decimal> GetExchangeRateAsync(string currencyCode, CancellationToken cancellationToken)
    {
        if (string.Equals(currencyCode, "TRY", StringComparison.OrdinalIgnoreCase))
        {
            return 1m;
        }

        var tenantId = _currentUser.TenantId;
        if (tenantId != Guid.Empty)
        {
            var tenant = await _unitOfWork.Repository<Tenant>().Query().FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);
            if (tenant != null)
            {
                if (string.Equals(currencyCode, "USD", StringComparison.OrdinalIgnoreCase) && tenant.FixedUsdRate.HasValue)
                    return tenant.FixedUsdRate.Value;
                if (string.Equals(currencyCode, "EUR", StringComparison.OrdinalIgnoreCase) && tenant.FixedEurRate.HasValue)
                    return tenant.FixedEurRate.Value;
            }
        }

        try
        {
            var response = await _httpClient.GetStringAsync(TcmbUrl, cancellationToken);
            var doc = XDocument.Parse(response);

            var currencyElement = doc.Descendants("Currency")
                .FirstOrDefault(e => e.Attribute("CurrencyCode")?.Value == currencyCode);

            if (currencyElement != null)
            {
                var rateStr = currencyElement.Element("ForexSelling")?.Value;
                if (decimal.TryParse(rateStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var rate))
                {
                    return rate;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch exchange rate from TCMB for {CurrencyCode}", currencyCode);
        }

        // Fallback rates if API is unreachable or parsing fails
        return currencyCode switch
        {
            "USD" => 33.5m,
            "EUR" => 36.2m,
            _ => 1m
        };
    }
}

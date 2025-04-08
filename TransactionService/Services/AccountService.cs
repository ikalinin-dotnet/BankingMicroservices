using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using TransactionService.DTOs;

namespace TransactionService.Services;

public class AccountService : IAccountService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AccountService> _logger;

    public AccountService(HttpClient httpClient, ILogger<AccountService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AccountDto?> GetAccountByIdAsync(Guid accountId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<AccountDto>($"api/accounts/{accountId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting account with ID: {AccountId}", accountId);
            return null;
        }
    }

    public async Task<AccountDto?> GetAccountByNumberAsync(string accountNumber)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<AccountDto>($"api/accounts/number/{accountNumber}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting account with number: {AccountNumber}", accountNumber);
            return null;
        }
    }

    public async Task<bool> UpdateAccountBalanceAsync(Guid accountId, decimal amount)
    {
        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(new { Amount = amount }),
                Encoding.UTF8,
                "application/json");

            var endpoint = amount >= 0 
                ? $"api/accounts/{accountId}/deposit" 
                : $"api/accounts/{accountId}/withdraw";

            if (amount < 0)
            {
                // Convert to positive for withdrawal
                content = new StringContent(
                    JsonSerializer.Serialize(new { Amount = Math.Abs(amount) }),
                    Encoding.UTF8,
                    "application/json");
            }

            var response = await _httpClient.PutAsync(endpoint, content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating balance for account ID: {AccountId}", accountId);
            return false;
        }
    }
}
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Data;

namespace RazorPagesMovie.Services;

public class ContactMessageNotificationService : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ContactMessageNotificationService> _logger;

    public ContactMessageNotificationService(
        IServiceScopeFactory scopeFactory,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<ContactMessageNotificationService> logger)
    {
        _scopeFactory = scopeFactory;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckForNewContactMessagesAsync(stoppingToken);
            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task CheckForNewContactMessagesAsync(CancellationToken stoppingToken)
    {
        var botToken = _configuration["Telegram:BotToken"];
        var chatId = _configuration["Telegram:ChatId"];

        if (string.IsNullOrWhiteSpace(botToken) || string.IsNullOrWhiteSpace(chatId))
        {
            _logger.LogWarning("Telegram:BotToken oder Telegram:ChatId ist nicht konfiguriert – Benachrichtigung wird übersprungen.");
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RazorPagesMovieContext>();

        var neueEintraege = await context.ContactMessage
            .Where(c => !c.NotificationSent)
            .OrderBy(c => c.SentAt)
            .ToListAsync(stoppingToken);

        if (neueEintraege.Count == 0)
        {
            return;
        }

        var httpClient = _httpClientFactory.CreateClient();
        var sendMessageUrl = $"https://api.telegram.org/bot{botToken}/sendMessage";

        foreach (var eintrag in neueEintraege)
        {
            var text = $"Neue Kontaktanfrage von {eintrag.Name}\n"
                + $"E-Mail: {eintrag.Email}\n"
                + (string.IsNullOrWhiteSpace(eintrag.Phone) ? "" : $"Telefon: {eintrag.Phone}\n")
                + (string.IsNullOrWhiteSpace(eintrag.Subject) ? "" : $"Betreff: {eintrag.Subject}\n")
                + $"\n{eintrag.Message}";

            try
            {
                var response = await httpClient.PostAsJsonAsync(
                    sendMessageUrl,
                    new { chat_id = chatId, text },
                    stoppingToken);

                if (response.IsSuccessStatusCode)
                {
                    eintrag.NotificationSent = true;
                    await context.SaveChangesAsync(stoppingToken);
                }
                else
                {
                    _logger.LogWarning("Telegram-Versand fehlgeschlagen für ContactMessage {Id}: {StatusCode}", eintrag.Id, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Telegram-Versand fehlgeschlagen für ContactMessage {Id}", eintrag.Id);
            }
        }
    }
}
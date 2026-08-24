using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Data;

namespace RazorPagesMovie.Services;

public class ContactMessageNotificationService : BackgroundService
{
    private static readonly TimeSpan FallbackSweepInterval = TimeSpan.FromMinutes(2);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IContactMessageNotificationQueue _queue;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<ContactMessageNotificationService> _logger;

    public ContactMessageNotificationService(
        IServiceScopeFactory scopeFactory,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IContactMessageNotificationQueue queue,
        IHostEnvironment hostEnvironment,
        ILogger<ContactMessageNotificationService> logger)
    {
        _scopeFactory = scopeFactory;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _queue = queue;
        _hostEnvironment = hostEnvironment;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumerTask = ConsumeQueueAsync(stoppingToken);
        var fallbackSweepTask = RunFallbackSweepAsync(stoppingToken);

        await Task.WhenAll(consumerTask, fallbackSweepTask);
    }

    private async Task ConsumeQueueAsync(CancellationToken stoppingToken)
    {
        await foreach (var contactMessageId in _queue.DequeueAllAsync(stoppingToken))
        {
            try
            {
                await SendNotificationAsync(contactMessageId, stoppingToken);
            }
            finally
            {
                _queue.Release(contactMessageId);
            }
        }
    }

    private async Task RunFallbackSweepAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await EnqueueUnsentFromDatabaseAsync(stoppingToken);

            try
            {
                await Task.Delay(FallbackSweepInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Dienst wird beendet.
            }
        }
    }

    private async Task EnqueueUnsentFromDatabaseAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RazorPagesMovieContext>();

        var offeneIds = await context.ContactMessage
            .Where(c => !c.NotificationSent)
            .OrderBy(c => c.SentAt)
            .Select(c => c.Id)
            .ToListAsync(stoppingToken);

        foreach (var id in offeneIds)
        {
            _queue.Enqueue(id);
        }
    }

    private async Task SendNotificationAsync(Guid contactMessageId, CancellationToken stoppingToken)
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

        var eintrag = await context.ContactMessage.FindAsync(new object?[] { contactMessageId }, stoppingToken);
        if (eintrag is null || eintrag.NotificationSent)
        {
            return;
        }

        var text = $"[{GetEnvironmentTag()}] Neue Kontaktanfrage von {eintrag.Name}\n"
            + $"E-Mail: {eintrag.Email}\n"
            + (string.IsNullOrWhiteSpace(eintrag.Phone) ? "" : $"Telefon: {eintrag.Phone}\n")
            + (string.IsNullOrWhiteSpace(eintrag.Subject) ? "" : $"Betreff: {eintrag.Subject}\n")
            + $"\n{eintrag.Message}";

        var httpClient = _httpClientFactory.CreateClient();
        var sendMessageUrl = $"https://api.telegram.org/bot{botToken}/sendMessage";

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

    private string GetEnvironmentTag() => _hostEnvironment.EnvironmentName switch
    {
        "Development" => "DEV",
        "Staging" => "TEST",
        "Production" => "PROD",
        var other => other.ToUpperInvariant()
    };
}

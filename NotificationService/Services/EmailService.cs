namespace NotificationService.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            // In a real application, Ш would use an email service like SendGrid, Mailgun, etc.
            // For this demo, I'll just log the email details
            
            _logger.LogInformation("Sending email to: {To}", to);
            _logger.LogInformation("Subject: {Subject}", subject);
            _logger.LogInformation("Body: {Body}", body);
            
            // Simulate sending email
            await Task.Delay(500);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {To}", to);
            return false;
        }
    }
}
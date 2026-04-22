using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace GLMS.Web.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IConfiguration config,
            ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        // ── Core send method ──────────────────────────────────
        public async Task SendEmailAsync(
            string toEmail,
            string toName,
            string subject,
            string htmlBody)
        {
            try
            {
                var fromEmail = _config["EmailSettings:FromEmail"] ?? "";
                var fromName = _config["EmailSettings:FromName"] ?? "GLMS";
                var smtpHost = _config["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_config["EmailSettings:SmtpPort"] ?? "587");
                var smtpUser = _config["EmailSettings:SmtpUser"] ?? "";
                var smtpPass = _config["EmailSettings:SmtpPassword"] ?? "";

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName, fromEmail));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlBody
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(
                    smtpHost, smtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtpUser, smtpPass);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation(
                    "Email sent to {Email} — Subject: {Subject}",
                    toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send email to {Email}", toEmail);
                // Do not throw — email failure should never crash the app
            }
        }

        // ── Contract Created ──────────────────────────────────
        public async Task SendContractCreatedAsync(
            string toEmail,
            string toName,
            int contractId,
            string clientName,
            string serviceLevel,
            DateTime startDate,
            DateTime endDate)
        {
            var subject = $"New Contract Created — GLMS #{contractId}";
            var body = BuildEmailHtml(
                title: "New Contract Created",
                preheader: $"A new contract has been created for {clientName}",
                color: "#1a1a2e",
                iconEmoji: "📄",
                bodyHtml: $@"
                    <p style='font-size:16px;color:#374151;'>
                        Hello <strong>{toName}</strong>,
                    </p>
                    <p style='font-size:15px;color:#374151;'>
                        A new contract has been created for your account.
                        Please log in to download your contract document,
                        sign it, and upload the signed copy.
                    </p>

                    <div style='background:#f3f4f6;border-radius:8px;
                                padding:20px;margin:24px 0;'>
                        <table style='width:100%;border-collapse:collapse;'>
                            <tr>
                                <td style='padding:6px 0;color:#6b7280;
                                           font-size:13px;width:40%;'>
                                    Contract Number
                                </td>
                                <td style='padding:6px 0;font-weight:600;
                                           font-size:13px;color:#111827;'>
                                    GLMS-{contractId:D5}
                                </td>
                            </tr>
                            <tr>
                                <td style='padding:6px 0;color:#6b7280;font-size:13px;'>
                                    Client
                                </td>
                                <td style='padding:6px 0;font-weight:600;
                                           font-size:13px;color:#111827;'>
                                    {clientName}
                                </td>
                            </tr>
                            <tr>
                                <td style='padding:6px 0;color:#6b7280;font-size:13px;'>
                                    Service Level
                                </td>
                                <td style='padding:6px 0;font-weight:600;
                                           font-size:13px;color:#111827;'>
                                    {serviceLevel}
                                </td>
                            </tr>
                            <tr>
                                <td style='padding:6px 0;color:#6b7280;font-size:13px;'>
                                    Start Date
                                </td>
                                <td style='padding:6px 0;font-weight:600;
                                           font-size:13px;color:#111827;'>
                                    {startDate:dd MMM yyyy}
                                </td>
                            </tr>
                            <tr>
                                <td style='padding:6px 0;color:#6b7280;font-size:13px;'>
                                    End Date
                                </td>
                                <td style='padding:6px 0;font-weight:600;
                                           font-size:13px;color:#111827;'>
                                    {endDate:dd MMM yyyy}
                                </td>
                            </tr>
                        </table>
                    </div>

                    <div style='background:#fefce8;border:1px solid #fde68a;
                                border-radius:8px;padding:14px;margin-bottom:24px;'>
                        <p style='margin:0;font-size:14px;color:#92400e;'>
                            <strong>⚠ Action Required:</strong>
                            Please download your contract, sign it, and
                            upload the signed copy via the GLMS portal.
                        </p>
                    </div>",
                buttonText: "View Contract",
                buttonUrl: $"https://localhost/Contracts/Details/{contractId}");

            await SendEmailAsync(toEmail, toName, subject, body);
        }

        // ── Contract Status Changed ───────────────────────────
        public async Task SendContractStatusChangedAsync(
            string toEmail,
            string toName,
            int contractId,
            string clientName,
            string serviceLevel,
            string oldStatus,
            string newStatus)
        {
            var statusColor = newStatus switch
            {
                "Active" => "#16a34a",
                "Expired" => "#dc2626",
                "OnHold" => "#d97706",
                _ => "#6b7280"
            };

            var subject = $"Contract Status Updated — GLMS #{contractId}";
            var body = BuildEmailHtml(
                title: "Contract Status Updated",
                preheader: $"Contract #{contractId} status changed to {newStatus}",
                color: statusColor,
                iconEmoji: "🔄",
                bodyHtml: $@"
                    <p style='font-size:16px;color:#374151;'>
                        Hello <strong>{toName}</strong>,
                    </p>
                    <p style='font-size:15px;color:#374151;'>
                        The status of your contract has been updated.
                    </p>

                    <div style='background:#f3f4f6;border-radius:8px;
                                padding:20px;margin:24px 0;'>
                        <table style='width:100%;border-collapse:collapse;'>
                            <tr>
                                <td style='padding:6px 0;color:#6b7280;
                                           font-size:13px;width:40%;'>
                                    Contract Number
                                </td>
                                <td style='padding:6px 0;font-weight:600;
                                           font-size:13px;color:#111827;'>
                                    GLMS-{contractId:D5}
                                </td>
                            </tr>
                            <tr>
                                <td style='padding:6px 0;color:#6b7280;font-size:13px;'>
                                    Client
                                </td>
                                <td style='padding:6px 0;font-weight:600;
                                           font-size:13px;color:#111827;'>
                                    {clientName}
                                </td>
                            </tr>
                            <tr>
                                <td style='padding:6px 0;color:#6b7280;font-size:13px;'>
                                    Service Level
                                </td>
                                <td style='padding:6px 0;font-weight:600;
                                           font-size:13px;color:#111827;'>
                                    {serviceLevel}
                                </td>
                            </tr>
                            <tr>
                                <td style='padding:6px 0;color:#6b7280;font-size:13px;'>
                                    Previous Status
                                </td>
                                <td style='padding:6px 0;font-size:13px;color:#6b7280;'>
                                    <s>{oldStatus}</s>
                                </td>
                            </tr>
                            <tr>
                                <td style='padding:6px 0;color:#6b7280;font-size:13px;'>
                                    New Status
                                </td>
                                <td style='padding:6px 0;'>
                                    <span style='background:{statusColor};color:#fff;
                                                 padding:3px 12px;border-radius:20px;
                                                 font-size:13px;font-weight:600;'>
                                        {newStatus}
                                    </span>
                                </td>
                            </tr>
                        </table>
                    </div>

                    {(newStatus == "Active" ? $@"
                    <div style='background:#f0fdf4;border:1px solid #bbf7d0;
                                border-radius:8px;padding:14px;margin-bottom:24px;'>
                        <p style='margin:0;font-size:14px;color:#166534;'>
                            <strong>✅ Your contract is now Active.</strong>
                            You can now raise service requests against this contract.
                        </p>
                    </div>" : "")}",
                buttonText: "View Contract",
                buttonUrl: $"https://localhost/Contracts/Details/{contractId}");

            await SendEmailAsync(toEmail, toName, subject, body);
        }

        // ── Client Uploaded Signed Contract ───────────────────
        public async Task SendClientUploadedSignedContractAsync(
            string toEmail,
            string toName,
            int contractId,
            string clientName,
            string serviceLevel,
            string uploadedByName)
        {
            var subject = $"Signed Contract Received — GLMS #{contractId}";
            var body = BuildEmailHtml(
                title: "Signed Contract Received",
                preheader: $"{clientName} has uploaded their signed contract",
                color: "#16a34a",
                iconEmoji: "✅",
                bodyHtml: $@"
                    <p style='font-size:16px;color:#374151;'>
                        Hello <strong>{toName}</strong>,
                    </p>
                    <p style='font-size:15px;color:#374151;'>
                        A client has uploaded their signed contract.
                        Please log in to review and download it.
                    </p>

                    <div style='background:#f3f4f6;border-radius:8px;
                                padding:20px;margin:24px 0;'>
                        <table style='width:100%;border-collapse:collapse;'>
                            <tr>
                                <td style='padding:6px 0;color:#6b7280;
                                           font-size:13px;width:40%;'>
                                    Contract Number
                                </td>
                                <td style='padding:6px 0;font-weight:600;
                                           font-size:13px;color:#111827;'>
                                    GLMS-{contractId:D5}
                                </td>
                            </tr>
                            <tr>
                                <td style='padding:6px 0;color:#6b7280;font-size:13px;'>
                                    Client
                                </td>
                                <td style='padding:6px 0;font-weight:600;
                                           font-size:13px;color:#111827;'>
                                    {clientName}
                                </td>
                            </tr>
                            <tr>
                                <td style='padding:6px 0;color:#6b7280;font-size:13px;'>
                                    Service Level
                                </td>
                                <td style='padding:6px 0;font-weight:600;
                                           font-size:13px;color:#111827;'>
                                    {serviceLevel}
                                </td>
                            </tr>
                            <tr>
                                <td style='padding:6px 0;color:#6b7280;font-size:13px;'>
                                    Uploaded By
                                </td>
                                <td style='padding:6px 0;font-weight:600;
                                           font-size:13px;color:#111827;'>
                                    {uploadedByName}
                                </td>
                            </tr>
                            <tr>
                                <td style='padding:6px 0;color:#6b7280;font-size:13px;'>
                                    Upload Time
                                </td>
                                <td style='padding:6px 0;font-weight:600;
                                           font-size:13px;color:#111827;'>
                                    {DateTime.Now:dd MMM yyyy HH:mm}
                                </td>
                            </tr>
                        </table>
                    </div>",
                buttonText: "Review Signed Contract",
                buttonUrl: $"https://localhost/Contracts/Details/{contractId}");

            await SendEmailAsync(toEmail, toName, subject, body);
        }

        // ── HTML email template builder ───────────────────────
        private string BuildEmailHtml(
            string title,
            string preheader,
            string color,
            string iconEmoji,
            string bodyHtml,
            string buttonText,
            string buttonUrl)
        {
            return $@"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'/>
    <meta name='viewport' content='width=device-width,initial-scale=1.0'/>
    <title>{title}</title>
</head>
<body style='margin:0;padding:0;background:#f9fafb;
             font-family:-apple-system,BlinkMacSystemFont,
             ""Segoe UI"",Roboto,sans-serif;'>

    <span style='display:none;max-height:0;overflow:hidden;'>
        {preheader}
    </span>

    <table width='100%' cellpadding='0' cellspacing='0'
           style='background:#f9fafb;padding:40px 20px;'>
        <tr>
            <td align='center'>
                <table width='600' cellpadding='0' cellspacing='0'
                       style='max-width:600px;width:100%;'>

                    <!-- Header -->
                    <tr>
                        <td style='background:{color};border-radius:12px 12px 0 0;
                                   padding:32px;text-align:center;'>
                            <div style='font-size:40px;margin-bottom:12px;'>
                                {iconEmoji}
                            </div>
                            <h1 style='margin:0;font-size:22px;font-weight:700;
                                       color:#ffffff;letter-spacing:-0.5px;'>
                                {title}
                            </h1>
                            <p style='margin:8px 0 0;font-size:13px;
                                      color:rgba(255,255,255,0.75);'>
                                TechMove Logistics — GLMS
                            </p>
                        </td>
                    </tr>

                    <!-- Body -->
                    <tr>
                        <td style='background:#ffffff;padding:36px 40px;
                                   border-left:1px solid #e5e7eb;
                                   border-right:1px solid #e5e7eb;'>
                            {bodyHtml}

                            <!-- CTA Button -->
                            <div style='text-align:center;margin:28px 0 8px;'>
                                <a href='{buttonUrl}'
                                   style='display:inline-block;
                                          background:{color};
                                          color:#ffffff;
                                          padding:14px 32px;
                                          border-radius:8px;
                                          font-size:15px;
                                          font-weight:600;
                                          text-decoration:none;
                                          letter-spacing:0.3px;'>
                                    {buttonText} →
                                </a>
                            </div>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style='background:#f3f4f6;
                                   border:1px solid #e5e7eb;
                                   border-radius:0 0 12px 12px;
                                   padding:24px 40px;
                                   text-align:center;'>
                            <p style='margin:0 0 8px;font-size:13px;
                                      color:#9ca3af;'>
                                This is an automated message from the
                                Global Logistics Management System.
                            </p>
                            <p style='margin:0;font-size:13px;color:#9ca3af;'>
                                © {DateTime.Now.Year} TechMove Logistics.
                                All rights reserved.
                            </p>
                        </td>
                    </tr>

                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }
    }
}
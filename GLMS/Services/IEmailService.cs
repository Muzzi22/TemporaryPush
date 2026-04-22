namespace GLMS.Web.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(
            string toEmail,
            string toName,
            string subject,
            string htmlBody);

        Task SendContractCreatedAsync(
            string toEmail,
            string toName,
            int contractId,
            string clientName,
            string serviceLevel,
            DateTime startDate,
            DateTime endDate);

        Task SendContractStatusChangedAsync(
            string toEmail,
            string toName,
            int contractId,
            string clientName,
            string serviceLevel,
            string oldStatus,
            string newStatus);

        Task SendClientUploadedSignedContractAsync(
            string toEmail,
            string toName,
            int contractId,
            string clientName,
            string serviceLevel,
            string uploadedByName);
    }
}
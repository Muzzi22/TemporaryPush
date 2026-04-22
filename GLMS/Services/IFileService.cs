namespace GLMS.Web.Services
{
    public interface IFileService
    {
        Task<string> SavePdfAsync(IFormFile file);
        bool IsValidPdf(IFormFile file);
    }
}
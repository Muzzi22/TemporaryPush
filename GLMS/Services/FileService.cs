namespace GLMS.Web.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FileService> _logger;

        public FileService(IWebHostEnvironment env, ILogger<FileService> logger)
        {
            _env = env;
            _logger = logger;
        }

        public bool IsValidPdf(IFormFile file)
        {
            if (file == null || file.Length == 0) return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return extension == ".pdf";
        }

        public async Task<string> SavePdfAsync(IFormFile file)
        {
            if (!IsValidPdf(file))
                throw new InvalidOperationException("Only PDF files are allowed.");

            // UUID naming prevents overwrites
            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var uploadFolder = Path.Combine(_env.WebRootPath, "uploads", "agreements");

            Directory.CreateDirectory(uploadFolder); // Ensure folder exists

            var filePath = Path.Combine(uploadFolder, uniqueFileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            // Return relative path for DB storage
            return Path.Combine("uploads", "agreements", uniqueFileName);
        }
    }
}
using GLMS.Web.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GLMS.Tests.Tests
{
    public class FileServiceTests
    {
        private FileService CreateService()
        {
            var env = new Mock<IWebHostEnvironment>();
            env.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
            var logger = new Mock<ILogger<FileService>>().Object;
            return new FileService(env.Object, logger);
        }

        private IFormFile CreateMockFile(
            string fileName, long size = 1024)
        {
            var file = new Mock<IFormFile>();
            file.Setup(f => f.FileName).Returns(fileName);
            file.Setup(f => f.Length).Returns(size);
            return file.Object;
        }

        // ── IsValidPdf ────────────────────────────────────────

        [Fact]
        public void IsValidPdf_WithPdfFile_ReturnsTrue()
        {
            var service = CreateService();
            var file = CreateMockFile("agreement.pdf");
            Assert.True(service.IsValidPdf(file));
        }

        [Fact]
        public void IsValidPdf_WithExeFile_ReturnsFalse()
        {
            var service = CreateService();
            var file = CreateMockFile("virus.exe");
            Assert.False(service.IsValidPdf(file));
        }

        [Fact]
        public void IsValidPdf_WithDocxFile_ReturnsFalse()
        {
            var service = CreateService();
            var file = CreateMockFile("document.docx");
            Assert.False(service.IsValidPdf(file));
        }

        [Fact]
        public void IsValidPdf_WithJpgFile_ReturnsFalse()
        {
            var service = CreateService();
            var file = CreateMockFile("photo.jpg");
            Assert.False(service.IsValidPdf(file));
        }

        [Fact]
        public void IsValidPdf_WithNullFile_ReturnsFalse()
        {
            var service = CreateService();
            Assert.False(service.IsValidPdf(null!));
        }

        [Fact]
        public void IsValidPdf_WithEmptyFile_ReturnsFalse()
        {
            var service = CreateService();
            var file = CreateMockFile("empty.pdf", 0);
            Assert.False(service.IsValidPdf(file));
        }

        [Fact]
        public void IsValidPdf_WithUppercasePdfExtension_ReturnsTrue()
        {
            var service = CreateService();
            var file = CreateMockFile("CONTRACT.PDF");
            Assert.True(service.IsValidPdf(file));
        }

        // ── SavePdfAsync ──────────────────────────────────────

        [Fact]
        public async Task SavePdfAsync_WithExeFile_ThrowsException()
        {
            var service = CreateService();
            var file = CreateMockFile("bad.exe");
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.SavePdfAsync(file));
        }

        [Fact]
        public async Task SavePdfAsync_WithDocxFile_ThrowsException()
        {
            var service = CreateService();
            var file = CreateMockFile("report.docx");
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.SavePdfAsync(file));
        }
    }
}
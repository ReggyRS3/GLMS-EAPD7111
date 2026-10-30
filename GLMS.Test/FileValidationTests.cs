using System.Text;
using GLMS.Web.Services;
using Xunit;

namespace GLMS.Test
{
    public class FileValidationTests
    {
        [Theory]
        [InlineData("SignedAgreement.pdf")]
        [InlineData("CONTRACT.PDF")]
        public void Validate_PdfFile_IsAccepted(string fileName)
        {
            var exception = Record.Exception(() => AgreementFileValidator.Validate(fileName, 2048));

            Assert.Null(exception);
        }

        [Theory]
        [InlineData("malware.exe")]
        [InlineData("notes.txt")]
        [InlineData("scan.jpg")]
        [InlineData("agreement.pdf.exe")]
        [InlineData("agreement")]
        public void Validate_RestrictedFileType_ThrowsError(string fileName)
        {
            var exception = Assert.Throws<InvalidOperationException>(
                () => AgreementFileValidator.Validate(fileName, 2048));

            Assert.Contains("Only PDF files", exception.Message);
        }

        [Fact]
        public void Validate_EmptyFile_ThrowsError()
        {
            Assert.Throws<InvalidOperationException>(() => AgreementFileValidator.Validate("agreement.pdf", 0));
        }

        [Fact]
        public void Validate_FileOverTenMegabytes_ThrowsError()
        {
            var tooBig = AgreementFileValidator.MaxFileSizeBytes + 1;

            Assert.Throws<InvalidOperationException>(() => AgreementFileValidator.Validate("agreement.pdf", tooBig));
        }

        [Fact]
        public void ValidatePdfContent_RealPdfHeader_IsAccepted()
        {
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes("%PDF-1.7 sample content"));

            var exception = Record.Exception(() => AgreementFileValidator.ValidatePdfContent(stream));

            Assert.Null(exception);
        }

        [Fact]
        public void ValidatePdfContent_ExeRenamedToPdf_ThrowsError()
        {
            // Windows programs start with "MZ", not "%PDF"
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes("MZ fake program bytes"));

            Assert.Throws<InvalidOperationException>(() => AgreementFileValidator.ValidatePdfContent(stream));
        }
    }
}
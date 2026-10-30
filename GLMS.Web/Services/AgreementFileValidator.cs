namespace GLMS.Web.Services
{
    public static class AgreementFileValidator
    {
        public const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

        public static void Validate(string fileName, long fileSizeBytes)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new InvalidOperationException("Please choose a file to upload.");
            }

            var extension = Path.GetExtension(fileName);
            if (!string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Only PDF files are allowed for signed agreements.");
            }

            if (fileSizeBytes <= 0)
            {
                throw new InvalidOperationException("The selected file is empty.");
            }

            if (fileSizeBytes > MaxFileSizeBytes)
            {
                throw new InvalidOperationException("The file is larger than the 10 MB limit.");
            }
        }

        // A renamed .exe can still end in .pdf, so check the file actually starts like a PDF
        public static void ValidatePdfContent(Stream stream)
        {
            var header = new byte[4];
            var bytesRead = stream.ReadAtLeast(header, 4, throwOnEndOfStream: false);

            if (bytesRead < 4 || System.Text.Encoding.ASCII.GetString(header) != "%PDF")
            {
                throw new InvalidOperationException("The file is not a valid PDF document.");
            }
        }
    }
}
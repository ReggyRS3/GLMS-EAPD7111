namespace GLMS.Web.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _folder;

        public LocalFileStorageService(IWebHostEnvironment environment)
        {
            _folder = Path.Combine(environment.ContentRootPath, "AgreementFiles");
            Directory.CreateDirectory(_folder);
        }

        public async Task<string> SaveAgreementAsync(IFormFile file, int contractId)
        {
            AgreementFileValidator.Validate(file.FileName, file.Length);

            using (var checkStream = file.OpenReadStream())
            {
                AgreementFileValidator.ValidatePdfContent(checkStream);
            }

            // Our own file name, so users can't overwrite other files or use odd characters
            var storedFileName = $"contract-{contractId}-{Guid.NewGuid():N}.pdf";
            var fullPath = Path.Combine(_folder, storedFileName);

            await using var target = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(target);

            return storedFileName;
        }

        public string? GetAgreementPath(string? storedFileName)
        {
            if (string.IsNullOrWhiteSpace(storedFileName)) return null;

            var fullPath = Path.Combine(_folder, Path.GetFileName(storedFileName));
            return File.Exists(fullPath) ? fullPath : null;
        }

        public void DeleteAgreement(string? storedFileName)
        {
            var fullPath = GetAgreementPath(storedFileName);
            if (fullPath != null)
            {
                File.Delete(fullPath);
            }
        }
    }
}
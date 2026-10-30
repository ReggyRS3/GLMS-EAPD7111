namespace GLMS.Web.Services
{
    public interface IFileStorageService
    {
        Task<string> SaveAgreementAsync(IFormFile file, int contractId);
        string? GetAgreementPath(string? storedFileName);
        void DeleteAgreement(string? storedFileName);
    }
}
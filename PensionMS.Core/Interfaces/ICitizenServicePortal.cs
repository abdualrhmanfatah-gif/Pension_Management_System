using PensionMS.Core.DTOs.Citizen;

namespace PensionMS.Core.Interfaces;

public interface ICitizenServicePortal
{
    // Documents
    Task<IEnumerable<CitizenDocumentDto>> GetDocumentsAsync(int retireeId);
    Task<IEnumerable<CitizenMissingDocumentDto>> GetMissingDocumentsAsync(int retireeId);
    Task<IEnumerable<CitizenDocumentPledgeDto>> GetPledgesAsync(int retireeId);
    Task<bool> UploadDocumentAsync(int retireeId, IRemoteFile file, string documentType);

    // Requests
    Task<IEnumerable<ServiceRequestDto>> GetRequestsAsync(int citizenAccountId);
    Task<bool> CreateRequestAsync(int citizenAccountId, int retireeId, CreateServiceRequestDto dto);
    Task<ServiceRequestDto?> GetRequestByIdAsync(int requestId, int citizenAccountId);
}

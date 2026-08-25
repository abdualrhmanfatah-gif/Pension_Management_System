using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Citizen;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.Citizen;

public class CitizenServicePortal : ICitizenServicePortal
{
    private readonly IUnitOfWork _unitOfWork;

    public CitizenServicePortal(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<CitizenDocumentDto>> GetDocumentsAsync(int retireeId)
    {
        var documents = await _unitOfWork.Documents.Query()
            .Include(d => d.DocumentType)
            .Where(d => d.RetireeID == retireeId)
            .ToListAsync();

        return documents.Select(d => new CitizenDocumentDto
        {
            DocumentID = d.DocumentID,
            FileName = d.FileName,
            DocumentType = d.DocumentType?.TypeName ?? "غير محدد",
            UploadDate = d.CreatedAt,
            FilePath = d.FilePath
        });
    }

    public async Task<IEnumerable<CitizenMissingDocumentDto>> GetMissingDocumentsAsync(int retireeId)
    {
        var file = await _unitOfWork.RetirementFiles.FirstOrDefaultAsync(f => f.RetireeID == retireeId);
        if (file == null) return Enumerable.Empty<CitizenMissingDocumentDto>();

        var missing = await _unitOfWork.MissingDocuments.Query()
            .Include(m => m.DocumentType)
            .Where(m => m.FileID == file.FileID && m.Status != "تم الإحضار")
            .ToListAsync();

        return missing.Select(m => new CitizenMissingDocumentDto
        {
            MissingDocumentID = m.MissingDocumentID,
            DocumentTypeName = m.DocumentType?.TypeName ?? "غير محدد",
            Status = m.Status,
            Notes = m.Notes
        });
    }

    public async Task<IEnumerable<CitizenDocumentPledgeDto>> GetPledgesAsync(int retireeId)
    {
        var file = await _unitOfWork.RetirementFiles.FirstOrDefaultAsync(f => f.RetireeID == retireeId);
        if (file == null) return Enumerable.Empty<CitizenDocumentPledgeDto>();

        var pledges = await _unitOfWork.DocumentPledges.Query()
            .Where(p => p.FileID == file.FileID)
            .ToListAsync();

        return pledges.Select(p => new CitizenDocumentPledgeDto
        {
            PledgeID = p.PledgeID,
            Description = p.Reason ?? "تعهد بإحضار وثائق",
            DueDate = p.Deadline,
            Status = p.Status
        });
    }

    public async Task<bool> UploadDocumentAsync(int retireeId, IRemoteFile file, string documentType)
    {
        var type = await _unitOfWork.DocumentTypes.FirstOrDefaultAsync(t => t.TypeName == documentType);
        if (type == null) return false;

        var retireeFile = await _unitOfWork.RetirementFiles.FirstOrDefaultAsync(f => f.RetireeID == retireeId);
        if (retireeFile == null) return false;

        var document = new Document
        {
            RetireeID = retireeId,
            FileID = retireeFile.FileID,
            DocumentTypeID = type.DocumentTypeID,
            FileName = file.FileName,
            FilePath = $"/uploads/citizen/{retireeId}/{file.FileName}",
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Documents.AddAsync(document);
        return await _unitOfWork.SaveChangesAsync() > 0;
    }

    public async Task<IEnumerable<ServiceRequestDto>> GetRequestsAsync(int citizenAccountId)
    {
        var requests = await _unitOfWork.ServiceRequests.Query()
            .Where(r => r.CitizenAccountID == citizenAccountId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return requests.Select(r => new ServiceRequestDto
        {
            RequestID = r.RequestID,
            RequestType = r.RequestType,
            Status = r.Status,
            CitizenNotes = r.CitizenNotes,
            AdminNotes = r.AdminNotes,
            CreatedAt = r.CreatedAt,
            ProcessedAt = r.ProcessedAt
        });
    }

    public async Task<bool> CreateRequestAsync(int citizenAccountId, int retireeId, CreateServiceRequestDto dto)
    {
        var retireeFile = await _unitOfWork.RetirementFiles.FirstOrDefaultAsync(f => f.RetireeID == retireeId);
        if (retireeFile == null) return false;

        var request = new ServiceRequest
        {
            CitizenAccountID = citizenAccountId,
            FileID = retireeFile.FileID,
            RequestType = dto.RequestType,
            CitizenNotes = dto.CitizenNotes,
            RequestDataJson = dto.RequestDataJson,
            Status = "جديد",
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ServiceRequests.AddAsync(request);
        return await _unitOfWork.SaveChangesAsync() > 0;
    }

    public async Task<ServiceRequestDto?> GetRequestByIdAsync(int requestId, int citizenAccountId)
    {
        var r = await _unitOfWork.ServiceRequests.Query()
            .FirstOrDefaultAsync(x => x.RequestID == requestId && x.CitizenAccountID == citizenAccountId);

        if (r == null) return null;

        return new ServiceRequestDto
        {
            RequestID = r.RequestID,
            RequestType = r.RequestType,
            Status = r.Status,
            CitizenNotes = r.CitizenNotes,
            AdminNotes = r.AdminNotes,
            CreatedAt = r.CreatedAt,
            ProcessedAt = r.ProcessedAt
        };
    }
}

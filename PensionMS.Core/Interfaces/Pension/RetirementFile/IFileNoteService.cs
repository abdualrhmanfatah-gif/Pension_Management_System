using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.FileNotes;

namespace PensionMS.Core.Interfaces;

public interface IFileNoteService
{
    Task<ApiResponse<List<FileNoteDto>>> GetByFileIdAsync(int fileId);
    Task<ApiResponse<FileNoteDto>> CreateAsync(CreateFileNoteDto dto, int userId);
    Task<ApiResponse<bool>> DeleteAsync(int id, int deletedByUserId);
}

using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.FileNotes;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services;

public class FileNoteService : IFileNoteService
{
    private readonly IUnitOfWork _unitOfWork;

    public FileNoteService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<List<FileNoteDto>>> GetByFileIdAsync(int fileId)
    {
        var notes = await _unitOfWork.FileNotes.Query()
            .AsNoTracking()
            .Include(n => n.CreatedByUser)
            .Where(n => n.FileID == fileId && !n.IsDeleted)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return ApiResponse<List<FileNoteDto>>.Success(notes.Select(MapToDto).ToList());
    }

    public async Task<ApiResponse<FileNoteDto>> CreateAsync(CreateFileNoteDto dto, int userId)
    {
        var note = new FileNote
        {
            FileID = dto.FileID,
            NoteText = dto.Content,
            NoteType = "General",
            Priority = 0,
            IsResolved = false,
            CreatedByUserID = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.FileNotes.AddAsync(note);
        await _unitOfWork.SaveChangesAsync();

        // Reload with relationships
        var created = await _unitOfWork.FileNotes.Query()
            .Include(n => n.CreatedByUser)
            .FirstAsync(n => n.NoteID == note.NoteID);

        return ApiResponse<FileNoteDto>.Success(MapToDto(created), "تم إضافة الملاحظة بنجاح");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, int deletedByUserId)
    {
        var note = await _unitOfWork.FileNotes.GetByIdAsync(id);
        if (note == null)
            return ApiResponse<bool>.Fail("الملاحظة غير موجودة");

        note.IsDeleted = true;
        note.DeletedAt = DateTime.UtcNow;
        note.DeletedByUserID = deletedByUserId;

        _unitOfWork.FileNotes.Update(note);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم حذف الملاحظة");
    }

    private static FileNoteDto MapToDto(FileNote n) => new()
    {
        NoteID = n.NoteID,
        FileID = n.FileID,
        Content = n.NoteText,
        CreatedByUsername = n.CreatedByUser?.FullName,
        CreatedAt = n.CreatedAt
    };
}

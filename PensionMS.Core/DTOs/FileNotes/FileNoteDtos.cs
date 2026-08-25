namespace PensionMS.Core.DTOs.FileNotes;

public class CreateFileNoteDto
{
    public int FileID { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class FileNoteDto
{
    public int NoteID { get; set; }
    public int FileID { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? CreatedByUsername { get; set; }
    public DateTime CreatedAt { get; set; }
}

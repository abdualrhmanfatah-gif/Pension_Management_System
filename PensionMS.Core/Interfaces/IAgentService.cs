using PensionMS.Core.DTOs.Agents;
using PensionMS.Core.DTOs.Common;

namespace PensionMS.Core.Interfaces;

public interface IAgentService
{
    Task<ApiResponse<List<AgentDto>>> GetByFileIdAsync(int fileId);
    Task<ApiResponse<AgentDto>> GetByIdAsync(int id);
    Task<ApiResponse<AgentDto>> CreateAsync(CreateAgentDto dto, int userId);
    Task<ApiResponse<AgentDto>> UpdateAsync(int id, UpdateAgentDto dto, int userId);
    Task<ApiResponse<bool>> DeactivateAsync(int id, int userId);
    Task<ApiResponse<bool>> DeleteAsync(int id, int userId);
    Task<ApiResponse<List<AgentSearchResultDto>>> SearchAsync(string query, int limit = 10);
}

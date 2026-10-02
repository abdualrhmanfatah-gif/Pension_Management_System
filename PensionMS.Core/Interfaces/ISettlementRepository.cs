using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Settlements;
using PensionMS.Core.Entities;

namespace PensionMS.Core.Interfaces;

public interface ISettlementRepository
{
    Task<List<SettlementBatchListDto>> GetAllBatchesAsync();
    Task<SettlementBatchListDto?> GetBatchByIdAsync(int id);
    Task<bool> IsBatchNumberExistsAsync(string batchNumber);
    
    Task<List<SettlementDecisionDto>> GetAllDecisionsAsync();
    Task<PagedResult<SettlementDecisionDto>> GetDecisionsPagedAsync(int page, int pageSize, string? searchTerm = null);
    Task<SettlementDecisionDto?> GetDecisionByIdAsync(int decisionId);
    Task<List<SettlementDecisionDto>> GetBatchDecisionsAsync(int batchId);
    
    Task<List<SettlementItemDto>> GetBatchItemsAsync(int batchId);
    Task<List<SettlementItemDto>> GetDecisionItemsAsync(int decisionId);
    Task<PagedResult<SettlementItemDto>> GetDecisionItemsPagedAsync(int decisionId, int page, int pageSize, string? searchTerm = null);
    Task<List<SettlementItemDto>> GetByRetireeIdAsync(int retireeId);
    
    Task<List<SettlementItem>> GetItemsByDecisionIdAsync(int decisionId);
    
    Task ApplyRetireeRankUpdatesAsync(List<RetireeRankUpdate> updates);
}

// DTO المستخدم محلياً داخل التحديثات
public class RetireeRankUpdate
{
    public int RetireeID { get; set; }
    public int ProcessedRankID { get; set; }
    public decimal NewSalary { get; set; }
}

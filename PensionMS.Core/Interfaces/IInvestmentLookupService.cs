using System.Collections.Generic;
using System.Threading.Tasks;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Investment;

namespace PensionMS.Core.Interfaces;

public interface IInvestmentLookupService
{
    Task<ApiResponse<List<InvestmentLookupDto>>> GetProjectStatusesAsync();
    Task<ApiResponse<List<InvestmentLookupDto>>> GetInvestmentTypesAsync();
    Task<ApiResponse<List<InvestmentLookupDto>>> GetPropertyTypesAsync();
    Task<ApiResponse<List<InvestmentLookupDto>>> GetUnitTypesAsync();
}

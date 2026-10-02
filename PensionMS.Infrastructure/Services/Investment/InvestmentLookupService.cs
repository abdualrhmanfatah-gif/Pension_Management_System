using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Investment;
using PensionMS.Core.Entities.Investment;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.Investment;

public class InvestmentLookupService : IInvestmentLookupService
{
    private readonly IUnitOfWork _unitOfWork;

    public InvestmentLookupService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<List<InvestmentLookupDto>>> GetProjectStatusesAsync()
    {
        // Use explicit object instantiation instead of anonymous types
        var statuses = new List<InvestmentLookupDto>
        {
            new InvestmentLookupDto { ID = 1, Name = "Active", Code = "ACTIVE" },
            new InvestmentLookupDto { ID = 2, Name = "Completed", Code = "COMPLETED" },
            new InvestmentLookupDto { ID = 3, Name = "On Hold", Code = "ONHOLD" },
            new InvestmentLookupDto { ID = 4, Name = "Cancelled", Code = "CANCELLED" }
        };

        return await Task.FromResult(ApiResponse<List<InvestmentLookupDto>>.Success(statuses));
    }
    
    public async Task<ApiResponse<List<InvestmentLookupDto>>> GetInvestmentTypesAsync()
    {
        var types = await _unitOfWork.InvestmentTypes.Query()
            .Where(t => !t.IsDeleted)
            .Select(t => new InvestmentLookupDto
            {
                ID = t.TypeID,
                Name = t.TypeName,
                Code = t.Code
            })
            .ToListAsync();
            
        return ApiResponse<List<InvestmentLookupDto>>.Success(types);
    }

    public async Task<ApiResponse<List<InvestmentLookupDto>>> GetPropertyTypesAsync()
    {
        var types = await _unitOfWork.PropertyTypes.Query()
            .Where(t => !t.IsDeleted)
            .Select(t => new InvestmentLookupDto
            {
                ID = t.TypeID,
                Name = t.TypeName
            })
            .ToListAsync();

        return ApiResponse<List<InvestmentLookupDto>>.Success(types);
    }

    public async Task<ApiResponse<List<InvestmentLookupDto>>> GetUnitTypesAsync()
    {
        var types = await _unitOfWork.UnitTypes.Query()
            .Where(t => !t.IsDeleted)
            .Select(t => new InvestmentLookupDto
            {
                ID = t.TypeID,
                Name = t.TypeName
            })
            .ToListAsync();

        return ApiResponse<List<InvestmentLookupDto>>.Success(types);
    }
}

using Microsoft.Extensions.DependencyInjection;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Services.Documents;

namespace PensionMS.Infrastructure.DependencyInjection;

public static class DocumentsServiceCollectionExtensions
{
    public static IServiceCollection AddDocumentsServices(this IServiceCollection services)
    {
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IDocumentExemptionService, DocumentExemptionService>();
        services.AddScoped<IDocumentPledgeService, DocumentPledgeService>();
        services.AddScoped<IDocumentTypeService, DocumentTypeService>();
        services.AddScoped<IMissingDocumentService, MissingDocumentService>();
        return services;
    }
}

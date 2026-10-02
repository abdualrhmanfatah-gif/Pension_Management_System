using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using PensionMS.Core.Interfaces;

namespace PensionMS.API.Helpers;

public class RemoteFileModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
            throw new ArgumentNullException(nameof(bindingContext));

        var modelName = bindingContext.ModelName;

        // Try to get IFormFile from the request (case-insensitive)
        var request = bindingContext.HttpContext.Request;
        var formFile = request.Form.Files.FirstOrDefault(f =>
            f.Name.Equals(modelName, StringComparison.OrdinalIgnoreCase));
        if (formFile != null)
        {
            bindingContext.Result = ModelBindingResult.Success(new RemoteFileWrapper(formFile));
        }
        else if (bindingContext.ModelMetadata.IsReferenceOrNullableType)
        {
            bindingContext.Result = ModelBindingResult.Success(null);
        }

        return Task.CompletedTask;
    }
}

public class RemoteFileModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        if (context.Metadata.ModelType == typeof(IRemoteFile))
        {
            return new RemoteFileModelBinder();
        }

        return null;
    }
}

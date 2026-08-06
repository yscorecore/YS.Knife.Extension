using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using YS.Knife.DataItem.Api.AspnetCore.Internal;

namespace YS.Knife.DataItem.Impl.Default
{
    [Service]
    [AutoConstructor]
    [Logger]

    public partial class DataItemWebService : IDataItemWebService
    {
        private readonly IDataItemService dataItemService;
        private readonly IModelMetadataProvider metadataProvider;
        private readonly IModelBinderFactory modelBinderFactory;
        private readonly ParameterBinder parameterBinder;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IActionContextAccessor _actionContextAccessor;
        public Task<List<DataItemDesc>> ListItems()
        {
            return dataItemService.GetAllDataItems();
        }

        public async Task<Dictionary<string, object>> LoadData(string[] di, CancellationToken cancellationToken)
        {
            var dic = new Dictionary<string, object>();
            var context = httpContextAccessor.HttpContext;
            foreach (var item in (di ?? Array.Empty<string>()).Distinct())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                var args = await GetParameterValues(item, context);
                try
                {
                    dic[item] = await dataItemService.GetItem(item, args, cancellationToken);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Invoke dataitem '{item}' error.", ex);
                }
            }
            return dic;
        }
        private async Task<object[]> GetParameterValues(string dataItem, HttpContext context)
        {
            var entry = await dataItemService.GetEntry(dataItem);
            var valueProvider = new DataItemValueQueryStringProvider(dataItem, context.Request.Query);
            var result = new object[entry.Parameters.Length];
            for (var i = 0; i < entry.Parameters.Length; i++)
            {
                var p = entry.Parameters[i];
                var modelMeta = metadataProvider.GetMetadataForType(p.ParameterType);
                var bindingInfo = new BindingInfo { BindingSource = BindingSource.Query, BinderModelName = p.Name };
                var binder = modelBinderFactory.CreateBinder(new ModelBinderFactoryContext
                {
                    Metadata = modelMeta,
                    BindingInfo = bindingInfo
                });
                var parameterDesc = new ParameterDescriptor
                {
                    BindingInfo = bindingInfo,
                    Name = p.Name,
                    ParameterType = p.ParameterType,
                };
                var defaultValue = GetParameterDefaultValue(p);

                var modelBindResult = await parameterBinder.BindModelAsync(_actionContextAccessor.ActionContext, binder, valueProvider, parameterDesc, modelMeta, defaultValue);
                if (modelBindResult.IsModelSet == false)
                {
                    logger.LogInformation("The parameter '{param}' of dataitem '{dataitem}' has not setted.", p.Name, dataItem);
                }
                result[i] = modelBindResult.Model;
            }
            return result;
        }
        private object GetParameterDefaultValue(ParameterInfo parameterInfo)
        {
            if (parameterInfo.HasDefaultValue)
            {
                return parameterInfo.DefaultValue;
            }
            return parameterInfo.ParameterType.GetDefaultValue();

        }
    }
}

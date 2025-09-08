using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using YS.Knife.DataItem.Api.AspnetCore.Internal;

namespace YS.Knife.DataItem.Api.AspnetCore
{
    [ApiController]
    [Route("api/[controller]")]
    [AutoConstructor]
    public partial class DataItemsController : ControllerBase
    {
        private const string DataItemName = "di";
        private readonly IDataItemService dataItemService;
        private readonly IModelMetadataProvider metadataProvider;
        private readonly IModelBinderFactory modelBinderFactory;
        private readonly ParameterBinder parameterBinder;
        private readonly ILogger<DataItemsController> logger;

        [HttpGet]
        public async Task<Dictionary<string, object>> LoadItemsData([FromQuery(Name = "di")] string[] dataItems, CancellationToken cancellationToken)
        {
            var dic = new Dictionary<string, object>();
            foreach (var item in (dataItems ?? Array.Empty<string>()).Distinct())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                var args = await GetParameterValues(item);
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
        private async Task<object[]> GetParameterValues(string dataItem)
        {
            var entry = await dataItemService.GetEntry(dataItem);
            var valueProvider = new DataItemValueQueryStringProvider(dataItem, this.HttpContext.Request.Query);
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
                var modelBindResult = await parameterBinder.BindModelAsync(this.ControllerContext, binder, valueProvider, parameterDesc, modelMeta, defaultValue);
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

        [HttpGet]
        [Route("all")]
        public Task<List<DataItemDesc>> GetAllDataItems()
        {
            return dataItemService.GetAllDataItems();
        }
    }

}

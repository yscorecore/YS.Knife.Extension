using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using static YS.Knife.CodeExchange.IImageCodeDataReceiver;
using static YS.Knife.CodeExchange.IImageCodeDataSender;

namespace YS.Knife.CodeExchange.Impl.DistributedCache
{
    [Service(typeof(IImageCodeDataSender))]
    [Service(typeof(IImageCodeDataReceiver))]
    [AutoConstructor]
    [Logger]
    public partial class ImageCodeDataService : IImageCodeDataSender, IImageCodeDataReceiver
    {
        private readonly IDistributedCache distributedCache;
        private readonly IEnumerable<IImageCodeHandler> imageCodeHandlers;
        private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };
        public async Task<ImageCodeInfo> CreateImageCode(string name, object args, CancellationToken cancellationToken)
        {
            var id = Guid.NewGuid();
            var handler = FindHandlerByName(name);
            var (sence, stream) = await handler.GeneratorCode(args, cancellationToken);
            var bytes = new byte[stream.Length];
            _ = await stream.ReadAsync(bytes, cancellationToken);
            var res = new ImageCodeInfo(id, DateTimeOffset.Now.Add(handler.Expired), bytes);
            var dataKind = handler.DataKind;
            var defaultData = dataKind == ImageCodeDataKind.Single ? default(object) : Array.Empty<object>();
            await distributedCache.SetObjectAsync($"{id}", new TempDataInfo(dataKind, sence, res.Exipred, defaultData), handler.Expired, jsonSerializerOptions);
            await distributedCache.SetObjectAsync(sence, new TempSenceInfo(name, id, args, res.Exipred), handler.Expired, jsonSerializerOptions);
            return res;
        }
        private IImageCodeHandler FindHandlerByName(string name)
        {
            var handler = imageCodeHandlers.FirstOrDefault(p => p.Name == name);
            _ = handler ?? throw new Exception($"Can not find image code handler by name '{name}'.");
            return handler;
        }
        public async Task<bool> Release(Guid id, CancellationToken cancellationToken)
        {
            var data = await distributedCache.GetObjectAsync<TempDataInfo>($"{id}", jsonSerializerOptions);
            if (data == null)
            {
                return false;
            }
            else
            {
                await distributedCache.RemoveAsync(data.Sence, cancellationToken);
                await distributedCache.RemoveAsync($"{id}", cancellationToken);
                return true;
            }
        }
        public async Task<ImageCodeRequest> QueryData(Guid id, CancellationToken cancellationToken)
        {
            var data = await distributedCache.GetStringAsync($"{id}", cancellationToken);
            if (data == null)
            {
                return new ImageCodeRequest(false, null);
            }
            else
            {
                var dataObj = data.AsJsonObject<TempDataInfo>(jsonSerializerOptions);
                if (dataObj.DataKind == ImageCodeDataKind.Queue)
                {
                    var newData = dataObj with { Data = Array.Empty<object>() };
                    await distributedCache.SetObjectAsync($"{id}", newData, new DistributedCacheEntryOptions { AbsoluteExpiration = dataObj.Exipred }, jsonSerializerOptions);
                    return new ImageCodeRequest(true, dataObj.Data);
                }
                else
                {
                    return new ImageCodeRequest(true, dataObj.Data);
                }
            }
        }
        public Task<SenceInfo> QuerySenceInfo(string sence, CancellationToken cancellationToken)
        {
            return distributedCache.GetObjectAsync<SenceInfo>(sence, jsonSerializerOptions);
        }

        public async Task<bool> SendData(string sence, object data, CancellationToken cancellationToken)
        {
            var tempSence = await distributedCache.GetObjectAsync<TempSenceInfo?>(sence, jsonSerializerOptions);
            if (tempSence == null)
            {
                logger.LogWarning("Sence '{sence}' is not exist.", sence);
                return false;
            }
            else
            {
                var handler = FindHandlerByName(tempSence.Name);
                var dataObj = await distributedCache.GetObjectAsync<TempDataInfo>($"{tempSence.Id}", jsonSerializerOptions);
                if (dataObj != null)
                {
                    if (dataObj.DataKind == ImageCodeDataKind.Queue)
                    {
                        //队列
                        var current = dataObj.Data as object[] ?? Array.Empty<object>();
                        var newTempData = dataObj with { Data = current.ConcatItems(data).ToArray() };
                        await handler.OnDataPushed(tempSence.Argument, data, cancellationToken);
                        await distributedCache.SetObjectAsync($"{tempSence.Id}", newTempData, new DistributedCacheEntryOptions { AbsoluteExpiration = tempSence.Expired }, jsonSerializerOptions);
                        return true;
                    }
                    else
                    {
                        if(dataObj.Data != null)
                        {
                            logger.LogWarning("Data '{id}' is already exists", tempSence.Id);
                            return false;
                        }
                        //单对象
                        var newTempData = dataObj with { Data = data };
                        await handler.OnDataPushed(tempSence.Argument, data, cancellationToken);
                        await distributedCache.SetObjectAsync($"{tempSence.Id}", newTempData, new DistributedCacheEntryOptions { AbsoluteExpiration = tempSence.Expired }, jsonSerializerOptions);
                        return true;
                    }
                }
                else
                {
                    logger.LogWarning("Data '{id}' is not exists", tempSence.Id);
                    return false;
                }
            }
        }

        private record TempSenceInfo(string Name, Guid Id, object Argument, DateTimeOffset Expired);
        private record TempDataInfo(ImageCodeDataKind DataKind, string Sence, DateTimeOffset Exipred, object? Data);

    }
}

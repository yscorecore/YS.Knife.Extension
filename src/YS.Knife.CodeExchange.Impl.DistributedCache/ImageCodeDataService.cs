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
            await distributedCache.SetObjectAsync($"{id}", new TempDataInfo(dataKind, sence, res.Exipred, defaultData), handler.Expired, IImageCodeHandler.JsonOptions);
            await distributedCache.SetObjectAsync(sence, new TempSenceInfo(name, id, args, res.Exipred), handler.Expired, IImageCodeHandler.JsonOptions);
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
            var data = await distributedCache.GetObjectAsync<TempDataInfo>($"{id}", IImageCodeHandler.JsonOptions);
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
                var dataObj = data.AsJsonObject<TempDataInfo>(IImageCodeHandler.JsonOptions);
                if (dataObj.DataKind == ImageCodeDataKind.Queue)
                {
                    var newData = dataObj with { Data = Array.Empty<object>() };
                    await distributedCache.SetObjectAsync($"{id}", newData, new DistributedCacheEntryOptions { AbsoluteExpiration = dataObj.Exipred }, IImageCodeHandler.JsonOptions);
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
            return distributedCache.GetObjectAsync<SenceInfo>(sence, IImageCodeHandler.JsonOptions);
        }

        public async Task<bool> SendData(string sence, object data, CancellationToken cancellationToken)
        {
            var tempSence = await distributedCache.GetObjectAsync<TempSenceInfo?>(sence, IImageCodeHandler.JsonOptions);
            if (tempSence == null)
            {
                logger.LogWarning("Sence '{sence}' is not exist.", sence);
                return false;
            }
            else
            {
                var handler = FindHandlerByName(tempSence.Name);
                var dataObj = await distributedCache.GetObjectAsync<TempDataInfo>($"{tempSence.Id}", IImageCodeHandler.JsonOptions);
                if (dataObj != null)
                {
                    if (dataObj.DataKind == ImageCodeDataKind.Queue)
                    {
                        //队列
                        var processedData = await handler.ProcessData(tempSence.Args, data, cancellationToken);
                        var current = (dataObj.Data).AsJsonElement().AsJsonObject<object[]>(IImageCodeHandler.JsonOptions) ?? Array.Empty<object>();
                        var newTempData = dataObj with { Data = current.ConcatItems(processedData).ToArray() };
                        await distributedCache.SetObjectAsync($"{tempSence.Id}", newTempData, new DistributedCacheEntryOptions { AbsoluteExpiration = tempSence.Expired }, IImageCodeHandler.JsonOptions);
                        return true;
                    }
                    else
                    {
                        if (dataObj.Data != null)
                        {
                            logger.LogWarning("Data '{id}' is already exists", tempSence.Id);
                            return false;
                        }
                        var processedData = await handler.ProcessData(tempSence.Args, data, cancellationToken);
                        //单对象
                        var newTempData = dataObj with { Data = processedData };
                        await distributedCache.SetObjectAsync($"{tempSence.Id}", newTempData, new DistributedCacheEntryOptions { AbsoluteExpiration = tempSence.Expired }, IImageCodeHandler.JsonOptions);
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

        private record TempSenceInfo(string Name, Guid Id, object Args, DateTimeOffset Expired);
        private record TempDataInfo(ImageCodeDataKind DataKind, string Sence, DateTimeOffset Exipred, object? Data);

    }
}

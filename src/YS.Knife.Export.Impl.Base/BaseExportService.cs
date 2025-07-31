using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Caching.Distributed;

namespace YS.Knife.Export.Impl
{
    [AutoConstructor]
    public abstract partial class BaseExportService : IExportService
    {
        private readonly ExportOptions exportOptions;
        private readonly IDistributedCache distributedCache;

        [CodeExceptions]
        static partial class Errors
        {
            [CodeException("001", "Token已过期")]
            public static partial Exception TokenHasExpired();
            [CodeException("002", "Token不存在")]
            public static partial Exception TokenNotExists();
            [CodeException("003", "名称为{name}的元数据不存在")]
            public static partial Exception MetadataNotExists(string name);
        }

        public async Task<ExportToken> BeginExport([Required] EntityMetadata[] metadatas)
        {
            var token = Guid.NewGuid();
            var expiredIn = DateTimeOffset.Now.AddSeconds(exportOptions.ExpiredIn);
            var exportInfo = new ExportInfo
            {
                ExpiredIn = expiredIn,
                MetaDatas = metadatas,
                Token = token,
                StartTime = DateTimeOffset.Now,
                FilePath = GetDataFileName(token)
            };
            await SaveExportInfo(exportInfo);
            await WriteDataTitle(exportInfo.FilePath, metadatas);
            return new ExportToken
            {
                Token = token,
                ExpiredIn = exportOptions.ExpiredIn,
            };
        }

        public async Task<bool> CancelExport(Guid token)
        {
            var exportInfo = await GetExportInfo(token);
            try
            {
                File.Delete(exportInfo.FilePath);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                await DeleteExportInfo(exportInfo);
            }
        }

        public async Task<Stream> EndExport(Guid token)
        {
            var exportInfo = await GetExportInfo(token);
            try
            {
                return File.OpenRead(exportInfo.FilePath);
            }
            finally
            {
                await DeleteExportInfo(exportInfo);
            }
        }

        public async Task Export(Guid token, EntityData data)
        {
            var exportInfo = await GetExportInfo(token);
            var (meta, index) = FindSheetIndex(exportInfo.MetaDatas);
            await WriteData(exportInfo.FilePath, meta, index, data.Datas);
            (EntityMetadata, int) FindSheetIndex(EntityMetadata[] metadatas)
            {
                for (int i = 0; i < metadatas.Length; i++)
                {
                    var metadata = metadatas[i];
                    if (metadata.Name == data.Name)
                    {
                        return (metadata, i);
                    }
                }
                throw Errors.MetadataNotExists(data.Name);
            }

        }
        protected virtual string GetDataFileName(Guid token) => Path.Combine(exportOptions.DataFolder, $"{DateTime.Now:yyyyMMddHHmmss}_{token}");
        protected abstract Task WriteDataTitle(string fileName, EntityMetadata[] metadatas);
        protected abstract Task WriteData(string fileName, EntityMetadata metadata, int metadataIndex, List<Dictionary<string, object>> datas);
        protected virtual async Task SaveExportInfo(ExportInfo exportInfo)
        {
            await distributedCache.SetStringAsync(exportInfo.Token.ToString(), exportInfo.ToJsonText(), exportInfo.ExpiredIn - exportInfo.StartTime);
        }
        protected virtual async Task<ExportInfo> GetExportInfo(Guid token)
        {
            var exportInfo = await distributedCache.GetObjectAsync<ExportInfo>(token.ToString());
            if (exportInfo == null)
            {
                throw Errors.TokenNotExists();
            }
            if (exportInfo.ExpiredIn < DateTimeOffset.Now)
            {
                throw Errors.TokenHasExpired();
            }
            return exportInfo;
        }
        protected virtual async Task DeleteExportInfo(ExportInfo exportInfo)
        {
            await distributedCache.RemoveAsync(exportInfo.Token.ToString());
        }


        protected record ExportInfo
        {
            public DateTimeOffset StartTime { get; set; }
            public Guid Token { get; set; }
            public DateTimeOffset ExpiredIn { get; set; }
            public EntityMetadata[] MetaDatas { get; set; }
            public string FilePath { get; set; }
        }

    }
}

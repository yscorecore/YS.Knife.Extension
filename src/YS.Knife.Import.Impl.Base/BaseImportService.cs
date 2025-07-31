using Microsoft.Extensions.Caching.Distributed;
using YS.Knife.Import.Abstractions;

namespace YS.Knife.Import.Impl.Base
{
    [AutoConstructor]
    public abstract partial class BaseImportService : IImportService
    {
        private readonly ImportOptions importOptions;
        private readonly IDistributedCache distributedCache;

        [CodeExceptions]
        protected static partial class Errors
        {
            [CodeException("001", "Token已过期")]
            public static partial Exception TokenHasExpired();
            [CodeException("002", "Token不存在")]
            public static partial Exception TokenNotExists();
            [CodeException("003", "名称为{name}的元数据不存在")]
            public static partial Exception MetadataNotExists(string name);

            [CodeException("004", "找不到名称为{name}的表")]
            public static partial Exception CannotFindSheet(string name);
            [CodeException("005", "找不到名称为{name}的列")]
            public static partial Exception CannotFindColumn(string name);
            [CodeException("005", "不支持的文件格式")]
            public static partial Exception NotSupportFileFormat();
        }


        public async Task<ImportToken> BeginImport(Stream data, string fileExt)
        {
            var token = Guid.NewGuid();
            var expiredIn = DateTimeOffset.Now.AddSeconds(importOptions.ExpiredIn);
            var exportInfo = new ImportInfo
            {
                ExpiredIn = expiredIn,
                Token = token,
                StartTime = DateTimeOffset.Now,
                FilePath = GetDataFileName(token, fileExt),
            };

            SaveDataFile(data, exportInfo);
            await SaveImportInfo(exportInfo);
            return new ImportToken
            {
                Token = token,
                ExpiredIn = importOptions.ExpiredIn,
            };
        }

        private static void SaveDataFile(Stream data, ImportInfo exportInfo)
        {
            var folder = Path.GetDirectoryName(exportInfo.FilePath);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            using var stream = File.OpenWrite(exportInfo.FilePath);
            data.CopyTo(stream);
            stream.Flush();
        }

        public async Task<bool> EndImport(Guid token)
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
                await DeleteImportInfo(exportInfo);
            }
        }

        public async Task<Dictionary<string, List<Dictionary<string, object>>>> ReadData(Guid token, EntityMetadata[] entityMetadata)
        {
            var importInfo = await GetExportInfo(token);
            return await ReadData(importInfo.FilePath, entityMetadata);
        }
        protected abstract Task<Dictionary<string, List<Dictionary<string, object>>>> ReadData(string filePath, EntityMetadata[] entityMetadata);

        public abstract Task<Dictionary<string, List<ColumnInfo>>> ReadColumn(Guid token);
        protected virtual async Task<ImportInfo> GetExportInfo(Guid token)
        {
            var exportInfo = await distributedCache.GetObjectAsync<ImportInfo>(token.ToString());
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

        protected virtual async Task SaveImportInfo(ImportInfo importInfo)
        {
            await distributedCache.SetObjectAsync(importInfo.Token.ToString(), importInfo, importInfo.ExpiredIn - importInfo.StartTime);
        }

        protected virtual async Task DeleteImportInfo(ImportInfo importInfo)
        {
            await distributedCache.RemoveAsync(importInfo.Token.ToString());
        }

        protected virtual string GetDataFileName(Guid token, string fileExt) => Path.Combine(importOptions.DataFolder, $"{DateTime.Now:yyyyMMddHHmmss}_{token}{fileExt}");

        protected record ImportInfo
        {
            public DateTimeOffset StartTime { get; set; }
            public Guid Token { get; set; }
            public DateTimeOffset ExpiredIn { get; set; }
            public string FilePath { get; set; }
            public string FileExt { get; set; }
        }
    }
}

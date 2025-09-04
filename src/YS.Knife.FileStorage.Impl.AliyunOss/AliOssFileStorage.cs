using Aliyun.OSS;

namespace YS.Knife.FileStorage.AliyunOss
{
    [Service]
    [DictionaryKey("alioss")]
    [AutoConstructor]
    public partial class AliOssFileStorage : IFileStorageService
    {
        private readonly AliyunOssOptions ossConfiguration;

        public Task<ClientUploadInfo> GetClientUploadInfo(string key, IDictionary<string, object> metadata, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ClientUploadInfo
            {

            });
        }

        public Task<FileObject> GetObject(string key)
        {
            return Task.FromResult(new FileObject
            {
                PublicUrl = $"{ossConfiguration.PublicPoint}/{key}"
            });
        }

        public Task<FileObject> MoveObject(string key, string newKey, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            var result = client.CopyObject(new CopyObjectRequest(ossConfiguration.BucketName, key, ossConfiguration.BucketName, newKey));
            if (result.HttpStatusCode == System.Net.HttpStatusCode.OK || result.HttpStatusCode == System.Net.HttpStatusCode.Created)
            {
                var deleteResult = client.DeleteObject(ossConfiguration.BucketName, key);
                if (deleteResult.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    throw new Exception($"delete oss object error, oss response code {result.HttpStatusCode}, requestid {result.RequestId}.");
                }
                var imageInfo = new FileObject
                {
                    PublicUrl = $"{ossConfiguration.PublicPoint}/{newKey}"
                };
                return Task.FromResult(imageInfo);
            }
            else
            {
                throw new Exception($"copy oss object error, oss response code {result.HttpStatusCode}, requestid {result.RequestId}.");
            }
        }


        public Task<FileObject> PutObject(string key, Stream content, IDictionary<string, object> metadata, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            var result = client.PutObject(ossConfiguration.BucketName, key, content, CreateObjectMeta(metadata));
            if (result.HttpStatusCode == System.Net.HttpStatusCode.OK || result.HttpStatusCode == System.Net.HttpStatusCode.Created)
            {
                var imageInfo = new FileObject
                {
                    Key = key,
                    PublicUrl = $"{ossConfiguration.PublicPoint}/{key}"
                };
                return Task.FromResult(imageInfo);
            }
            else
            {
                throw new Exception($"put oss object error, oss response code {result.HttpStatusCode}, requestid {result.RequestId}.");
            }
        }

        private OssClient CreateClient()
        {
            return new OssClient($"{ossConfiguration.Endpoint}", ossConfiguration.AccessKeyId, ossConfiguration.AccessKeySecret);
        }

        private ObjectMetadata CreateObjectMeta(IDictionary<string, object> mata)
        {
            if (mata == null)
            {
                return null;
            }
            else
            {
                ObjectMetadata res = new ObjectMetadata();
                foreach (var (k, v) in mata)
                {
                    res.AddHeader(k, v);
                }
                return res;
            }
        }
    }
}

using System.Net;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;

namespace YS.Knife.FileStorage.Minio
{
    [Service]
    [AutoConstructor]
    public partial class MinioFileStorage : IFileStorageService
    {
        private MinioOptions options;
        private ILogger<MinioFileStorage> logger;

        private (string, bool) GetEndPoint()
        {
            if (options.Endpoint.StartsWith("http://"))
            {
                return (options.Endpoint[7..], false);
            }
            else if (options.Endpoint.StartsWith("https://"))
            {
                return (options.Endpoint[8..], true);
            }
            else
            {
                return (options.Endpoint, true);
            }
        }

        private IMinioClient CreateClient()
        {
            var (endPoint, ssl) = GetEndPoint();
            return new MinioClient()
                    .WithEndpoint(endPoint)
                    .WithCredentials(options.AccessKeyId, options.AccessKeySecret)
                    .WithSSL(ssl)
                    .Build();
        }

        public async Task<FileObject> PutObject(string key, Stream content, IDictionary<string, object> metadata, CancellationToken cancellationToken = default)
        {
            try
            {
                var minio = CreateClient();

                var arg = new PutObjectArgs().WithBucket(options.BucketName)
                        .WithObject(key)
                        .WithStreamData(content)
                        .WithObjectSize(content.Length)
                        .WithHeaders(ConvertMeta(metadata));
                var res = await minio.PutObjectAsync(arg, cancellationToken);

                return new FileObject
                {
                    FileName = Path.GetFileName(key),
                    Key = res.ObjectName,
                    PublicUrl = $"{options.PublicPoint}/{res.ObjectName}"
                };
            }
            catch (Exception ex)
            {
                throw new Exception("upload minio file failed", ex);
            }

        }
        private IDictionary<string, string> ConvertMeta(IDictionary<string, object> metadata)
        {
            var res = new Dictionary<string, string>();
            if (metadata != null)
            {
                foreach (var (k, v) in metadata)
                {
                    if (k != null && v != null)
                    {
                        res.Add(k, v.ToString());
                    }
                }
            }

            return res;
        }

        public async Task<FileObject> MoveObject(string key, string newKey, CancellationToken cancellationToken = default)
        {
            try
            {
                var minio = CreateClient();

                var arg = new CopyObjectArgs()
                        .WithBucket(options.BucketName)
                        .WithObject(newKey)
                        .WithCopyObjectSource(new CopySourceObjectArgs().WithBucket(options.BucketName).WithObject(key));

                await minio.CopyObjectAsync(arg, cancellationToken);

                var removeArg = new RemoveObjectArgs().WithBucket(options.BucketName).WithObject(key);
                await minio.RemoveObjectAsync(removeArg, cancellationToken);

                return new FileObject
                {
                    Key = newKey,
                    FileName = Path.GetFileName(newKey),
                    PublicUrl = $"{options.PublicPoint}/{newKey}"
                };
            }
            catch (Exception ex)
            {

                throw new Exception("move minio file failed", ex);
            }
        }

        public Task<FileObject> GetObject(string key)
        {
            return Task.FromResult(new FileObject
            {
                Key = key,
                FileName = Path.GetFileName(key),
                PublicUrl = $"{options.PublicPoint}/{key}"
            });
        }

        public async Task<ClientUploadInfo> GetClientUploadInfo(string key, long fileSize, IDictionary<string, object> metadata, CancellationToken cancellationToken = default)
        {
            var minio = CreateClient();

            // MinIO v6 POST 表单签名：PostPolicy + PresignedPostPolicyAsync
            // PostPolicy 来自 Minio.DataModel 命名空间，已通过 using Minio.DataModel 引入
            var policy = new PostPolicy();
            policy.SetBucket(options.BucketName);
            policy.SetKey(key); // 固定 key，客户端不能改
            policy.SetExpires(DateTime.Now.AddMinutes(15));

            // 文件大小限制：content-length-range 条件，客户端上传的 Content-Length 必须在 [min, max] 内
            if (fileSize > 0)
            {
                policy.SetContentRange(0, fileSize);
            }

            if (metadata != null && metadata.Count > 0)
            {
                foreach (var item in metadata)
                {
                    if (item.Value != null)
                    {
                        policy.SetUserMetadata(item.Key, item.Value.ToString());
                    }
                }
            }

            // PresignedPostPolicyAsync 在 v6 里只有一个参数的重载，无 CancellationToken
            var (uri, formData) = await minio.PresignedPostPolicyAsync(policy);

            var result = new ClientUploadInfo
            {
                UploadUrl = uri.ToString(),
                Method = "POST",
                FileFormName = "file",
                Headers = null,
                FormFields = new Dictionary<string, string>(formData)
            };

            return result;
        }

        public async Task<bool> Exists(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var minio = CreateClient();

                var args = new StatObjectArgs()
                   .WithBucket(options.BucketName)
                   .WithObject(key);

                await minio.StatObjectAsync(args, cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogInformation(ex, "check minio object exists failed");
                return false;

            }
        }
    }
}

using System.Net;
using Minio;
using Minio.DataModel.Args;

namespace YS.Knife.FileStorage.Minio
{
    [Service]
    [AutoConstructor]
    public partial class MinioFileStorage : IFileStorageService
    {
        private MinioOptions options;

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
        public async Task<FileObject> PutObject(string key, Stream content, IDictionary<string, object> metadata)
        {
            try
            {
                var (endPoint, ssl) = GetEndPoint();
                var minio = new MinioClient()
                        .WithEndpoint(endPoint)
                        .WithCredentials(options.AccessKeyId, options.AccessKeySecret)
                        .WithSSL(ssl)
                        .Build();

                var arg = new PutObjectArgs().WithBucket(options.BucketName)
                        .WithObject(key)
                        .WithStreamData(content)
                        .WithObjectSize(content.Length)
                        .WithHeaders(ConvertMeta(metadata));
                var res = await minio.PutObjectAsync(arg);

                return new FileObject
                {
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

        public async Task<FileObject> MoveObject(string key, string newKey)
        {
            try
            {
                var (endPoint, ssl) = GetEndPoint();
                var minio = new MinioClient()
                        .WithEndpoint(endPoint)
                        .WithCredentials(options.AccessKeyId, options.AccessKeySecret)
                        .WithSSL(ssl)
                        .Build();

                var arg = new CopyObjectArgs()
                        .WithBucket(options.BucketName)
                        .WithObject(newKey)
                        .WithCopyObjectSource(new CopySourceObjectArgs().WithBucket(options.BucketName).WithObject(key));

                await minio.CopyObjectAsync(arg);

                var removeArg = new RemoveObjectArgs().WithBucket(options.BucketName).WithObject(key);
                await minio.RemoveObjectAsync(removeArg);

                return new FileObject
                {
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
                PublicUrl = $"{options.PublicPoint}/{key}"
            });
        }
    }
}

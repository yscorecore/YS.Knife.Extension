using System.Security.Cryptography;
using System.Text;
using Aliyun.OSS;

namespace YS.Knife.FileStorage.AliyunOss
{
    [Service]
    [DictionaryKey("alioss")]
    [AutoConstructor]
    public partial class AliOssFileStorage : IFileStorageService
    {
        private readonly AliyunOssOptions ossConfiguration;

        public Task<bool> Exists(string key, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            var res = client.DoesObjectExist(ossConfiguration.BucketName, key);
            return Task.FromResult(res);
        }

        public Task<ClientUploadInfo> GetClientUploadInfo(string key, long fileSize, IDictionary<string, object> metadata, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();
            var expiration = DateTime.Now.AddMinutes(15);

            // === Aliyun OSS POST 表单签名规则 ===
            // policy 是 JSON，包含 expiration + conditions（必须包含 ["eq","$key",key]）
            // 签名 = base64(HMAC-SHA1(AccessKeySecret, base64(UTF8(policyJSON))))
            // 上传 URL = https://{bucket}.{endpoint}/ (bucket 域名，不带 object path)
            // 表单必须按顺序提交：key → policy → OSSAccessKeyId → signature → 其他 x-oss-meta-* → file(最后!)

            // 1. 构建 policy JSON
            // conditions 里固定 key，同时把所有 x-oss-meta-* 字段加入 starts-with 条件
            var condList = new List<object> { new object[] { "eq", "$key", key } };

            // 文件大小限制：content-length-range 条件，客户端上传的 Content-Length 必须在 [min, max] 内
            if (fileSize > 0)
            {
                condList.Add(new object[] { "content-length-range", 0, fileSize });
            }

            var metaFields = new Dictionary<string, string>();
            if (metadata != null)
            {
                foreach (var item in metadata)
                {
                    if (item.Value != null)
                    {
                        var val = item.Value.ToString();
                        condList.Add(new object[] { "starts-with", $"$x-oss-meta-{item.Key}", "" });
                        metaFields[$"x-oss-meta-{item.Key}"] = val;
                    }
                }
            }

            // policy JSON
            var policyJson = new
            {
                expiration = expiration.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                conditions = condList
            };
            var policyJsonStr = System.Text.Json.JsonSerializer.Serialize(policyJson);

            // 2. base64(UTF8(policyJSON))
            var policyBytes = Encoding.UTF8.GetBytes(policyJsonStr);
            var policyBase64 = Convert.ToBase64String(policyBytes);

            // 3. HMAC-SHA1 签名 AccessKeySecret 对 policyBase64
            string signature;
            using (var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(ossConfiguration.AccessKeySecret)))
            {
                var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(policyBase64));
                signature = Convert.ToBase64String(signatureBytes);
            }

            // 4. 上传 URL = protocol + bucket + host（注意 Endpoint 可能自带协议前缀，需先剥离）
            var (protocol, host) = GetEndpointInfo();
            var uploadUrl = $"{protocol}{ossConfiguration.BucketName}.{host}/";

            // 5. 组装 form 字段（顺序很重要：OSS 按顺序校验）
            var formFields = new Dictionary<string, string>
            {
                { "key", key },
                { "policy", policyBase64 },
                { "OSSAccessKeyId", ossConfiguration.AccessKeyId },
                { "signature", signature },
            };
            foreach (var field in metaFields)
            {
                formFields[field.Key] = field.Value;
            }

            var result = new ClientUploadInfo
            {
                UploadUrl = uploadUrl,
                Method = "POST",
                FileFormName = "file",
                Headers = null,
                FormFields = formFields
            };

            return Task.FromResult(result);
        }

        public Task<FileObject> GetObject(string key)
        {
            return Task.FromResult(new FileObject
            {
                PublicUrl = $"{ossConfiguration.PublicPoint}/{key}",
                Key = key,
                FileName = Path.GetFileName(key)

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
                    PublicUrl = $"{ossConfiguration.PublicPoint}/{newKey}",
                    FileName = Path.GetFileName(newKey),
                    Key = key,
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
                    FileName = Path.GetFileName(key),
                    PublicUrl = $"{ossConfiguration.PublicPoint}/{key}"
                };
                return Task.FromResult(imageInfo);
            }
            else
            {
                throw new Exception($"put oss object error, oss response code {result.HttpStatusCode}, requestid {result.RequestId}.");
            }
        }

        /// <summary>
        /// 从 Endpoint 配置中提取协议和纯 host。
        /// 配置允许带 https:// 或 http:// 前缀（如 https://oss-cn-hangzhou.aliyuncs.com），
        /// 返回 (protocol, host) 用于后续 bucket 子域名拼接。
        /// </summary>
        private (string protocol, string host) GetEndpointInfo()
        {
            var ep = ossConfiguration.Endpoint;
            if (ep.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return ("https://", ep["https://".Length..]);
            }
            else if (ep.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                return ("http://", ep["http://".Length..]);
            }
            return ("https://", ep);
        }

        private OssClient CreateClient()
        {
            return new OssClient(ossConfiguration.Endpoint, ossConfiguration.AccessKeyId, ossConfiguration.AccessKeySecret);
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

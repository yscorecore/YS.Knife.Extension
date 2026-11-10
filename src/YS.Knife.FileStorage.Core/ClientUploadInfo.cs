
namespace YS.Knife.FileStorage
{
    public record ClientUploadInfo
    {
        /// <summary>上传目标地址（PUT 时是带签名的完整 URL；POST 表单时是 bucket endpoint）</summary>
        public string UploadUrl { get; set; }

        /// <summary>PUT 方式的 HTTP Headers（Content-Type、x-oss-meta-* 等）</summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>POST 表单方式需额外提交的 name=value（policy、signature、x-oss-meta-* 等）；PUT 方式为 null</summary>
        public Dictionary<string, string> FormFields { get; set; }

        /// <summary>客户端上传文件的字段名（默认 "file"，可被 FileCategory.FileFormName 覆盖）</summary>
        public string FileFormName { get; set; }

        /// <summary>"PUT" 或 "POST"</summary>
        public string Method { get; set; }
    }
}

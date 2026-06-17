namespace YS.Knife.AI.Core
{
    public record class AiInputData
    {
        public object Data { get; private set; } = null!;

        public AiDataType Type { get; private set; }

        public string? MediaType { get; private set; } = null!;
        public static AiInputData From(string urlOrPathOrBase64)
        {
            _ = urlOrPathOrBase64 ?? throw new ArgumentNullException(nameof(urlOrPathOrBase64));
            if (urlOrPathOrBase64.StartsWith("https://") || urlOrPathOrBase64.StartsWith("http://"))
            {
                return new AiInputData
                {
                    Data = urlOrPathOrBase64,
                    Type = AiDataType.Url
                };
            }
            else if (File.Exists(urlOrPathOrBase64))
            {
                var bytes = File.ReadAllBytes(urlOrPathOrBase64);
                return new AiInputData
                {
                    Data = bytes,
                    Type = AiDataType.ByteArray,
                    MediaType = ContentTypeMappings.GetContentTypeByFileName(urlOrPathOrBase64)
                };
            }
            else if (urlOrPathOrBase64.StartsWith("data:"))
            {
                var startIndex = urlOrPathOrBase64.IndexOf(";base64,");
                if (startIndex < 0)
                {
                    throw new ArgumentException("Invalid base64 string format.");
                }
                var mediaType = startIndex > 0 ? urlOrPathOrBase64.Substring(5, startIndex - 5) : null;
                var base64Data = urlOrPathOrBase64.Substring(startIndex + 8);
                if (!base64Data.IsValidBase64Strict())
                {
                    throw new Exception("The provided string is not a valid base64 encoded data.");
                }
                return new AiInputData
                {
                    Data = base64Data,
                    Type = AiDataType.Base64,
                    MediaType = mediaType,
                };
            }
            else
            {
                if (!urlOrPathOrBase64.IsValidBase64Strict())
                {
                    throw new Exception("The provided string is not a valid base64 encoded data.");
                }
                return new AiInputData
                {
                    Data = urlOrPathOrBase64,
                    Type = AiDataType.Base64
                };
            }

        }
        public static AiInputData FromStream(Stream stream, string? mediaType = default)
        {
            return new AiInputData
            {
                MediaType = mediaType,
                Type = AiDataType.Stream,
                Data = stream
            };
        }
        public static AiInputData FromBytes(byte[] bytes, string? mediaType = default)
        {
            return new AiInputData
            {
                MediaType = mediaType,
                Type = AiDataType.ByteArray,
                Data = bytes
            };
        }
        public static AiInputData FromStreamBody(StreamBody streamBody)
        {
            return new AiInputData
            {
                MediaType = streamBody.ContentType,
                Type = AiDataType.Stream,
                Data = streamBody.Stream
            };
        }

    }
}

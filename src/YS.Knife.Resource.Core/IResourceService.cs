using System.Threading.Tasks;

namespace YS.Knife.Resource
{
    public interface IResourceService
    {
        Task<Stream> Load(string resourceUri);
    }

    public static class ResourceServiceExtensions
    {
        public static async Task<byte[]> LoadBytes(this IResourceService resourceService, string resourceUri, CancellationToken cancellationToken = default)
        {
            using var stream = await resourceService.Load(resourceUri);
            return await StreamToByteArrayManual(stream, cancellationToken);
        }

        private static async Task<byte[]> StreamToByteArrayManual(Stream input, CancellationToken cancellationToken = default)
        {
            if (input is MemoryStream ms)
            {
                return ms.ToArray();
            }
            else
            {
                using MemoryStream ms2 = new MemoryStream();
                byte[] buffer = new byte[4096];
                int bytesRead;
                // 循环读取输入流的数据，直到读取完所有数据
                while ((bytesRead = await input.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    ms2.Write(buffer, 0, bytesRead);
                }
                return ms2.ToArray();
            }


        }
    }
}

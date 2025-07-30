namespace YS.Knife.BarCode
{
    public interface IBarCodeService
    {
        Task<Stream> GenerateBarCode(BarCode barcode);
        public record BarCode
        {
            public string Content { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
        }
    }
    public static class BarCodeServiceExtensions
    {
        public static Task<Stream> GenerateBarCode(this IBarCodeService service, string content, int width, int height)
        {
            return service.GenerateBarCode(new IBarCodeService.BarCode { Content = content, Width = width, Height = height });
        }
        public static async Task GenerateBarCodeToFile(this IBarCodeService service, string content, int width, int height, string outputFile)
        {
            using var stream = await service.GenerateBarCode(new IBarCodeService.BarCode { Content = content, Width = width, Height = height });
            using var writer = File.OpenWrite(outputFile);
            await stream.CopyToAsync(writer);
        }
    }


}

namespace YS.Knife.QrCode
{
    public static class QrCodeServiceExtensions
    {
        public static Task<Stream> GenerateBarCode(this IQrCodeService service, string content, int size)
        {
            return service.GenerateQrCode(new IQrCodeService.QrCode { Content = content, Size = size });
        }
        public static async Task GenerateBarCodeToFile(this IQrCodeService service, string content, int size, string outputFile)
        {
            using var stream = await service.GenerateQrCode(new IQrCodeService.QrCode { Content = content, Size = size });
            using var writer = File.OpenWrite(outputFile);
            await stream.CopyToAsync(writer);
        }
    }
}

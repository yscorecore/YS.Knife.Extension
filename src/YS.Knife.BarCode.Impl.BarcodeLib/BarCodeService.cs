using BarcodeStandard;
using SkiaSharp;

namespace YS.Knife.BarCode.Impl.BarcodeLib
{
    [AutoConstructor]
    [Service]
    public partial class BarCodeService : IBarCodeService
    {
        public Task<Stream> GenerateBarCode(IBarCodeService.BarCode barcode)
        {
            var b = new Barcode();
            b.IncludeLabel = false;
            var img = b.Encode(BarcodeStandard.Type.Code128, barcode.Content, SKColors.Black, SKColors.White, barcode.Width, barcode.Height);
            using var data = img.Encode(SKEncodedImageFormat.Jpeg, 100); // 编码为jpg格式
            var memoryStream = new MemoryStream();
            data.SaveTo(memoryStream);
            memoryStream.Position = 0; // 重置流的位置
            return Task.FromResult<Stream>(memoryStream);


        }
    }
}

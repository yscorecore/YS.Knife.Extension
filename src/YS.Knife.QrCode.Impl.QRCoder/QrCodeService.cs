using System;
using System.Reflection.Emit;
using QRCoder;
using static QRCoder.QRCodeGenerator;

namespace YS.Knife.QrCode.Impl.QRCoder
{
    [AutoConstructor]
    [Service]
    public partial class QrCodeService : IQrCodeService
    {
        public Task<Stream> GenerateQrCode(IQrCodeService.QrCode barcode)
        {
            using QRCodeGenerator qrGenerator = new QRCodeGenerator();
            using QRCodeData qrCodeData = qrGenerator.CreateQrCode(barcode.Content, QRCodeGenerator.ECCLevel.M);
            using PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeImage = qrCode.GetGraphic(barcode.Size, true);
            var steam = new MemoryStream(qrCodeImage);
            steam.Position = 0;
            return Task.FromResult<Stream>(steam);
        }
    }
}

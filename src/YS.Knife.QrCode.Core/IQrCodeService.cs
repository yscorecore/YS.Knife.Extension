namespace YS.Knife.QrCode
{
    public interface IQrCodeService
    {

        Task<Stream> GenerateQrCode(QrCode barcode);
        public record QrCode
        {
            public string Content { get; set; }
            public int Size { get; set; }
        }
    }
}

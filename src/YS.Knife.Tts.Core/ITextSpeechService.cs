namespace YS.Knife.Tts
{
    public interface ITextSpeechService
    {
        Task<TextSpeechRes> TextToSpeech(TextSpeechReq req, CancellationToken cancellationToken);
    }

    public record TextSpeechReq
    {
        public string Text { get; set; }
    }
    public record TextSpeechRes
    {
        public byte[] Audio { get; set; }
    }
}

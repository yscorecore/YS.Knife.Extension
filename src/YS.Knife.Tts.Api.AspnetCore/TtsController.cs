using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YS.Knife.Tts;

namespace YS.Knife.EnumCode.AspnetCore
{

    [ApiController]
    [Route("[controller]")]
    [AutoConstructor]
    public partial class TtsController : ControllerBase
    {
        private readonly ITextSpeechService textSpeechService;
        [HttpGet]
        public Task<TextSpeechRes> TextToSpeech([FromQuery] TextSpeechReq req, CancellationToken cancellationToken)
        {
            return textSpeechService.TextToSpeech(req, cancellationToken);
        }

        [HttpGet]
        [Route("download")]
        public async Task<IActionResult> DownLoad([FromQuery] TextSpeechReq req, CancellationToken cancellationToken)
        {
            var data = await textSpeechService.TextToSpeech(req, cancellationToken);
            return File(data.Audio, "audio/mpeg", $"{DateTime.Now:YYYYMMddHHmmss_fff}.mp3");
        }
    }
}

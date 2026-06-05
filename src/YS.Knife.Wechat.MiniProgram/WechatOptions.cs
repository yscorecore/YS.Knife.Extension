using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YS.Knife.Wechat.MiniProgram
{
    [Options]
    public class WechatOptions
    {
        [Required]
        public string AppId { get; set; }
        [Required]
        public string AppSecret { get; set; }
        /// <summary>
        /// developer为开发版；trial为体验版；formal为正式版；默认为正式版，(通知消息时使用）
        /// </summary>
        [Required]
        public string MiniprogramState { get; set; } = "release";
        /// <summary>
        /// release=正式版，trial=体验版，develop=开发版
        /// </summary>
        [Required]
        public string Env { get; set; } = "release";

    }
}

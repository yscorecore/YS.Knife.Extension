using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YS.Knife.Wechat.MiniProgram
{

    [Serializable]
    public class WechatException : Exception
    {
        public WechatException() { }
        public WechatException(int code, string message) : base(message)
        {
            this.ErrorCode = code;
        }
        public WechatException(string message, Exception inner) : base(message, inner) { }

        public int ErrorCode { get; set; }
    }
}

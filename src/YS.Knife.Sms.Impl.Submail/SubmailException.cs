namespace YS.Knife.Sms.Impl.Submail
{
    [Serializable]
    public class SubmailException : Exception
    {
        public SubmailException() { }
        public SubmailException(string message) : base(message) { }
        public SubmailException(string message, Exception inner) : base(message, inner) { }
        protected SubmailException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}

namespace YS.Knife.Sms.Impl.Submail
{
    public static class BaseResponseExtensions
    {
        public static void EnsureSuccessCode(this SubmailBaseResponse response)
        {
            if (response.Status != "success")
            {
                throw new SubmailException($"Code:{response.Code},Msg:{response.Msg}");
            }
        }
    }
}

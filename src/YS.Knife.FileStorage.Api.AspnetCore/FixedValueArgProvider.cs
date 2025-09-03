using YS.Knife.FileStorage;

namespace YS.Knife.FileStorage.Api.AspnetCore
{
    public class FixedValueArgProvider : ISystemArgProvider
    {
        private readonly object value;
        public FixedValueArgProvider(object value)
        {
            this.value = value;
        }
        public string DefaultFormatter => string.Empty;
        public object GetValue()
        {
            return value;
        }
    }

}

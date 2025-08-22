using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;

namespace YS.Knife.DataItem.Api.AspnetCore.Internal
{

    class DataItemValueQueryStringProvider : QueryStringValueProvider
    {
        public DataItemValueQueryStringProvider(string itemName, IQueryCollection values) :
            base(BindingSource.Query, values, null)
        {
            ItemName = itemName;
        }

        public string ItemName { get; }
        public override ValueProviderResult GetValue(string key)
        {
            var namedKey = $"{ItemName}.{key}";
            var namedValue = base.GetValue(namedKey);
            var commonValue = base.GetValue(key);
            if (namedValue == ValueProviderResult.None && commonValue == ValueProviderResult.None)
            {
                return ValueProviderResult.None;
            }
            return new ValueProviderResult(StringValues.Concat(namedValue.Values, commonValue.Values), Culture);
        }
        public override IDictionary<string, string> GetKeysFromPrefix(string prefix)
        {
            var namedKey = $"{ItemName}.{prefix}";
            var commonValue = base.GetKeysFromPrefix(prefix);
            var namedValue = base.GetKeysFromPrefix(namedKey);
            if (commonValue.Count == 0)
            {
                return namedValue;
            }
            else
            {
                foreach (var (k, v) in namedValue)
                {
                    commonValue[k] = v;
                }
                return commonValue;
            }
        }
        public override bool ContainsPrefix(string prefix)
        {
            var namedKey = $"{ItemName}.{prefix}";
            return base.ContainsPrefix(namedKey) || base.ContainsPrefix(prefix);
        }
    }
}


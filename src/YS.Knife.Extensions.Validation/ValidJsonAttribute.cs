using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

namespace System.ComponentModel.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
        AllowMultiple = false)]
    public class ValidJsonAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var jsonString = value as string;
            return jsonString is null || CanParseJson(jsonString);
        }

        private static bool CanParseJson(string jsonString)
        {
            try
            {
                JsonNode.Parse(jsonString);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

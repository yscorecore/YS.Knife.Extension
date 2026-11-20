
namespace YS.Knife.FileStorage
{
    public interface ITemplatePlaceholder
    {
        string FillPlaceholder(string placeHolder, IDictionary<string, string> userArgs, IDictionary<string, ISystemArgProvider> systemArgs);
    }
}

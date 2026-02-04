namespace YS.Knife.KeyValue
{
    public interface IKeyValueGroup
    {
        string BuildUniqueKey(string key);
    }
    public abstract class KeyValueGroup : IKeyValueGroup
    {
        public abstract string BuildUniqueKey(string key);

    }
}

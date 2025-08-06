namespace YS.Knife.KeyValue
{
    public abstract class KeyValueGroup
    {
        public abstract Task<string> GetKeyPrefix();

    }
}

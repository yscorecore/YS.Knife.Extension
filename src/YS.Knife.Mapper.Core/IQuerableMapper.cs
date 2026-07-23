namespace YS.Knife.Mapper
{
    public interface IQuerableMapper
    {
        IQueryable<To> MapQuery<From, To>(IQueryable<From> source) where To : new();
    }
    public interface IConvertMapper
    {
        To Convert<From, To>(From source) where To : new();
    }
    public interface ICopyMapper
    {
        void Copy<From, To>(From source, To target, Action<object>? onAddItem = null, Action<object>? onRemoveItem= null) where To:class;
    }
}

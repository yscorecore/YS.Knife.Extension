namespace YS.Knife
{
    public enum CollectionSaveMode
    {
        //按照Key为唯一键，只追加新的元素
        Append = 0,
        //按照Key为唯一键，追加新的元素，并且修改相同key的元素
        Merge = 1,
        //按照Key为唯一键，追加新的元素，修改相同key的元素，另外删除不在传入Key的元素
        Update = 2
    }
}

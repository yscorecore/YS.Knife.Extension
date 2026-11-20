namespace YS.Knife.FileStorage
{
    public interface IFileCategoryProvider
    {
        Task<FileCategory> CreateCategory(string categoryName);
    }

}

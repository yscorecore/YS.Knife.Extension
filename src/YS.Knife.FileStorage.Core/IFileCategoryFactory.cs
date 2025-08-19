namespace YS.Knife.FileStorage
{
    public interface IFileCategoryFactory
    {
        Task<FileCategory> CreateCategory(string categoryName);

    }

}

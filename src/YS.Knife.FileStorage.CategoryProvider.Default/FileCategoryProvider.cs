
namespace YS.Knife.FileStorage.CategoryProvider.Default
{
    [Service]
    [AutoConstructor]
    public partial class FileCategoryProvider : IFileCategoryProvider
    {
        private readonly FileCategoriesOptions options;
        public Task<FileCategory> CreateCategory(string categoryName)
        {
            if (options.TryGetValue(categoryName, out var category))
            {
                return Task.FromResult(category);
            }
            else
            {
                return Task.FromResult<FileCategory>(null);
            }
        }
    }
}

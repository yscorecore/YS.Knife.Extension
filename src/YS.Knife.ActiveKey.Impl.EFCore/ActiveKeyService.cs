using Microsoft.EntityFrameworkCore;
using YS.Knife.ActiveKey.Core;
using YS.Knife.ActiveKey.Entity.EFCore;
using YS.Knife.Entity;

namespace YS.Knife.ActiveKey.Impl.EFCore
{
    [Service]
    [AutoConstructor]
    public partial class ActiveKeyService : IActiveKeyService
    {
        private readonly IEntityStore<ActiveKeyEntity<Guid>> _entityStore;
        public async Task Consume(string code, Dictionary<string, object> consumeContext, CancellationToken cancellationToken)
        {
            var current = await _entityStore.Current.FilterDeleted().Where(p => p.Code == code).FindOrThrowAsync(cancellationToken);
            current.LatestConsumeTime = DateTime.Now;
            current.ConsumeContext = consumeContext;
            await _entityStore.SaveChangesAsync(cancellationToken);
        }

        public Task<string> Request(string owner, Dictionary<string, object> requestContext, CancellationToken cancellationToken)
        {


            throw new NotImplementedException();
        }
    }
}

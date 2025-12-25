using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using YS.Knife.Entity;
using YS.Knife.Query;
using YS.Knife.Service;

namespace YS.Knife.EFCore
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class EFEntityStoreAttribute : KnifeAttribute
    {
        public Type EntityType { get; private set; }
        public bool RegisterQueryPageService { get; set; } = false;
        public bool RegisterCudService { get; set; } = false;
        public EFEntityStoreAttribute(Type type) : base(typeof(DbContext))
        {
            this.EntityType = type;
        }

        public override void RegisterService(IServiceCollection services, IRegisterContext context, Type declareType)
        {
            services.AddScoped(typeof(IEntityStore<>).MakeGenericType(this.EntityType), (sp) =>
            {
                var entityStoreType = typeof(EFEntityStore<,>).MakeGenericType(this.EntityType, declareType);
                return ActivatorUtilities.CreateInstance(sp, entityStoreType);
            });
            if (RegisterCudService)
            {
                var entityKeyType = GetEntityKeyType() ?? throw new Exception($"Entity type '{EntityType.FullName}' should implement the interface '{typeof(IEntity<>).FullName}'");
                var serviceType = typeof(ICudServiceApi<,>).MakeGenericType(this.EntityType, entityKeyType);
                var instanceType = typeof(EFEntityStoreCudService<,>).MakeGenericType(this.EntityType, entityKeyType);
                services.AddScoped(serviceType, instanceType);
            }
            if (RegisterQueryPageService)
            {
                var serviceType = typeof(IQueryPageApi<>).MakeGenericType(this.EntityType);
                var instanceType = typeof(EFEntityStoreQueryService<>).MakeGenericType(this.EntityType);
                services.AddScoped(serviceType, instanceType);
            }
        }
        private Type GetEntityKeyType()
        {
            var entityInterface = this.EntityType.GetInterfaces()
                 .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<>));
            if (entityInterface != null)
            {
                return entityInterface.GenericTypeArguments[0];
            }
            return null;
        }
    }
    [AutoConstructor]
    public partial class EFEntityStoreQueryService<T> : IQueryPageApi<T>
        where T : class, new()
    {
        private readonly IEntityStore<T> _entityStore;
        public Task<PagedList<T>> QueryPagedList(LimitQueryInfo req, CancellationToken cancellationToken = default)
        {
            var query = _entityStore.Current.AsNoTracking();
            if (typeof(ISoftDeleteEntity).IsAssignableFrom(typeof(T)))
            {
                query = query.Where(e => !((ISoftDeleteEntity)e).IsDeleted);
            }
            if (typeof(ISortableEntity).IsAssignableFrom(typeof(T)))
            {
                query = query.OrderBy(e => ((ISortableEntity)e).Order);
            }
            return query.QueryPageAsync(req, cancellationToken);
        }
    }
    [AutoConstructor]
    public partial class EFEntityStoreCudService<T, TKey> : ICudServiceApi<T, TKey>
        where T : class, IEntity<TKey>
        where TKey : notnull
    {
        private readonly IEntityStore<T> _entityStore;
        public async Task<TKey[]> Create(T[] dtos, CancellationToken token = default)
        {
            _entityStore.AddRange(dtos);
            await _entityStore.SaveChangesAsync(token);
            return dtos.Select(p => p.Id).ToArray();
        }

        public async Task Delete(TKey[] ids, CancellationToken token = default)
        {
            var entitys = await _entityStore.Current.FindArrayOrThrowAsync(ids, token);
            _entityStore.DeleteRange(entitys);
            await _entityStore.SaveChangesAsync(token);
        }

        public async Task Update(TKey[] ids, T dto, CancellationToken token = default)
        {
            var entitys = await _entityStore.Current.FindArrayOrThrowAsync(ids, token);
            var properties = typeof(T).GetProperties()
                .Where(p => p.CanRead && p.CanWrite && p.Name != nameof(IEntity<string>.Id))
                .ToList(); ;
            foreach (var entity in entitys)
            {
                foreach (var p in properties)
                {
                    p.SetValue(p, p.GetValue(dto));
                }
            }
            await _entityStore.SaveChangesAsync(token);
        }
    }
}

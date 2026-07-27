using System.Collections.Generic;
using System.Reflection;
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
        public EFEntityStoreAttribute() : base(typeof(DbContext))
        {
            
        }
        public EFEntityStoreAttribute(Type type) : base(typeof(DbContext))
        {
            this.EntityType = type;
        }
        /// <summary>
        /// 从 DbContext 类型中获取所有 DbSet 属性的实体类型
        /// </summary>
        private static IEnumerable<Type> GetDbSetEntityTypes(Type dbContextType)
        {
            if (dbContextType == null)
                throw new ArgumentNullException(nameof(dbContextType));

            // 获取所有公共实例属性
            var properties = dbContextType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                var propType = prop.PropertyType;

                // 检查是否为泛型类型
                if (!propType.IsGenericType)
                    continue;

                // 获取泛型定义，判断是否为 DbSet<> 或 IDbSet<>
                var genericDef = propType.GetGenericTypeDefinition();
                if (genericDef == typeof(DbSet<>))
                {
                    // 取出第一个泛型参数（即实体类型）
                    var entityType = propType.GetGenericArguments()[0];
                    yield return entityType;
                }
            }
        }
        public override void RegisterService(IServiceCollection services, IRegisterContext context, Type declareType)
        {
            if (this.EntityType == null)
            {
                foreach(var type in GetDbSetEntityTypes(declareType))
                {
                    services.AddScoped(typeof(IEntityStore<>).MakeGenericType(type), (sp) =>
                    {
                        var entityStoreType = typeof(EFEntityStore<,>).MakeGenericType(type, declareType);
                        return ActivatorUtilities.CreateInstance(sp, entityStoreType);
                    });
                }
            }
            else
            {
                services.AddScoped(typeof(IEntityStore<>).MakeGenericType(this.EntityType), (sp) =>
                {
                    var entityStoreType = typeof(EFEntityStore<,>).MakeGenericType(this.EntityType, declareType);
                    return ActivatorUtilities.CreateInstance(sp, entityStoreType);
                });
            }
          
        }
        //private Type GetEntityKeyType()
        //{
        //    var entityInterface = this.EntityType.GetInterfaces()
        //         .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<>));
        //    if (entityInterface != null)
        //    {
        //        return entityInterface.GenericTypeArguments[0];
        //    }
        //    return null;
        //}
    }
    //[AutoConstructor]
    //public partial class EFEntityStoreQueryService<T> : IQueryPageApi<T>
    //    where T : class, new()
    //{
    //    private readonly IEntityStore<T> _entityStore;
    //    public Task<PagedList<T>> QueryPagedList(LimitQueryInfo req, CancellationToken cancellationToken = default)
    //    {
    //        var query = _entityStore.Current.AsNoTracking();
    //        if (typeof(ISoftDeleteEntity).IsAssignableFrom(typeof(T)))
    //        {
    //            query = query.Where(e => !((ISoftDeleteEntity)e).IsDeleted);
    //        }
    //        if (typeof(ISortableEntity).IsAssignableFrom(typeof(T)))
    //        {
    //            query = query.OrderBy(e => ((ISortableEntity)e).Order);
    //        }
    //        return query.QueryPageAsync(req, cancellationToken);
    //    }
    //}
    //[AutoConstructor]
    //public partial class EFEntityStoreCudService<T, TKey> : ICudServiceApi<T, TKey>
    //    where T : class, IEntity<TKey>
    //    where TKey : notnull
    //{
    //    private readonly IEntityStore<T> _entityStore;
    //    public async Task<TKey[]> Create(T[] dtos, CancellationToken token = default)
    //    {
    //        _entityStore.AddRange(dtos);
    //        await _entityStore.SaveChangesAsync(token);
    //        return dtos.Select(p => p.Id).ToArray();
    //    }

    //    public async Task Delete(TKey[] ids, CancellationToken token = default)
    //    {
    //        var entitys = await _entityStore.Current.FindArrayOrThrowAsync(ids, token);
    //        if (typeof(ISoftDeleteEntity).IsAssignableFrom(typeof(T)))
    //        {
    //            entitys.OfType<ISoftDeleteEntity>().ToList().ForEach(p => { p.IsDeleted = true; });
    //        }
    //        else
    //        {
    //            _entityStore.DeleteRange(entitys);
    //        }

    //        await _entityStore.SaveChangesAsync(token);
    //    }

    //    public async Task Update(TKey[] ids, T dto, CancellationToken token = default)
    //    {
    //        var entitys = await _entityStore.Current.FindArrayOrThrowAsync(ids, token);
    //        var properties = typeof(T).GetProperties()
    //            .Where(p => p.CanRead && p.CanWrite && p.Name != nameof(IEntity<string>.Id))
    //            .ToList(); ;
    //        foreach (var entity in entitys)
    //        {
    //            foreach (var p in properties)
    //            {
    //                p.SetValue(p, p.GetValue(dto));
    //            }
    //        }
    //        await _entityStore.SaveChangesAsync(token);
    //    }
    //}
}

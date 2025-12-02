using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YS.Knife.Entity;
using YS.Knife.LogicRoles;
using YS.Knife.NotifyMessages.Entity.EFCore;
using YS.Knife.Query;

namespace YS.Knife.NotifyMessages.Impl.EFCore
{
    [Service]
    [AutoConstructor]
    [Mapper(typeof(MessageTopicEntity<Guid>), typeof(MessageTopicDto<Guid>), MapperType = MapperType.Query)]
    [Mapper(typeof(MessageChannelEntity<Guid>), typeof(MessageChannelDto<Guid>), MapperType = MapperType.Query)]

    public partial class NotifyMessageService : INotifyMessageService
    {
        private readonly IEntityStore<MessageTopicEntity<Guid>> entityStore;
        private readonly IEntityStore<MessageChannelEntity<Guid>> channels;
        private readonly MessageOptions messageOptions;
        private readonly IEnumerable<ILogicRoleProvider> logicRoleProviders;
        public async Task<PagedList<MessageTopicDto<Guid>>> QueryPagedList(LimitQueryInfo req, CancellationToken cancellationToken = default)
        {
            var roles = await logicRoleProviders.GetAllRoles(messageOptions.MainLogicRole);
            var mainRole = roles.Last();
            return await entityStore.Current
                 .Where(p => p.Owner == mainRole)
                 .To<MessageTopicDto<Guid>>()
                 .QueryPageAsync(req, cancellationToken);
        }

        public async Task<PagedList<MessageChannelDto<Guid>>> ListMyChannels(LimitQueryInfo req, CancellationToken cancellationToken = default)
        {
            var mainLogicRole = (await logicRoleProviders.GetAllRoles(messageOptions.MainLogicRole)).Last();
            var roles = await logicRoleProviders.GetAllRoles(messageOptions.LogicRoleProviders);
            return await channels.Current.Include(p => p.Users.Where(t => t.Code == mainLogicRole))
                    .Where(p => p.Users.Any(t => roles.Contains(t.Code)))
                 .To<MessageChannelDto<Guid>>()
                 .QueryPageAsync(req, cancellationToken);
        }
    }
    [Options]
    public class MessageOptions
    {
        [Required]
        public string[] LogicRoleProviders { get; set; } = new string[0];
        public string MainLogicRole { get; set; }
    }
}

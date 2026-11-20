using System.ComponentModel.DataAnnotations;

namespace YS.Knife.Service
{
    public record IdDto<TKey> : IIdDto<TKey>
    {
        [ScaffoldColumn(false)]
        public TKey Id { get; set; }
    }
}

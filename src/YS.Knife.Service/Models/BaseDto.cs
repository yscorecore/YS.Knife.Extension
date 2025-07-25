namespace YS.Knife.Service
{

    public record BaseDto<TKey> : IdDto<TKey>
    {
        public DateTime CreateTime { get; set; }
        public string CreateUser { get; set; }
    }
}

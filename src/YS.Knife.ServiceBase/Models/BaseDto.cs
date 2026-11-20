namespace YS.Knife.Service
{
    //[Obsolete("use CreationAuditedDto")]
    public record BaseDto<TKey> : IdDto<TKey>
    {
        public DateTime CreateTime { get; set; }
        public string CreateUser { get; set; }
    }

    public record CreationAuditedDto<TKey> : IdDto<TKey>
    {
        DateTimeOffset CreationTime { get; set; }
        string Creator { get; set; }
    }
}

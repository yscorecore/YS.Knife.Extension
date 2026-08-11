namespace YS.Knife.Function
{
    public record LayerValueInfo
    {
        public LayerValueInfo()
        {

        }
        public string Key { get; set; } = null!;
        public string RoleCode { get; set; } = null!;
        public string Value { get; set; } = null!;
    }
    public record LayerValueInfo<T>
    {
        public LayerValueInfo()
        {

        }
        public string Key { get; set; } = null!;
        public string RoleCode { get; set; } = null!;
        public T Value { get; set; } = default!;
    }


}

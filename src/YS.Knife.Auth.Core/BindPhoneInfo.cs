using System.ComponentModel.DataAnnotations;

namespace YS.Knife.Auth
{
    public record BindPhoneInfo
    {
        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = null!;

        [Required]
        public string Code { get; set; } = null!;
    }
}

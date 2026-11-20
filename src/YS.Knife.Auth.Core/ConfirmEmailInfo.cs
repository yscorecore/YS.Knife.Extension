using System.ComponentModel.DataAnnotations;

namespace YS.Knife.Auth
{
    public record ConfirmEmailInfo(
        [property: Required, EmailAddress] string Email,
        [property: Required] string Code);
}

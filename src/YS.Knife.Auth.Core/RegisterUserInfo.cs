using System.ComponentModel.DataAnnotations;

namespace YS.Knife.Auth
{
    public record RegisterUserInfo(
        [property: Required, EmailAddress] string Email,
        [property: Required] string Password);
}

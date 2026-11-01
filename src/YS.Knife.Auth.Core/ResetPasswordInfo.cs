using System.ComponentModel.DataAnnotations;

namespace YS.Knife.Auth
{
    public record ResetPasswordInfo(
        [property: Required, EmailAddress] string Email,
        [property: Required] string ResetCode,
        [property: Required] string Password);
}

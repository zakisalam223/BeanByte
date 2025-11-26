// Models/FLogin.cs
using System.ComponentModel.DataAnnotations;

namespace forum_aspcore.Models
{
    public class FLogin
    {
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}

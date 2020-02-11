namespace WhyNotEarth.Meredith.App.Models.Api.v0.Authentication
{
    using System.ComponentModel.DataAnnotations;

    public class LoginModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}

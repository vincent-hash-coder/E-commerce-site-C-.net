using System;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace E_Commerce_Project_CRUD_Dapper.Models
{
    public class UserModel
    {
        public int UserID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string PasswordHash { get; set; }

        [Required]
        public string Address { get; set; }

        public string ProfileImagePath { get; set; }

        [Required]
        public string MobileNo { get; set; }

        public string Position { get; set; } = "Consumer";

        public HttpPostedFileBase ProfileImage { get; set; }
    }
}
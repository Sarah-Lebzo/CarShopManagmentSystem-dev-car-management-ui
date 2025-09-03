using System;
using System.ComponentModel.DataAnnotations;

namespace CarShopManagementSystem.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100)]
        public string Password { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        public string Role { get; set; } = "Customer";

        public bool IsAdmin { get; set; } = false;

        public DateTime RegisterDate { get; set; } = DateTime.Now;
    }
}
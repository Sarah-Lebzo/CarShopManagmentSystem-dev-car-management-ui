using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarShopManagementSystem.Models
{
    public class CreditCard
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(16, MinimumLength = 13)]
        public string CardNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string HolderName { get; set; }

        [Required]
        [Range(1, 12)]
        public int ExpiryMonth { get; set; }

        [Required]
        public int ExpiryYear { get; set; }

        [Required]
        [StringLength(4, MinimumLength = 3)]
        public string CVV { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }

        public bool IsValid { get; set; } = true;
    }
}
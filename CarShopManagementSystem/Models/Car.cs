using System;
using System.ComponentModel.DataAnnotations;

namespace CarShopManagementSystem.Models
{
    public class Car
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Model { get; set; }

        [Range(1900, 2100)]
        public int Year { get; set; }

        [StringLength(30)]
        public string Color { get; set; }

        [Range(0, 100000000)]
        [Display(Name = "Selling Price")]
        public decimal SellingPrice { get; set; }   // السعر النهائي للبيع

        [Range(0, 100000000)]
        [Display(Name = "Costing")]
        public decimal Costing { get; set; }        // تكلفة السيارة

        [Range(0, 100000000)]
        [Display(Name = "Estimated Price (Min)")]
        public decimal EstimatedPriceMin { get; set; }   // الحد الأدنى للتقدير

        [Range(0, 100000000)]
        [Display(Name = "Estimated Price (Max)")]
        public decimal EstimatedPriceMax { get; set; }   // الحد الأعلى للتقدير

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Used Car?")]
        public bool IsUsed { get; set; }

        [Range(0, 1000000)]
        public int? Mileage { get; set; }  // nullable

        [Display(Name = "Image")]
        public string ImageUrl { get; set; }

        [Display(Name = "Available")]
        public bool IsAvailable { get; set; } = true;

        [Display(Name = "Date Added")]
        public DateTime DateAdded { get; set; } = DateTime.Now;

        [Display(Name = "Sold Date")]
        public DateTime? SoldDate { get; set; }
        public int? ManufacturerId { get; set; }
        public Manufacturer Manufacturer { get; set; }

        public int? SubBrandId { get; set; }
        public SubBrand SubBrand { get; set; }

    }
}


//using System;
//using System.ComponentModel.DataAnnotations;

//namespace CarShopManagementSystem.Models
//{
//    public class Car
//    {
//        [Key]
//        public int Id { get; set; }

//        [Required]
//        [StringLength(100)]
//        public string Make { get; set; }

//        [Required]
//        [StringLength(100)]
//        public required string Model { get; set; }

//        [Required]
//        public int Year { get; set; }

//        [Required]
//        [StringLength(50)]
//        public required string Color { get; set; }

//        [Required]
//        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "decimal(18,2)")]
//        public decimal Price { get; set; }

//        [StringLength(500)]
//        public required string Description { get; set; }

//        public bool IsUsed { get; set; }

//        public int? Mileage { get; set; }

//        [StringLength(200)]
//        public required string ImageUrl { get; set; }

//        public bool IsAvailable { get; set; } = true;

//        public DateTime? SoldDate { get; set; }

//        public DateTime DateAdded { get; set; } = DateTime.Now;
//    }
//}
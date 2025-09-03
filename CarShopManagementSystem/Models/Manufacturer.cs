using System.ComponentModel.DataAnnotations;

namespace CarShopManagementSystem.Models
{
    public class Manufacturer
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = "";

        public string? LogoPath { get; set; }

        // Navigation
        public List<SubBrand> SubBrands { get; set; } = new();
    }

    public class SubBrand
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = "";

        public int ManufacturerId { get; set; }
        public Manufacturer? Manufacturer { get; set; }
    }

}

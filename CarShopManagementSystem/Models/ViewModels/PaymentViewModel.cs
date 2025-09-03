using CarShopManagementSystem.Models;

namespace CarShopManagementSystem.Models.ViewModels
{
    public class PaymentViewModel
    {
        public Booking Booking { get; set; }
        public string CardNumber { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public string CVV { get; set; }
        public string Message { get; set; }
        public string ShippingCountry { get; set; }
        public string ShippingMethod { get; set; }
    }
}
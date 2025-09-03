using System.Collections.Generic;
using CarShopManagementSystem.Models;

namespace CarShopManagementSystem.Models.ViewModels
{
    public class EmployeeReportViewModel
    {
        public int TotalCarsSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<Transaction> Transactions { get; set; }
    }
}
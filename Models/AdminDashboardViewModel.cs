using System.Collections.Generic;

namespace Book_Store.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalOrders { get; set; }
        public int TotalBooks { get; set; }
        public int TotalUsers { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<Order> RecentOrders { get; set; } = new();
        public List<Book> LowStockBooks { get; set; } = new();

        public double RevenueGrowth { get; set; }
        public double OrdersGrowth { get; set; }
        public double UsersGrowth { get; set; }
        public double BooksGrowth { get; set; }

        public List<string> RevenueLabels { get; set; } = new();
        public List<decimal> RevenueValues { get; set; } = new();

        public List<string> StatusLabels { get; set; } = new();
        public List<int> StatusValues { get; set; } = new();
    }
}

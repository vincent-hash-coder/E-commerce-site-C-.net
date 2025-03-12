using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace E_Commerce_Project_CRUD_Dapper.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public int UserID { get; set; }
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public DateTime OrderDate { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentDetails { get; set; }
        public string DeliveryStatus { get; set; }
        public int ShippingTime { get; set; }
        public DateTime? DeliveryDate { get; set; }
    }
}
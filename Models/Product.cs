using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace E_Commerce_Project_CRUD_Dapper.Models
{
    public class Product
    {

        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ImageLocation { get; set; }
        public decimal RealPrice { get; set; }
        public string Color { get; set; }
        public decimal Price { get; set; }
        public string Description_ { get; set; }
        public int Quantity { get; set; }
    }
}
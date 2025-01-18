using E_Commerce_Project_CRUD_Dapper.Models;
using System.Collections.Generic;
using System.Web.Mvc;
using Dapper;
using System.Linq;

namespace E_Commerce_Project_CRUD_Dapper.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(int NumberOfProducts = 8)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@NumberOfProducts", NumberOfProducts);
            List<Product> products = DapperORM.ReturnList<Product>("GetRandomProducts", parameters);
            foreach (var product in products)
            {
                var ImagePart = product.ImageLocation.Split(',');
                product.ImageLocation = ImagePart.FirstOrDefault();
            }
            return View(products);
        }



        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult Shop()
        {
            List<Product> products = DapperORM.ReturnList<Product>("GetAllProductList");
            foreach (var product in products)
            {
                var ImagePart = product.ImageLocation.Split(',');
                product.ImageLocation = ImagePart.FirstOrDefault();
            }
            return View(products);
        }


        public ActionResult Detail(int id = 1)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ProductID", id);
            Product product = DapperORM.ReturnSingleRow<Product>("GetAnProduct", parameters);
            string[] ImgLocation = product.ImageLocation.Split(',');
            ViewBag.ImgLocation = ImgLocation;
            return View(product);
        }
        public ActionResult Checkout()
        {
            return View();
        }

        public ActionResult Cart(string productIds = "0")
        {
            if (string.IsNullOrEmpty(productIds))
            {
                return View(new List<Whislist>());
            }
            var parameters = new DynamicParameters();
            parameters.Add("ProductIDs", productIds);
            List<Whislist> products = DapperORM.ReturnList<Whislist>("GetWishlistData", parameters);
            foreach (var product in products)
            {
                if (!string.IsNullOrEmpty(product.ImageLocation))
                {
                    var ImagePart = product.ImageLocation.Split(',');
                    product.ImageLocation = ImagePart.FirstOrDefault();
                }
            }
            return View(products);
        }



        public ActionResult Back()
        {
            return View("Index");
        }

        public ActionResult Login()
        {
            return View();
        }
        public ActionResult Signup() 
        { 
            return View(); 
        }
    }
}

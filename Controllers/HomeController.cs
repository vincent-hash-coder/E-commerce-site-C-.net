using E_Commerce_Project_CRUD_Dapper.Models;
using System.Collections.Generic;
using System.Web.Mvc;
using Dapper;
using System.Linq;
using System.IO;
using System.Data.SqlClient;
using System;
using Newtonsoft.Json;

namespace E_Commerce_Project_CRUD_Dapper.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(int NumberOfProducts = 8)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@NumberOfProducts", NumberOfProducts);
            List<Product> products = DapperORM.ReturnList<Product>("GetRandomProducts", parameters) ?? new List<Product>();
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
        public ActionResult Contact()
        {
            return View();
        }
        public ActionResult Shop(string priceFilter = null)
        {
            var productCounts = DapperORM.ReturnList<Product>("GetProductCountsByPrice");

            ViewBag.ProductCounts = productCounts.ToDictionary(pc => pc.PriceFilter, pc => pc.ProductCount);

            var parameters = new DynamicParameters();
            parameters.Add("@PriceFilter", priceFilter);

            List<Product> products = DapperORM.ReturnList<Product>("GetFilteredProductsByPrice", parameters);
            foreach (var product in products)
            {
                var ImagePart = product.ImageLocation.Split(',');
                product.ImageLocation = ImagePart.FirstOrDefault();
            }

            return View(products);
        }
        public ActionResult FilterProductsByPrice(string priceFilter = null)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@PriceFilter", priceFilter);

            List<Product> products = DapperORM.ReturnList<Product>("GetFilteredProductsByPrice", parameters);
            foreach (var product in products)
            {
                var ImagePart = product.ImageLocation.Split(',');
                product.ImageLocation = ImagePart.FirstOrDefault();
            }
            return PartialView("_ProductListPartial", products);
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
        public ActionResult Checkout(string productIds = "0")
        {
            var user = Session["User"] as UserModel;
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            if (string.IsNullOrEmpty(productIds) || productIds == "0")
            {
                System.Diagnostics.Debug.WriteLine("Cart is empty.");
                return View(new List<Product>());
            }
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("ProductIDs", productIds);

                List<Product> products = DapperORM.ReturnList<Product>("GetProductsForCheckout", parameters) ?? new List<Product>();

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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in Checkout: " + ex.Message);
                return View("Error");
            }
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
        [HttpPost]
        public JsonResult Login(string email, string password)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@Email", email);

                UserModel user = DapperORM.ReturnSingleRow<UserModel>("GetUserByEmail", param);

                if (user == null)
                {
                    return Json(new { success = false, message = "User not found!" });
                }

                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

                if (isPasswordValid)
                {
                    Session["User"] = user;

                    return Json(new { success = true, message = "Login successful!" });
                }
                else
                {
                    return Json(new { success = false, message = "Incorrect password!" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        public ActionResult Signup()
        {
            return View();
        }
        [HttpPost]
        public JsonResult Signup(UserModel user)
        {
            try
            {
                string profileImgPath = Server.MapPath("~/Profileimg");
                if (!Directory.Exists(profileImgPath))
                {
                    Directory.CreateDirectory(profileImgPath);
                }

                if (user.ProfileImage != null && user.ProfileImage.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(user.ProfileImage.FileName);
                    string savePath = Path.Combine(profileImgPath, fileName);
                    user.ProfileImage.SaveAs(savePath);
                    user.ProfileImagePath = "/Profileimg/" + fileName;
                }
                else
                {
                    user.ProfileImagePath = "/Profileimg/default.png";
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password);

                DynamicParameters param = new DynamicParameters();
                param.Add("@Name", user.Name);
                param.Add("@Email", user.Email);
                param.Add("@PasswordHash", user.PasswordHash);
                param.Add("@Address", user.Address);
                param.Add("@ProfileImagePath", user.ProfileImagePath);
                param.Add("@MobileNo", user.MobileNo);
                param.Add("@Position", user.Position);

                DapperORM.ReturnNothing("InsertUser", param);

                return Json(new { success = true, message = "Registration successful!" });
            }
            catch (SqlException ex)
            {
                if (ex.Number == 50000)
                {
                    return Json(new { success = false, message = ex.Message });
                }
                return Json(new { success = false, message = "SQL Error: " + ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        public JsonResult GetProductCounts()
        {
            var productCounts = DapperORM.ReturnList<Product>("GetProductCountsByPrice");
            var counts = productCounts.ToDictionary(pc => pc.PriceFilter, pc => pc.ProductCount);
            return Json(counts, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();

            return RedirectToAction("Index", "Home");
        }
        public ActionResult SellerSignup()
        {
            return View();
        }

        [HttpPost]
        public JsonResult SellerSignup(UserModel seller)
        {
            try
            {
                string profileImgPath = Server.MapPath("~/Profileimg");
                if (!Directory.Exists(profileImgPath))
                {
                    Directory.CreateDirectory(profileImgPath);
                }

                if (seller.ProfileImage != null && seller.ProfileImage.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(seller.ProfileImage.FileName);
                    string savePath = Path.Combine(profileImgPath, fileName);
                    seller.ProfileImage.SaveAs(savePath);
                    seller.ProfileImagePath = "/Profileimg/" + fileName;
                }
                else
                {
                    seller.ProfileImagePath = "Profileimg/default.png";
                }

                seller.PasswordHash = BCrypt.Net.BCrypt.HashPassword(seller.Password);
                seller.Position = "Seller"; 

                DynamicParameters param = new DynamicParameters();
                param.Add("@Name", seller.Name);
                param.Add("@Email", seller.Email);
                param.Add("@PasswordHash", seller.PasswordHash);
                param.Add("@Address", seller.Address);
                param.Add("@ProfileImagePath", seller.ProfileImagePath);
                param.Add("@MobileNo", seller.MobileNo);
                param.Add("@Position", seller.Position);

                DapperORM.ReturnNothing("InsertUser", param);

                return Json(new { success = true, message = "Seller Registration successful!" });
            }
            catch (SqlException ex)
            {
                if (ex.Number == 50000)
                {
                    return Json(new { success = false, message = ex.Message });
                }
                return Json(new { success = false, message = "SQL Error: " + ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        [HttpPost]
        public JsonResult PlaceOrder(OrderData orderData)
        {
            try
            {
                var user = Session["User"] as UserModel;
                if (user == null)
                {
                    return Json(new { success = false, message = "User not logged in." });
                }

                foreach (var product in orderData.Products)
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@UserID", user.UserID);
                    parameters.Add("@ProductID", product.ProductID);
                    parameters.Add("@Quantity", product.Quantity);
                    parameters.Add("@OrderDate", DateTime.Now);
                    parameters.Add("@PaymentMethod", orderData.PaymentMethod);
                    parameters.Add("@PaymentDetails", JsonConvert.SerializeObject(orderData.PaymentDetails));

                    DapperORM.ReturnNothing("CheckoutOrder", parameters);
                }

                return Json(new { success = true, message = "Order placed successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        public class OrderData
        {
            public int UserID { get; set; }
            public List<ProductOrder> Products { get; set; }
            public string PaymentMethod { get; set; }
            public dynamic PaymentDetails { get; set; }
        }
        public class ProductOrder
        {
            public int ProductID { get; set; }
            public int Quantity { get; set; }
        }
        public ActionResult OrderStatus()
        {
            var user = Session["User"] as UserModel;
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            DapperORM.ReturnNothing("UpdateAllOrderStatuses");

            var parameters = new DynamicParameters();
            parameters.Add("@UserID", user.UserID);

            List<Order> orders = DapperORM.ReturnList<Order>("GetOrderStatusByUserID", parameters) ?? new List<Order>();

            return View(orders);
        }
    }
}
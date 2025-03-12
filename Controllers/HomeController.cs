using E_Commerce_Project_CRUD_Dapper.Models;
using System.Collections.Generic;
using System.Web.Mvc;
using Dapper;
using System.Linq;
using System.IO;
using System.Data.SqlClient;
using System;

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
                    user.ProfileImagePath = "Profileimg/default.png";
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

    }
}
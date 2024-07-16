using System;
using System.Data.SqlClient;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private string connectionString = "Data Source=DESKTOP-CJT3444\\SQLEXPRESS;Initial Catalog=UniversityPerformanceTracker;Integrated Security=True";

        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                // Example: Hash password before saving (for production use appropriate password hashing)
                string hashedPassword = HashPassword(user.Password);

                // Insert into database
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sql = "INSERT INTO Users (Name, Email, Password) VALUES (@Name, @Email, @Password)";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Name", user.Name);
                        command.Parameters.AddWithValue("@Email", user.Email);
                        command.Parameters.AddWithValue("@Password", hashedPassword); // Store hashed password

                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Login");
            }
            return View(user);
        }

        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            // Example: Hash password before checking (for production use appropriate password hashing)
            string hashedPassword = HashPassword(password);

            // Retrieve from database
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "SELECT * FROM Users WHERE Email = @Email AND Password = @Password";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@Password", hashedPassword);

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // User authenticated
                            return RedirectToAction("Index", "Study");
                        }
                    }
                }
            }

            // If no user found or authentication failed
            ModelState.AddModelError("", "Invalid email or password");
            return View();
        }

        // Example: Password hashing function (replace with appropriate hashing library)
        private string HashPassword(string password)
        {
            // Example: Simple MD5 hashing (not secure, use a secure hashing algorithm)
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(password);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert byte array to hexadecimal string
                string hashedPassword = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                return hashedPassword;
            }
        }

        public ActionResult Welcome()
        {
            return View();
        }
    }
}

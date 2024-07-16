using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class BreaksController : Controller
    {
        private string connectionString = "Data Source=DESKTOP-CJT3444\\SQLEXPRESS;Initial Catalog=UniversityPerformanceTracker;Integrated Security=True";

        // GET: Breaks
        public ActionResult Index()
        {
            int userId = GetLoggedInUserId(); // Get logged-in user's ID
            List<Break> breaks = GetBreaksForUser(userId);
            return View(breaks);
        }

        // GET: Breaks/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Breaks/Create
        [HttpPost]
        public ActionResult Create(Break breakModel)
        {
            try
            {
                int userId = GetLoggedInUserId(); // Get logged-in user's ID
                SaveBreak(breakModel, userId);
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Breaks/Edit/5
        public ActionResult Edit(int id)
        {
            int userId = GetLoggedInUserId(); // Get logged-in user's ID
            var breakItem = GetBreakByIdAndUser(id, userId);
            if (breakItem == null)
            {
                return HttpNotFound();
            }
            return View(breakItem);
        }

        // POST: Breaks/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, Break breakModel)
        {
            try
            {
                int userId = GetLoggedInUserId(); // Get logged-in user's ID
                UpdateBreak(breakModel, userId);
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Breaks/Delete/5
        public ActionResult Delete(int id)
        {
            int userId = GetLoggedInUserId(); // Get logged-in user's ID
            var breakItem = GetBreakByIdAndUser(id, userId);
            if (breakItem == null)
            {
                return HttpNotFound();
            }
            return View(breakItem);
        }

        // POST: Breaks/Delete/5
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                int userId = GetLoggedInUserId(); // Get logged-in user's ID
                DeleteBreak(id, userId);
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // Helper method to get logged-in user's ID (example, replace with your authentication logic)
        private int GetLoggedInUserId()
        {
            // Example: Replace with your authentication logic to get the logged-in user's ID
            // For demo purposes, returning a hardcoded user ID
            return 1; // Replace with your actual logic
        }

        // Helper method to retrieve breaks for a specific user
        private List<Break> GetBreaksForUser(int userId)
        {
            List<Break> breaks = new List<Break>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "SELECT Id, StartTime, EndTime FROM Breaks WHERE UserId = @UserId";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Break breakItem = new Break
                            {
                                Id = (int)reader["Id"],
                                StartTime = (DateTime)reader["StartTime"],
                                EndTime = (DateTime)reader["EndTime"],
                                UserId = userId
                            };
                            breaks.Add(breakItem);
                        }
                    }
                }
            }

            return breaks;
        }

        // Helper method to save a break for a specific user
        private void SaveBreak(Break breakModel, int userId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "INSERT INTO Breaks (UserId, StartTime, EndTime) VALUES (@UserId, @StartTime, @EndTime)";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@StartTime", breakModel.StartTime);
                    command.Parameters.AddWithValue("@EndTime", breakModel.EndTime);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        // Helper method to get a break by ID for a specific user
        private Break GetBreakByIdAndUser(int id, int userId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "SELECT Id, StartTime, EndTime FROM Breaks WHERE Id = @Id AND UserId = @UserId";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@UserId", userId);

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Break breakItem = new Break
                            {
                                Id = (int)reader["Id"],
                                StartTime = (DateTime)reader["StartTime"],
                                EndTime = (DateTime)reader["EndTime"],
                                UserId = userId
                            };
                            return breakItem;
                        }
                    }
                }
            }

            return null;
        }

        // Helper method to update a break for a specific user
        private void UpdateBreak(Break breakModel, int userId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "UPDATE Breaks SET StartTime = @StartTime, EndTime = @EndTime WHERE Id = @Id AND UserId = @UserId";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@StartTime", breakModel.StartTime);
                    command.Parameters.AddWithValue("@EndTime", breakModel.EndTime);
                    command.Parameters.AddWithValue("@Id", breakModel.Id);
                    command.Parameters.AddWithValue("@UserId", userId);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        // Helper method to delete a break for a specific user
        private void DeleteBreak(int id, int userId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "DELETE FROM Breaks WHERE Id = @Id AND UserId = @UserId";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@UserId", userId);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class StudyController : Controller
    {
        private string connectionString = "Data Source=DESKTOP-CJT3444\\SQLEXPRESS;Initial Catalog=UniversityPerformanceTracker;Integrated Security=True";

        public ActionResult Index()
        {
            List<StudySession> studySessions = GetAllStudySessions();

            ViewBag.Users = new SelectList(GetAllUsers(), "Id", "Name");
            ViewBag.Subjects = new SelectList(GetAllSubjects(), "Id", "Name");
            return View(studySessions);
        }

        public ActionResult AddStudySession()
        {
            ViewBag.Users = new SelectList(GetAllUsers(), "Id", "Name");
            ViewBag.Subjects = new SelectList(GetAllSubjects(), "Id", "Name");
            return View();
        }

        [HttpPost]
        public ActionResult AddStudySession(StudySession session)
        {
            if (ModelState.IsValid)
            {
                // Check if the subject ID exists in the Subjects table
                bool subjectExists = GetAllSubjects().Any(s => s.Id == session.SubjectId);

                if (!subjectExists)
                {
                    ModelState.AddModelError("SubjectId", "Subject does not exist.");
                    ViewBag.Users = new SelectList(GetAllUsers(), "Id", "Name");
                    ViewBag.Subjects = new SelectList(GetAllSubjects(), "Id", "Name");
                    return View(session);
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sql = "INSERT INTO StudySessions (UserId, SubjectId, StartTime, EndTime) VALUES (@UserId, @SubjectId, @StartTime, @EndTime)";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", session.UserId);
                        command.Parameters.AddWithValue("@SubjectId", session.SubjectId);
                        command.Parameters.AddWithValue("@StartTime", session.StartTime);
                        command.Parameters.AddWithValue("@EndTime", session.EndTime);

                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Index");
            }

            ViewBag.Users = new SelectList(GetAllUsers(), "Id", "Name");
            ViewBag.Subjects = new SelectList(GetAllSubjects(), "Id", "Name");
            return View(session);
        }


        public ActionResult EditStudySession(int id)
        {
            StudySession session = GetStudySessionById(id);
            if (session == null)
                return HttpNotFound();

            ViewBag.Users = new SelectList(GetAllUsers(), "Id", "Name", session.UserId);
            ViewBag.Subjects = new SelectList(GetAllSubjects(), "Id", "Name", session.SubjectId);
            return View(session);
        }

        [HttpPost]
        public ActionResult EditStudySession(StudySession session)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sql = "UPDATE StudySessions SET UserId = @UserId, SubjectId = @SubjectId, StartTime = @StartTime, EndTime = @EndTime WHERE Id = @Id";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", session.Id);
                        command.Parameters.AddWithValue("@UserId", session.UserId);
                        command.Parameters.AddWithValue("@SubjectId", session.SubjectId);
                        command.Parameters.AddWithValue("@StartTime", session.StartTime);
                        command.Parameters.AddWithValue("@EndTime", session.EndTime);

                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Index");
            }

            ViewBag.Users = new SelectList(GetAllUsers(), "Id", "Name", session.UserId);
            ViewBag.Subjects = new SelectList(GetAllSubjects(), "Id", "Name", session.SubjectId);
            return View(session);
        }

        public ActionResult DeleteStudySession(int id)
        {
            StudySession session = GetStudySessionById(id);
            if (session != null)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sql = "DELETE FROM StudySessions WHERE Id = @Id";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult WeeklyProgress()
        {
            var weeklyProgress = CalculateWeeklyProgress();
            return View(weeklyProgress);
        }

        private Dictionary<DateTime, List<(StudySession, string)>> CalculateWeeklyProgress()
        {
            var weeklyProgress = new Dictionary<DateTime, List<(StudySession, string)>>();
            var startDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            var endDate = startDate.AddDays(6);

            List<StudySession> allSessions = GetAllStudySessions();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var sessionsForDate = allSessions.Where(s => s.StartTime.Date == date).ToList();
                var activitiesForDate = sessionsForDate.Select(s =>
                {
                    var subjectName = GetAllSubjects().FirstOrDefault(sub => sub.Id == s.SubjectId)?.Name ?? "N/A";
                    return (s, subjectName);
                }).ToList();
                weeklyProgress[date] = activitiesForDate;
            }

            return weeklyProgress;
        }

        public ActionResult PredictedAcademicStatus()
        {
            double studyHours = CalculateTotalStudyHours();
            double beakHours = CalculateTotalBreakHours();
            var (predictedGrade, activeHours) = CalculatePredictedGrades(studyHours, beakHours);
            ViewBag.PredictedGrade = predictedGrade;
            ViewBag.ActiveHours = activeHours.ToString("F2");
            return View();
        }

        private List<Break> GetAllBreaks()
        {
            List<Break> breaks = new List<Break>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "SELECT Id, UserId, StartTime, EndTime FROM Breaks";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Break breakSession = new Break
                            {
                                Id = (int)reader["Id"],
                                UserId = (int)reader["UserId"],
                                StartTime = (DateTime)reader["StartTime"],
                                EndTime = (DateTime)reader["EndTime"]
                            };
                            breaks.Add(breakSession);
                        }
                    }
                }
            }

            return breaks;
        }

        private double CalculateTotalBreakHours()
        {
            List<Break> allBreak = GetAllBreaks();
            double totalHours = allBreak.Sum(s => (s.EndTime - s.StartTime).TotalHours);
            return totalHours;
        }

        private double CalculateTotalStudyHours()
        {
            List<StudySession> allSessions = GetAllStudySessions();
            double totalHours = allSessions.Sum(s => (s.EndTime - s.StartTime).TotalHours);
            return totalHours;
        }

        private (string, double) CalculatePredictedGrades(double studyHours, double breakHours)
        {
            double activeHours = studyHours - breakHours;

            string predictedGrade;
            if (activeHours >= 40)
            {
                predictedGrade = "Excellent";
            }
            else if (activeHours >= 30)
            {
                predictedGrade = "Good";
            }
            else if (activeHours >= 20)
            {
                predictedGrade = "Average";
            }
            else
            {
                predictedGrade = "Below Average";
            }

            return (predictedGrade, activeHours);
        }


        private List<User> GetAllUsers()
        {
            List<User> users = new List<User>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "SELECT Id, Name, Email, Password FROM Users";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            User user = new User
                            {
                                Id = (int)reader["Id"],
                                Name = reader["Name"].ToString(),
                                Email = reader["Email"].ToString(),
                                Password = reader["Password"].ToString()
                            };
                            users.Add(user);
                        }
                    }
                }
            }

            return users;
        }

        private List<Subject> GetAllSubjects()
        {
            List<Subject> subjects = new List<Subject>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "SELECT Id, Name, Instructor FROM Subjects";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Subject subject = new Subject
                            {
                                Id = (int)reader["Id"],
                                Name = reader["Name"].ToString(),
                                Instructor = reader["Instructor"].ToString()
                            };
                            subjects.Add(subject);
                        }
                    }
                }
            }

            return subjects;
        }

        private User GetUserById(int id)
        {
            User user = null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "SELECT Id, Name, Email, Password FROM Users WHERE Id = @Id";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                Id = (int)reader["Id"],
                                Name = reader["Name"].ToString(),
                                Email = reader["Email"].ToString(),
                                Password = reader["Password"].ToString()
                            };
                        }
                    }
                }
            }

            return user;
        }

        private StudySession GetStudySessionById(int id)
        {
            StudySession session = null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "SELECT Id, UserId, SubjectId, StartTime, EndTime FROM StudySessions WHERE Id = @Id";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            session = new StudySession
                            {
                                Id = (int)reader["Id"],
                                UserId = (int)reader["UserId"],
                                SubjectId = (int)reader["SubjectId"],
                                StartTime = (DateTime)reader["StartTime"],
                                EndTime = (DateTime)reader["EndTime"]
                            };
                            session.UserName = GetUserById(session.UserId)?.Name;
                        }
                    }
                }
            }

            return session;
        }

        private List<StudySession> GetAllStudySessions()
        {
            List<StudySession> studySessions = new List<StudySession>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "SELECT Id, UserId, SubjectId, StartTime, EndTime FROM StudySessions";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            StudySession session = new StudySession
                            {
                                Id = (int)reader["Id"],
                                UserId = (int)reader["UserId"],
                                SubjectId = (int)reader["SubjectId"],
                                StartTime = (DateTime)reader["StartTime"],
                                EndTime = (DateTime)reader["EndTime"]
                            };
                            session.UserName = GetUserById(session.UserId)?.Name;
                            studySessions.Add(session);
                        }
                    }
                }
            }

            return studySessions;
        }
    }
}

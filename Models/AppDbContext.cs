using System.Data.Entity;

namespace WebApplication1.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Break> Breaks { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<StudySession> StudySessions { get; set; }
        public DbSet<Subject> Subjects { get; set; }

        // Constructor to specify connection string (optional)
        public AppDbContext() : base("name=DefaultConnection")
        {
        }
    }
}

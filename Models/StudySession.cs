using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class StudySession
    {
        public int Id { get; set; }
        public int UserId { get; set; }  // Ensure this property exists
        public int SubjectId { get; set; }  // Ensure this property exists
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string UserName { get; set; }
    }
}
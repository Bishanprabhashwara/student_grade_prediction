using System;
using System.Collections.Generic;
using WebApplication1.Controllers;
using WebApplication1.Models;

namespace WebApplication1.ViewModels
{
    public class WeeklyProgressViewModel
    {
        public DateTime Date { get; set; }
        public List<(StudySession, string)> Activities { get; set; }
    }
}

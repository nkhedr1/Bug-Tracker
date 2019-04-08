using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModels
{
    public class CreateProjectViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<ApplicationUser> Users { get; set; }
    }
}
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
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public int AssignedUsers { get; set; }
        public int Tickets { get; set; }
        public bool Archived { get; set; }
        public virtual List<ApplicationUser> Users { get; set; }

    }
}
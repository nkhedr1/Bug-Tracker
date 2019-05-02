using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.Domain
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public int AssignedUsers { get; set; }
        public int TicketCount { get; set; }
        public virtual List<ApplicationUser> Users { get; set; }
        public virtual List<Ticket> Tickets { get; set; }
        public bool Archived { get; set; }

        public Project()
        {
            Users = new List<ApplicationUser>();
            Tickets = new List<Ticket>();
        }
    }
}
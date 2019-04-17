using BugTracker.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModels
{
    public class ViewAllTicketsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Comment { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public Project Project { get; set; }
        public int ProjectId { get; set; }
        public TicketPriority TicketPriority { get; set; }
        public int TicketPriorityId { get; set; }
        public TicketStatus TicketStatus { get; set; }
        public int TicketStatusId { get; set; }
        public TicketType TicketType { get; set; }
        public int TicketTypeId { get; set; }

        public ApplicationUser CreatedBy { get; set; }
        public string CreatedById { get; set; }

        public ApplicationUser AssignedTo { get; set; }
        public string AssignedToId { get; set; }

    }
}
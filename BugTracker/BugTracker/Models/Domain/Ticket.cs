using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.Domain
{
    public class Ticket
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public virtual Project Project { get; set; }
        public int ProjectId { get; set; }
        public virtual TicketPriority TicketPriority { get; set; }
        public int TicketPriorityId { get; set; }
        public virtual TicketStatus TicketStatus { get; set; }
        public int TicketStatusId { get; set; }
        public virtual TicketType TicketType { get; set; }
        public int TicketTypeId { get; set; }

        public ApplicationUser CreatedBy { get; set; }
        public string CreatedById { get; set; }

        public ApplicationUser AssignedTo { get; set; }
        public string AssignedToId { get; set; }
    }
}
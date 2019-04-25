using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.Domain
{
    public class TicketHistory
    {
        public int Id { get; set; }
        public string Property { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public DateTime DateChanged { get; set; }

        public virtual Ticket Ticket { get; set; }
        public int TicketId { get; set; }

        public virtual ApplicationUser User { get; set; }
        public string UserId { get; set; }

        public string ChangedBy { get; set; }

        public virtual List<TicketAttachment> TicketAttachments { get; set; }

        public TicketHistory()
        {
            TicketAttachments = new List<TicketAttachment>();
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BugTracker.Models.Domain
{
    public class TicketAttachment
    {
        public int Id { get; set; }
        public string AttachmentDiscription { get; set; }
        public string FilePath { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public virtual Ticket Ticket { get; set; }
        public int TicketId { get; set; }

        public virtual ApplicationUser User { get; set; }
        public string UserId { get; set; }
    }
}
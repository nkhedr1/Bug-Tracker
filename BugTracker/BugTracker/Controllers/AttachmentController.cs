using BugTracker.Models;
using BugTracker.Models.Domain;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    public class AttachmentController : Controller
    {
        private ApplicationDbContext DbContext;

        public AttachmentController()
        {
            DbContext = new ApplicationDbContext();
        }

        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult ListAllAttachmentsForTicketAdminProjectManager(int id)
        {

            var ticketAttachments =
                 (from attach in DbContext.TicketAttachments
                  where attach.TicketId == id && attach.Ticket.Project.Archived == false
                  select attach
                  ).ToList();

            return View(ticketAttachments);
        }

        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult DeleteAttachmentAdminProjectManager(TicketAttachment attachData)
        {

            var attachmentToRemove = DbContext.TicketAttachments.FirstOrDefault(
                attach => attach.Id == attachData.Id && attach.Ticket.Project.Archived == false);


            DbContext.TicketAttachments.Remove(attachmentToRemove);
            DbContext.SaveChanges();

            return RedirectToAction("ListAllAttachmentsForTicketAdminProjectManager", new { id = attachData.TicketId });
        }

        [Authorize(Roles = "Developer, Submitter")]
        public ActionResult ListAllAttachmentsForDeveloperSubmitter(int id)
        {
            var userId = User.Identity.GetUserId();

            var ticketAttachments =
                 (from attach in DbContext.TicketAttachments
                  where attach.TicketId == id && attach.UserId == userId && attach.Ticket.Project.Archived == false
                  select attach
                  ).ToList();

            return View(ticketAttachments);
        }

        [Authorize(Roles = "Developer, Submitter")]
        public ActionResult DeleteAttachmentDeveloperSubmitter(TicketAttachment attachData)
        {
            var userId = User.Identity.GetUserId();

            var attachmentToRemove = DbContext.TicketAttachments.FirstOrDefault(
                attach => attach.Id == attachData.Id && attach.UserId == userId && attach.Ticket.Project.Archived == false);


            DbContext.TicketAttachments.Remove(attachmentToRemove);
            DbContext.SaveChanges();

            return RedirectToAction("ListAllAttachmentsForDeveloperSubmitter", new { id = attachData.TicketId });
        }

    }
}
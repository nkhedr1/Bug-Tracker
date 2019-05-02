using BugTracker.Models;
using BugTracker.Models.Domain;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Net;
using System.Net.Mail;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    public class CommentController : Controller
    {
        private ApplicationDbContext DbContext;

        public CommentController()
        {
            DbContext = new ApplicationDbContext();
        }

        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult ListAllCommentsForTicketAdminProjectManager(int id)
        {

            var ticketComments =
                 (from comment in DbContext.Comments
                  where comment.TicketId == id && comment.Ticket.Project.Archived == false
                  select comment
                  ).ToList();

            return View(ticketComments);
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult EditCommentAdminProjectManager(int id)
        {
            var userId = User.Identity.GetUserId();

            var commentToEdit = DbContext.Comments.FirstOrDefault(
                comment => comment.Id == id && comment.Ticket.Project.Archived == false);

            return View(commentToEdit);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult EditCommentAdminProjectManager(Comment commentData)
        {
            var commentToEdit = DbContext.Comments.FirstOrDefault(
                comment => comment.Id == commentData.Id && comment.Ticket.Project.Archived == false);

            commentToEdit.TicketComment = commentData.TicketComment;
            commentToEdit.DateUpdated = DateTime.Today;

            // Sending mail notification to developer for any change in the ticket

            var ticketToEdit = DbContext.Tickets.FirstOrDefault(
                ticket => ticket.Id == commentData.TicketId);

            var ticketAssignedUserEmail =
                           (from p in DbContext.Users
                            where p.Id == ticketToEdit.AssignedToId
                            select p.Email).FirstOrDefault();

            if (ticketAssignedUserEmail != null)
            {
                SendEmailNotification(ticketAssignedUserEmail, "Ticket Modified", "The ticket you are assigned to has been modified");
            }

            DbContext.SaveChanges();

            return RedirectToAction("ListAllCommentsForTicketAdminProjectManager", new { id = commentData.TicketId});
        }

        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult DeleteCommentAdminProjectManager(Comment commentData)
        {
            var commentToRemove = DbContext.Comments.FirstOrDefault(
                comment => comment.Id == commentData.Id && comment.Ticket.Project.Archived == false);

          
                DbContext.Comments.Remove(commentToRemove);
                DbContext.SaveChanges();

            return RedirectToAction("ListAllCommentsForTicketAdminProjectManager", new { id = commentData.TicketId });
        }

        [Authorize(Roles = "Developer, Submitter")]
        public ActionResult ListAllCommentsForTicketDeveloperSubmitter(int id)
        {
            var userId = User.Identity.GetUserId();

            var ticketComments =
                 (from comment in DbContext.Comments
                  where comment.TicketId == id && comment.UserId == userId && comment.Ticket.Project.Archived == false
                  select comment
                  ).ToList();

            return View(ticketComments);
        }

        [HttpGet]
        [Authorize(Roles = "Developer, Submitter")]
        public ActionResult EditCommentDeveloperSubmitter(int id)
        {
            var userId = User.Identity.GetUserId();

            var commentToEdit = DbContext.Comments.FirstOrDefault(
                comment => comment.Id == id && comment.UserId == userId && comment.Ticket.Project.Archived == false);

            return View(commentToEdit);
        }

        [HttpPost]
        [Authorize(Roles = "Developer, Submitter")]
        public ActionResult EditCommentDeveloperSubmitter(Comment commentData)
        {
            var commentToEdit = DbContext.Comments.FirstOrDefault(
                comment => comment.Id == commentData.Id && comment.Ticket.Project.Archived == false);

            commentToEdit.TicketComment = commentData.TicketComment;
            commentToEdit.DateUpdated = DateTime.Today;

            // Sending mail notification to developer for any change in the ticket

            var ticketToEdit = DbContext.Tickets.FirstOrDefault(
                ticket => ticket.Id == commentData.TicketId && ticket.Project.Archived == false);

            var ticketAssignedUserEmail =
                           (from p in DbContext.Users
                            where p.Id == ticketToEdit.AssignedToId
                            select p.Email).FirstOrDefault();

            if (ticketAssignedUserEmail != null)
            {
                SendEmailNotification(ticketAssignedUserEmail, "Ticket Modified", "The ticket you are assigned to has been modified");
            }

            DbContext.SaveChanges();

            return RedirectToAction("ListAllCommentsForTicketDeveloperSubmitter", new { id = commentData.TicketId });
        }

        [Authorize(Roles = "Developer, Submitter")]
        public ActionResult DeleteCommentDeveloperSubmitter(Comment commentData)
        {
            var userId = User.Identity.GetUserId();

            var commentToRemove = DbContext.Comments.FirstOrDefault(
                comment => comment.Id == commentData.Id && comment.UserId == userId && comment.Ticket.Project.Archived == false);


            DbContext.Comments.Remove(commentToRemove);
            DbContext.SaveChanges();

            return RedirectToAction("ListAllCommentsForTicketDeveloperSubmitter", new { id = commentData.TicketId });
        }

        //method to send email notification
        protected void SendEmailNotification(string email, string subject, string body)
        {

            MailAddress from = new MailAddress("nour@gmail.com");
            MailAddress to = new MailAddress(email);
            MailMessage message = new MailMessage(from, to);

            message.Subject = subject;
            message.Body = body;
            message.BodyEncoding = System.Text.Encoding.UTF8;
            message.IsBodyHtml = true;

            SmtpClient client = new SmtpClient("smtp.mailtrap.io", 2525);
            client.UseDefaultCredentials = false;
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential("e4ffc6c1f9b78a", "0e3ad862fa6817");

            try
            {
                client.Send(message);
            }
            catch
            {
                //error message?
            }
            finally
            {

            }
        }
    }
}
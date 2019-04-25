using BugTracker.Models;
using BugTracker.Models.Domain;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
                  where comment.TicketId == id
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
                comment => comment.Id == id);

            return View(commentToEdit);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult EditCommentAdminProjectManager(Comment commentData)
        {
            var commentToEdit = DbContext.Comments.FirstOrDefault(
                comment => comment.Id == commentData.Id);

            commentToEdit.TicketComment = commentData.TicketComment;
            commentToEdit.DateUpdated = DateTime.Today;

            DbContext.SaveChanges();

            return RedirectToAction("ListAllCommentsForTicketAdminProjectManager", new { id = commentData.TicketId});
        }

        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult DeleteCommentAdminProjectManager(Comment commentData)
        {
            var commentToRemove = DbContext.Comments.FirstOrDefault(
                comment => comment.Id == commentData.Id);

          
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
                  where comment.TicketId == id && comment.UserId == userId
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
                comment => comment.Id == id && comment.UserId == userId);

            return View(commentToEdit);
        }

        [HttpPost]
        [Authorize(Roles = "Developer, Submitter")]
        public ActionResult EditCommentDeveloperSubmitter(Comment commentData)
        {
            var commentToEdit = DbContext.Comments.FirstOrDefault(
                comment => comment.Id == commentData.Id);

            commentToEdit.TicketComment = commentData.TicketComment;
            commentToEdit.DateUpdated = DateTime.Today;

            DbContext.SaveChanges();

            return RedirectToAction("ListAllCommentsForTicketDeveloperSubmitter", new { id = commentData.TicketId });
        }

        [Authorize(Roles = "Developer, Submitter")]
        public ActionResult DeleteCommentDeveloperSubmitter(Comment commentData)
        {
            var userId = User.Identity.GetUserId();

            var commentToRemove = DbContext.Comments.FirstOrDefault(
                comment => comment.Id == commentData.Id && comment.UserId == userId);


            DbContext.Comments.Remove(commentToRemove);
            DbContext.SaveChanges();

            return RedirectToAction("ListAllCommentsForTicketDeveloperSubmitter", new { id = commentData.TicketId });
        }
    }
}
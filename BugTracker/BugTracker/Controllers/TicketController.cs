﻿using BugTracker.Models;
using BugTracker.Models.Domain;
using BugTracker.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    public class TicketController : Controller
    {
        private ApplicationDbContext DbContext;

        public TicketController()
        {
            DbContext = new ApplicationDbContext();
        }

        [HttpGet]
        [Authorize(Roles = "Submitter")]
        public ActionResult CreateTicket()
        {

            var ticketTypes = (from type in DbContext.TicketTypes
                               select type
                             ).ToList();

            ViewBag.AllTicketTypes = ticketTypes;

            var ticketPriorities = (from priority in DbContext.TicketPriorities
                                    select priority
                             ).ToList();

            ViewBag.AllTicketPriorities = ticketPriorities;

            var ticketStatuses = (from status in DbContext.TicketStatuses
                                  select status
                            ).ToList();

            ViewBag.AllTicketStatuses = ticketStatuses;

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Submitter")]
        public ActionResult CreateTicket(int id, FormCollection formData)
        {
            var userId = User.Identity.GetUserId();
            var ticketTypeId = formData["TicketType"];
            var ticketPriorityId = formData["TicketPriority"];
     
            var projectToAddTicketTo = DbContext.Projects.FirstOrDefault(
               project => project.Id == id);

            var ticketDefaultStatus = DbContext.TicketStatuses.FirstOrDefault(
               status => status.Name == "Open");

            Ticket currentTicket = new Ticket();

            currentTicket.Title = formData["Title"];
            currentTicket.Description = formData["Description"];
            currentTicket.ProjectId = id;
            currentTicket.DateCreated = DateTime.Today;
            currentTicket.TicketPriorityId = Int32.Parse(ticketPriorityId);
            currentTicket.TicketTypeId = Int32.Parse(ticketTypeId);
            currentTicket.TicketStatusId = ticketDefaultStatus.Id;
            currentTicket.CreatedById = userId;

            projectToAddTicketTo.Tickets.Add(currentTicket);
            DbContext.SaveChanges();

            return RedirectToAction("Index", "Home");
        }

       

        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult ListAllTickets()
        {

            var allTickets =
                  (from ticket in DbContext.Tickets
                   select new ViewAllTicketsViewModel
                   {
                       Id = ticket.Id,
                       CreatedBy = ticket.CreatedBy,
                       AssignedTo = ticket.AssignedTo,
                       Title = ticket.Title,
                       Description = ticket.Description,
                       DateCreated = ticket.DateCreated,
                       DateUpdated = ticket.DateUpdated,
                       TicketStatus = ticket.TicketStatus,
                       TicketPriority = ticket.TicketPriority,
                       TicketType = ticket.TicketType,
                       Project = ticket.Project

                   }).ToList();



            return View(allTickets);
        }

        // List ticket created by the submitter 
        [Authorize(Roles = "Submitter")]
        public ActionResult ListMyTickets()
        {
            var userId = User.Identity.GetUserId();

            var myTickets =
                  (from ticket in DbContext.Tickets
                   where ticket.CreatedBy.Id == userId
                   select new ViewAllTicketsViewModel
                   {
                       Id = ticket.Id,
                       CreatedBy = ticket.CreatedBy,
                       AssignedTo = ticket.AssignedTo,
                       Title = ticket.Title,
                       Description = ticket.Description,
                       DateCreated = ticket.DateCreated,
                       DateUpdated = ticket.DateUpdated,
                       TicketStatus = ticket.TicketStatus,
                       TicketPriority = ticket.TicketPriority,
                       TicketType = ticket.TicketType,
                       Project = ticket.Project

                   }).ToList();

            return View(myTickets);
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult EditTicket(int id)
        {
            var userId = User.Identity.GetUserId();

            var ticketToEdit = DbContext.Tickets.FirstOrDefault(
                ticket => ticket.Id == id);

            var ticketModel = new ViewAllTicketsViewModel();
            ticketModel.Title = ticketToEdit.Title;
            ticketModel.Description = ticketToEdit.Description;
            ticketModel.DateCreated = ticketToEdit.DateCreated;
            ticketModel.DateUpdated = ticketToEdit.DateUpdated;
            ticketModel.Project = ticketToEdit.Project;
            ticketModel.TicketPriority = ticketToEdit.TicketPriority;
            ticketModel.TicketStatus = ticketToEdit.TicketStatus;
            ticketModel.TicketType = ticketToEdit.TicketType;

            var currentProjects = (from project in DbContext.Projects
                                   select project
                          ).ToList();

            ViewBag.AllProjects = currentProjects;

            var ticketTypes = (from type in DbContext.TicketTypes
                               select type
                            ).ToList();

            ViewBag.AllTicketTypes = ticketTypes;

            var ticketPriorities = (from priority in DbContext.TicketPriorities
                                    select priority
                             ).ToList();

            ViewBag.AllTicketPriorities = ticketPriorities;

            var ticketStatuses = (from status in DbContext.TicketStatuses
                                  select status
                            ).ToList();

            ViewBag.AllTicketStatuses = ticketStatuses;

            // Querying a list of Developers and displaying them on the view in order to Assign to Ticket

            var modelUser = (from user in DbContext.Users
                             select new ManageUsersViewModel
                             {
                                 Id = user.Id,
                                 FirstName = user.FirstName,
                                 Email = user.Email,
                                 CurrentRoles = (from userRoles in user.Roles
                                                 join role in DbContext.Roles on userRoles.RoleId
                                                 equals role.Id
                                                 select role.Name).ToList()
                             }).ToList();


            var currentDevelopers = (from user in modelUser
                                     where user.CurrentRoles.Contains("Developer")
                                     select user
                          ).ToList();

            ViewBag.CurrentDatabaseDevelopers = currentDevelopers;

            return View(ticketModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult EditTicket(ViewAllTicketsViewModel ticketData, FormCollection formData)
        {
            var ticketToEdit = DbContext.Tickets.FirstOrDefault(
                project => project.Id == ticketData.Id);

            var ticketTypeId = formData["TicketType"];
            var ticketPriorityId = formData["TicketPriority"];
            var ticketStatusId = formData["TicketStatus"];
            var ticketProjectId = formData["Projects"];
            var developerId = formData["AssignDeveloper"];

            ticketToEdit.DateUpdated = DateTime.Today;
            ticketToEdit.Title = ticketData.Title;
            ticketToEdit.Description = ticketData.Description;
            ticketToEdit.ProjectId = Int32.Parse(ticketProjectId);
            ticketToEdit.TicketPriorityId = Int32.Parse(ticketPriorityId);
            ticketToEdit.TicketStatusId = Int32.Parse(ticketStatusId);
            ticketToEdit.TicketTypeId = Int32.Parse(ticketTypeId);
            if (developerId == "Not Assigned")
            {
                ticketToEdit.AssignedToId = null;
            }
            else
            {
                ticketToEdit.AssignedToId = developerId;
            }

            DbContext.SaveChanges();
            return RedirectToAction(nameof(TicketController.ListAllTickets));
        }

        [Authorize(Roles = "Developer")]
        public ActionResult ListDeveloperTickets()
        {
            var userId = User.Identity.GetUserId();

            var myTickets =
                  (from ticket in DbContext.Tickets
                   where ticket.AssignedToId == userId
                   select new ViewAllTicketsViewModel
                   {
                       Id = ticket.Id,
                       CreatedBy = ticket.CreatedBy,
                       AssignedTo = ticket.AssignedTo,
                       Title = ticket.Title,
                       Description = ticket.Description,
                       DateCreated = ticket.DateCreated,
                       DateUpdated = ticket.DateUpdated,
                       TicketStatus = ticket.TicketStatus,
                       TicketPriority = ticket.TicketPriority,
                       TicketType = ticket.TicketType,
                       Project = ticket.Project

                   }).ToList();


            var myAssignedProjects =
                  (from project in DbContext.Projects
                   where project.Users.Any(usr => usr.Id == userId)
                   select project)
                   .ToList();

            var myAssignedProjectTickets = myAssignedProjects.SelectMany(project => project.Tickets).ToList();

            ViewBag.MyAssignedProjectTicketList = myAssignedProjectTickets;

            return View(myTickets);
        }

        [HttpGet]
        [Authorize(Roles = "Developer")]
        public ActionResult EditDeveloperTicket(int id)
        {
            var userId = User.Identity.GetUserId();

            var ticketToEdit = DbContext.Tickets.FirstOrDefault(
                ticket => ticket.Id == id);

            var ticketModel = new ViewAllTicketsViewModel();
            ticketModel.Title = ticketToEdit.Title;
            ticketModel.Description = ticketToEdit.Description;
            ticketModel.DateCreated = ticketToEdit.DateCreated;
            ticketModel.DateUpdated = ticketToEdit.DateUpdated;
            ticketModel.Project = ticketToEdit.Project;
            ticketModel.TicketPriority = ticketToEdit.TicketPriority;
            ticketModel.TicketType = ticketToEdit.TicketType;

            var currentProjects = (from project in DbContext.Projects
                                   select project
                          ).ToList();

            ViewBag.AllProjects = currentProjects;

            var ticketTypes = (from type in DbContext.TicketTypes
                               select type
                            ).ToList();

            ViewBag.AllTicketTypes = ticketTypes;

            var ticketPriorities = (from priority in DbContext.TicketPriorities
                                    select priority
                             ).ToList();

            ViewBag.AllTicketPriorities = ticketPriorities;

            return View(ticketModel);
        }

        [HttpPost]
        [Authorize(Roles = "Developer")]
        public ActionResult EditDeveloperTicket(ViewAllTicketsViewModel ticketData, FormCollection formData)
        {
            var ticketToEdit = DbContext.Tickets.FirstOrDefault(
                project => project.Id == ticketData.Id);

            var ticketTypeId = formData["TicketType"];
            var ticketPriorityId = formData["TicketPriority"];
            var ticketProjectId = formData["Projects"];

            ticketToEdit.DateUpdated = DateTime.Today;
            ticketToEdit.Title = ticketData.Title;
            ticketToEdit.Description = ticketData.Description;
            ticketToEdit.ProjectId = Int32.Parse(ticketProjectId);
            ticketToEdit.TicketPriorityId = Int32.Parse(ticketPriorityId);
            ticketToEdit.TicketTypeId = Int32.Parse(ticketTypeId);
 
            DbContext.SaveChanges();
            return RedirectToAction(nameof(TicketController.ListDeveloperTickets));
        }

        [HttpGet]
        [Authorize(Roles = "Submitter")]
        public ActionResult EditMySubmitterTicket(int id)
        {
            var userId = User.Identity.GetUserId();

            var ticketToEdit = DbContext.Tickets.FirstOrDefault(
                ticket => ticket.Id == id);

            var ticketModel = new ViewAllTicketsViewModel();
            ticketModel.Title = ticketToEdit.Title;
            ticketModel.Description = ticketToEdit.Description;
            ticketModel.DateCreated = ticketToEdit.DateCreated;
            ticketModel.DateUpdated = ticketToEdit.DateUpdated;
            ticketModel.Project = ticketToEdit.Project;
            ticketModel.TicketPriority = ticketToEdit.TicketPriority;
            ticketModel.TicketType = ticketToEdit.TicketType;

            var currentProjects = (from project in DbContext.Projects
                                   select project
                          ).ToList();

            ViewBag.AllProjects = currentProjects;

            var ticketTypes = (from type in DbContext.TicketTypes
                               select type
                            ).ToList();

            ViewBag.AllTicketTypes = ticketTypes;

            var ticketPriorities = (from priority in DbContext.TicketPriorities
                                    select priority
                             ).ToList();

            ViewBag.AllTicketPriorities = ticketPriorities;

            return View(ticketModel);
        }

        [HttpPost]
        [Authorize(Roles = "Submitter")]
        public ActionResult EditMySubmitterTicket(ViewAllTicketsViewModel ticketData, FormCollection formData)
        {
            var ticketToEdit = DbContext.Tickets.FirstOrDefault(
                project => project.Id == ticketData.Id);

            var ticketTypeId = formData["TicketType"];
            var ticketPriorityId = formData["TicketPriority"];
            var ticketProjectId = formData["Projects"];

            ticketToEdit.DateUpdated = DateTime.Today;
            ticketToEdit.Title = ticketData.Title;
            ticketToEdit.Description = ticketData.Description;
            ticketToEdit.ProjectId = Int32.Parse(ticketProjectId);
            ticketToEdit.TicketPriorityId = Int32.Parse(ticketPriorityId);
            ticketToEdit.TicketTypeId = Int32.Parse(ticketTypeId);

            DbContext.SaveChanges();
            return RedirectToAction(nameof(TicketController.ListMyTickets));
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult AddCommentAdminsProjectManagers()
        {
       
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult AddCommentAdminsProjectManagers(string TicketComment, int id)
        {
            var userId = User.Identity.GetUserId();

            var ticketToEdit = DbContext.Tickets.FirstOrDefault(
                project => project.Id == id);

            var comment = new Comment();

            comment.DateCreated = DateTime.Today;
            comment.TicketId = id;
            comment.TicketComment = TicketComment;
            comment.UserId = userId;

            ticketToEdit.Comments.Add(comment);
            DbContext.SaveChanges();
            return RedirectToAction(nameof(TicketController.ListAllTickets));
        }

        [HttpGet]
        [Authorize(Roles = "Developer")]
        public ActionResult AddCommentDeveloper()
        {        
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Developer")]
        public ActionResult AddCommentDeveloper(string TicketComment, int id)
        {
            var userId = User.Identity.GetUserId();

            var ticketToEdit = DbContext.Tickets.FirstOrDefault(
                project => project.Id == id);

            var comment = new Comment();

            comment.DateCreated = DateTime.Today;
            comment.TicketId = id;
            comment.TicketComment = TicketComment;
            comment.UserId = userId;

            ticketToEdit.Comments.Add(comment);
            DbContext.SaveChanges();

            return RedirectToAction(nameof(TicketController.ListDeveloperTickets));
        }

        [HttpGet]
        [Authorize(Roles = "Submitter")]
        public ActionResult AddCommentSubmitter()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Submitter")]
        public ActionResult AddCommentSubmitter(string TicketComment, int id)
        {
            var userId = User.Identity.GetUserId();

            var ticketToEdit = DbContext.Tickets.FirstOrDefault(
                project => project.Id == id);

            var comment = new Comment();

            comment.DateCreated = DateTime.Today;
            comment.TicketId = id;
            comment.TicketComment = TicketComment;
            comment.UserId = userId;

            ticketToEdit.Comments.Add(comment);
            DbContext.SaveChanges();

            return RedirectToAction(nameof(TicketController.ListMyTickets));
        }

        // method to create attatchment folder and the path for it
        private string UploadFile(HttpPostedFileBase file)
        {
            if (file != null)
            {
                var uploadFolder = "~/Upload/";
                var mappedFolder = Server.MapPath(uploadFolder);

                if (!Directory.Exists(mappedFolder))
                {
                    Directory.CreateDirectory(mappedFolder);
                }

                file.SaveAs(mappedFolder + file.FileName);

                return uploadFolder + file.FileName;
            }

            return null;
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult AddAttatchmentAdminProjectManager()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult AddAttatchmentAdminProjectManager(TicketAttachment attachData, HttpPostedFileBase UploadedFile, int id)
        {
            var ticketToEdit = DbContext.Tickets.FirstOrDefault(
                ticket => ticket.Id == id);

            var userId = User.Identity.GetUserId();

            var attachment = new TicketAttachment();

            if (UploadedFile != null && UploadedFile.ContentLength > 0)
            {
                var fileName = Path.GetFileName(UploadedFile.FileName);
                attachment.FilePath = UploadFile(UploadedFile);
                attachment.AttachmentDiscription = attachData.AttachmentDiscription;
                attachment.UserId = userId;
                attachment.DateCreated = DateTime.Today;
                attachment.TicketId = id;
            }

            ticketToEdit.TicketAttachments.Add(attachment);

            DbContext.SaveChanges();
            return RedirectToAction(nameof(TicketController.ListAllTickets));
        }

        [HttpGet]
        [Authorize(Roles = "Developer")]
        public ActionResult AddAttatchmentDeveloper()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Developer")]
        public ActionResult AddAttatchmentDeveloper(TicketAttachment attachData, HttpPostedFileBase UploadedFile, int id)
        {
            var ticketToEdit = DbContext.Tickets.FirstOrDefault(
                ticket => ticket.Id == id);

            var userId = User.Identity.GetUserId();

            var attachment = new TicketAttachment();

            if (UploadedFile != null && UploadedFile.ContentLength > 0)
            {
                var fileName = Path.GetFileName(UploadedFile.FileName);
                attachment.FilePath = UploadFile(UploadedFile);
                attachment.AttachmentDiscription = attachData.AttachmentDiscription;
                attachment.UserId = userId;
                attachment.DateCreated = DateTime.Today;
                attachment.TicketId = id;
            }

            ticketToEdit.TicketAttachments.Add(attachment);

            DbContext.SaveChanges();
            return RedirectToAction(nameof(TicketController.ListDeveloperTickets));
        }

        [HttpGet]
        [Authorize(Roles = "Submitter")]
        public ActionResult AddAttatchmentSubmitter()
        {
          
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Submitter")]
        public ActionResult AddAttatchmentSubmitter(TicketAttachment attachData, HttpPostedFileBase UploadedFile, int id)
        {
            var ticketToEdit = DbContext.Tickets.FirstOrDefault(
                 ticket => ticket.Id == id);

            var userId = User.Identity.GetUserId();

            var attachment = new TicketAttachment();

            if (UploadedFile != null && UploadedFile.ContentLength > 0)
            {
                var fileName = Path.GetFileName(UploadedFile.FileName);
                attachment.FilePath = UploadFile(UploadedFile);
                attachment.AttachmentDiscription = attachData.AttachmentDiscription;
                attachment.UserId = userId;
                attachment.DateCreated = DateTime.Today;
                attachment.TicketId = id;
            }

            ticketToEdit.TicketAttachments.Add(attachment);

            DbContext.SaveChanges();
            return RedirectToAction(nameof(TicketController.ListMyTickets));
        }

        [Authorize]
        public ActionResult ViewTicketDetails(int id)
        {
            var ticketToView = DbContext.Tickets.FirstOrDefault(
                project => project.Id == id);


            var ticketAttachments = (from attatch in DbContext.TicketAttachments
                                     where attatch.TicketId == id
                                     select attatch
                           ).ToList();

            ViewBag.TicketAttachments = ticketAttachments;

            return View(ticketToView);
        }
    }
}
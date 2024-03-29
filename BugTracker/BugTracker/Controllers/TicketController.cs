﻿using BugTracker.Models;
using BugTracker.Models.Domain;
using BugTracker.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
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
               project => project.Id == id && project.Archived == false);

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
                   where ticket.Project.Archived == false
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
                   && ticket.Project.Archived == false
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
                ticket => ticket.Id == id && ticket.Project.Archived == false);

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
                                   where project.Archived == false
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
                ticket => ticket.Id == ticketData.Id && ticket.Project.Archived == false);

            var userId = User.Identity.GetUserId();

            var currentUserName =
                           (from p in DbContext.Users
                            where p.Id == userId
                            select p.UserName).FirstOrDefault();

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

            // Tracking the History of changes in a ticket

            var modifiedEntities = DbContext.ChangeTracker.Entries()
                .Where(p => p.State == EntityState.Modified).ToList();

            var dateChanged = DateTime.UtcNow;

            foreach (var change in modifiedEntities)
            {

                foreach (var prop in change.OriginalValues.PropertyNames)
                {
                    // Checking to see if the property is not AssignedTo as there is a string in my select option for the assigned users that sets the user to "Not Assigned" incase the ticket is un assigned
                    if (prop != "AssignedToId" && prop != "DateUpdated")
                    {
                        var originalValue = change.OriginalValues[prop].ToString();
                        var currentValue = change.CurrentValues[prop].ToString();

                        if (originalValue != currentValue)
                        {
                            if (prop == "ProjectId")
                            {
                                var originalValueInt = Int32.Parse(originalValue);
                                var currentValueInt = Int32.Parse(currentValue);
                                var oldValueQuery =
                                    (from p in DbContext.Projects
                                     where p.Id == originalValueInt
                                     select p.Name).FirstOrDefault();

                                var currentValueQuery =
                                 (from p in DbContext.Projects
                                  where p.Id == currentValueInt
                                  select p.Name).FirstOrDefault();

                                TicketHistory log = new TicketHistory()
                                {
                                    Property = "Project",
                                    OldValue = oldValueQuery,
                                    NewValue = currentValueQuery,
                                    DateChanged = dateChanged,
                                    UserId = userId,
                                    ChangedBy = currentUserName
                                };

                                ticketToEdit.TicketHistories.Add(log);
                            }
                            else if (prop == "TicketPriorityId")
                            {
                                var originalValueInt = Int32.Parse(originalValue);
                                var currentValueInt = Int32.Parse(currentValue);
                                var oldValueQuery =
                                    (from p in DbContext.TicketPriorities
                                     where p.Id == originalValueInt
                                     select p.Name).FirstOrDefault();

                                var currentValueQuery =
                                 (from p in DbContext.TicketPriorities
                                  where p.Id == currentValueInt
                                  select p.Name).FirstOrDefault();

                                TicketHistory log = new TicketHistory()
                                {
                                    Property = "Priority",
                                    OldValue = oldValueQuery,
                                    NewValue = currentValueQuery,
                                    DateChanged = dateChanged,
                                    UserId = userId,
                                    ChangedBy = currentUserName
                                };

                                ticketToEdit.TicketHistories.Add(log);
                            }

                            else if (prop == "TicketStatusId")
                            {
                                var originalValueInt = Int32.Parse(originalValue);
                                var currentValueInt = Int32.Parse(currentValue);
                                var oldValueQuery =
                                    (from p in DbContext.TicketStatuses
                                     where p.Id == originalValueInt
                                     select p.Name).FirstOrDefault();

                                var currentValueQuery =
                                 (from p in DbContext.TicketStatuses
                                  where p.Id == currentValueInt
                                  select p.Name).FirstOrDefault();

                                TicketHistory log = new TicketHistory()
                                {
                                    Property = "Ticket Status",
                                    OldValue = oldValueQuery,
                                    NewValue = currentValueQuery,
                                    DateChanged = dateChanged,
                                    UserId = userId,
                                    ChangedBy = currentUserName
                                };

                                ticketToEdit.TicketHistories.Add(log);
                            }

                            else if (prop == "TicketTypeId")
                            {
                                var originalValueInt = Int32.Parse(originalValue);
                                var currentValueInt = Int32.Parse(currentValue);
                                var oldValueQuery =
                                    (from p in DbContext.TicketTypes
                                     where p.Id == originalValueInt
                                     select p.Name).FirstOrDefault();

                                var currentValueQuery =
                                 (from p in DbContext.TicketTypes
                                  where p.Id == currentValueInt
                                  select p.Name).FirstOrDefault();

                                TicketHistory log = new TicketHistory()
                                {
                                    Property = "Ticket Type",
                                    OldValue = oldValueQuery,
                                    NewValue = currentValueQuery,
                                    DateChanged = dateChanged,
                                    UserId = userId,
                                    ChangedBy = currentUserName
                                };

                                ticketToEdit.TicketHistories.Add(log);
                            }

                            else if (prop == "TicketTypeId")
                            {
                                var originalValueInt = Int32.Parse(originalValue);
                                var currentValueInt = Int32.Parse(currentValue);
                                var oldValueQuery =
                                    (from p in DbContext.TicketTypes
                                     where p.Id == originalValueInt
                                     select p.Name).FirstOrDefault();

                                var currentValueQuery =
                                 (from p in DbContext.TicketTypes
                                  where p.Id == currentValueInt
                                  select p.Name).FirstOrDefault();

                                TicketHistory log = new TicketHistory()
                                {
                                    Property = "Ticket Type",
                                    OldValue = oldValueQuery,
                                    NewValue = currentValueQuery,
                                    DateChanged = dateChanged,
                                    UserId = userId,
                                    ChangedBy = currentUserName
                                };

                                ticketToEdit.TicketHistories.Add(log);
                            }

                            else
                            {
                                TicketHistory log = new TicketHistory()
                                {
                                    Property = prop,
                                    OldValue = originalValue,
                                    NewValue = currentValue,
                                    DateChanged = dateChanged,
                                    UserId = userId
                                };

                                ticketToEdit.TicketHistories.Add(log);
                            }
                        }
                    }

                    else if (prop == "AssignedToId" && change.OriginalValues[prop] != null)
                    {
                        var originalValue = change.OriginalValues[prop].ToString();
                        var currentValue = "";
                        var currentValueQuery = "";
                        var oldValueQuery =
                            (from p in DbContext.Users
                             where p.Id == originalValue
                             select p.UserName).FirstOrDefault();

                        if (currentValue.GetType() != typeof(string))
                        {
                            currentValueQuery =
                               (from p in DbContext.Users
                                where p.Id == currentValue.ToString()
                                select p.UserName).FirstOrDefault();
                        }
                        else if (currentValue.GetType() == typeof(string))
                        {
                            {
                                currentValueQuery =
                                   (from p in DbContext.Users
                                    where p.Id == currentValue
                                    select p.UserName).FirstOrDefault();
                            }
                        }


                        TicketHistory log = new TicketHistory()

                        {
                            Property = "Assigned To",
                            OldValue = oldValueQuery,
                            NewValue = currentValueQuery,
                            DateChanged = dateChanged,
                            UserId = userId,
                            ChangedBy = currentUserName
                        };

                        ticketToEdit.TicketHistories.Add(log);


                        // quering for the assigned to user email to send notification of assigning a ticket
                        var originalAssignedToEmail =
                                (from p in DbContext.Users
                                 where p.Id == originalValue
                                 select p.Email).FirstOrDefault();

                        if (change.CurrentValues[prop] != null)
                        {
                            var currentAssignedToId = change.CurrentValues[prop].ToString();

                            var currentAssignedToEmail =
                            (from p in DbContext.Users
                             where p.Id == currentAssignedToId
                             select p.Email).FirstOrDefault();

                            if (originalAssignedToEmail != currentAssignedToEmail)

                                SendEmailNotification(currentAssignedToEmail, "Ticket Assigned", "You have been assigned a new ticket");
                        }

                    }

                }


            }

            // Sending mail notification to developer for any change in the ticket

            var ticketAssignedUserEmail =
                           (from p in DbContext.Users
                            where p.Id == ticketToEdit.AssignedToId
                            select p.Email).FirstOrDefault();

            if (modifiedEntities.Any() && ticketAssignedUserEmail != null)
            {
                SendEmailNotification(ticketAssignedUserEmail, "Ticket Modified", "The ticket you are assigned to has been modified");
            }

            // Sending mail notification to Admins and Project Managers who are opted in receiving notifications for for any change in the ticket

            foreach (var user in ticketToEdit.EmailNotifications)
            {
                if (modifiedEntities.Any())
                {
                    SendEmailNotification(user.Email, "Ticket Modified", "This ticket has been modified");
                }
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
                   && ticket.Project.Archived == false
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
                   where project.Archived == false
                   && project.Users.Any(usr => usr.Id == userId)
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
                ticket => ticket.Id == id && ticket.Project.Archived == false);

            var ticketModel = new ViewAllTicketsViewModel();
            ticketModel.Title = ticketToEdit.Title;
            ticketModel.Description = ticketToEdit.Description;
            ticketModel.DateCreated = ticketToEdit.DateCreated;
            ticketModel.DateUpdated = ticketToEdit.DateUpdated;
            ticketModel.Project = ticketToEdit.Project;
            ticketModel.TicketPriority = ticketToEdit.TicketPriority;
            ticketModel.TicketType = ticketToEdit.TicketType;

            var currentProjects = (from project in DbContext.Projects
                                   where project.Archived == false
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
                ticket => ticket.Id == ticketData.Id && ticket.Project.Archived == false);

            var ticketTypeId = formData["TicketType"];
            var ticketPriorityId = formData["TicketPriority"];
            var ticketProjectId = formData["Projects"];

            ticketToEdit.DateUpdated = DateTime.Today;
            ticketToEdit.Title = ticketData.Title;
            ticketToEdit.Description = ticketData.Description;
            ticketToEdit.ProjectId = Int32.Parse(ticketProjectId);
            ticketToEdit.TicketPriorityId = Int32.Parse(ticketPriorityId);
            ticketToEdit.TicketTypeId = Int32.Parse(ticketTypeId);



            // Tracking the History of changes in a ticket

            var userId = User.Identity.GetUserId();

            var currentUserName =
                           (from p in DbContext.Users
                            where p.Id == userId
                            select p.UserName).FirstOrDefault();

            var modifiedEntities = DbContext.ChangeTracker.Entries()
                .Where(p => p.State == EntityState.Modified).ToList();

            var dateChanged = DateTime.UtcNow;

            foreach (var change in modifiedEntities)
            {

                foreach (var prop in change.OriginalValues.PropertyNames)
                {

                    var originalValue = change.OriginalValues[prop].ToString();
                    var currentValue = change.CurrentValues[prop].ToString();

                    if (originalValue != currentValue)
                    {
                        if (prop == "ProjectId")
                        {
                            var originalValueInt = Int32.Parse(originalValue);
                            var currentValueInt = Int32.Parse(currentValue);
                            var oldValueQuery =
                                (from p in DbContext.Projects
                                 where p.Id == originalValueInt
                                 select p.Name).FirstOrDefault();

                            var currentValueQuery =
                             (from p in DbContext.Projects
                              where p.Id == currentValueInt
                              select p.Name).FirstOrDefault();

                            TicketHistory log = new TicketHistory()
                            {
                                Property = "Project",
                                OldValue = oldValueQuery,
                                NewValue = currentValueQuery,
                                DateChanged = dateChanged,
                                UserId = userId,
                                ChangedBy = currentUserName
                            };

                            ticketToEdit.TicketHistories.Add(log);
                        }
                        else if (prop == "TicketPriorityId")
                        {
                            var originalValueInt = Int32.Parse(originalValue);
                            var currentValueInt = Int32.Parse(currentValue);
                            var oldValueQuery =
                                (from p in DbContext.TicketPriorities
                                 where p.Id == originalValueInt
                                 select p.Name).FirstOrDefault();

                            var currentValueQuery =
                             (from p in DbContext.TicketPriorities
                              where p.Id == currentValueInt
                              select p.Name).FirstOrDefault();

                            TicketHistory log = new TicketHistory()
                            {
                                Property = "Priority",
                                OldValue = oldValueQuery,
                                NewValue = currentValueQuery,
                                DateChanged = dateChanged,
                                UserId = userId,
                                ChangedBy = currentUserName
                            };

                            ticketToEdit.TicketHistories.Add(log);
                        }


                        else if (prop == "TicketTypeId")
                        {
                            var originalValueInt = Int32.Parse(originalValue);
                            var currentValueInt = Int32.Parse(currentValue);
                            var oldValueQuery =
                                (from p in DbContext.TicketTypes
                                 where p.Id == originalValueInt
                                 select p.Name).FirstOrDefault();

                            var currentValueQuery =
                             (from p in DbContext.TicketTypes
                              where p.Id == currentValueInt
                              select p.Name).FirstOrDefault();

                            TicketHistory log = new TicketHistory()
                            {
                                Property = "Ticket Type",
                                OldValue = oldValueQuery,
                                NewValue = currentValueQuery,
                                DateChanged = dateChanged,
                                UserId = userId,
                                ChangedBy = currentUserName
                            };

                            ticketToEdit.TicketHistories.Add(log);
                        }

                        else if (prop == "TicketTypeId")
                        {
                            var originalValueInt = Int32.Parse(originalValue);
                            var currentValueInt = Int32.Parse(currentValue);
                            var oldValueQuery =
                                (from p in DbContext.TicketTypes
                                 where p.Id == originalValueInt
                                 select p.Name).FirstOrDefault();

                            var currentValueQuery =
                             (from p in DbContext.TicketTypes
                              where p.Id == currentValueInt
                              select p.Name).FirstOrDefault();

                            TicketHistory log = new TicketHistory()
                            {
                                Property = "Ticket Type",
                                OldValue = oldValueQuery,
                                NewValue = currentValueQuery,
                                DateChanged = dateChanged,
                                UserId = userId,
                                ChangedBy = currentUserName
                            };

                            ticketToEdit.TicketHistories.Add(log);
                        }

                        else
                        {
                            TicketHistory log = new TicketHistory()
                            {
                                Property = prop,
                                OldValue = originalValue,
                                NewValue = currentValue,
                                DateChanged = dateChanged,
                                UserId = userId
                            };

                            ticketToEdit.TicketHistories.Add(log);
                        }
                    }

                }
            }

            // Sending mail notification to developer for any change in the ticket

            var ticketAssignedUserEmail =
                           (from p in DbContext.Users
                            where p.Id == ticketToEdit.AssignedToId
                            select p.Email).FirstOrDefault();

            if (modifiedEntities.Any() && ticketAssignedUserEmail != null)
            {
                SendEmailNotification(ticketAssignedUserEmail, "Ticket Modified", "The ticket you are assigned to has been modified");
            }

            // Sending mail notification to Admins and Project Managers who are opted in receiving notifications for for any change in the ticket

            foreach (var user in ticketToEdit.EmailNotifications)
            {
                if (modifiedEntities.Any())
                {
                    SendEmailNotification(user.Email, "Ticket Modified", "This ticket has been modified");
                }
            }

            DbContext.SaveChanges();
            return RedirectToAction(nameof(TicketController.ListDeveloperTickets));
        }

        [HttpGet]
        [Authorize(Roles = "Submitter")]
        public ActionResult EditMySubmitterTicket(int id)
        {
            var userId = User.Identity.GetUserId();

            var ticketToEdit = DbContext.Tickets.FirstOrDefault(
                ticket => ticket.Id == id && ticket.Project.Archived == false);

            var ticketModel = new ViewAllTicketsViewModel();
            ticketModel.Title = ticketToEdit.Title;
            ticketModel.Description = ticketToEdit.Description;
            ticketModel.DateCreated = ticketToEdit.DateCreated;
            ticketModel.DateUpdated = ticketToEdit.DateUpdated;
            ticketModel.Project = ticketToEdit.Project;
            ticketModel.TicketPriority = ticketToEdit.TicketPriority;
            ticketModel.TicketType = ticketToEdit.TicketType;

            var currentProjects = (from project in DbContext.Projects
                                   where project.Archived == false
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
                ticket => ticket.Id == ticketData.Id && ticket.Project.Archived == false);

            var ticketTypeId = formData["TicketType"];
            var ticketPriorityId = formData["TicketPriority"];
            var ticketProjectId = formData["Projects"];

            ticketToEdit.DateUpdated = DateTime.Today;
            ticketToEdit.Title = ticketData.Title;
            ticketToEdit.Description = ticketData.Description;
            ticketToEdit.ProjectId = Int32.Parse(ticketProjectId);
            ticketToEdit.TicketPriorityId = Int32.Parse(ticketPriorityId);
            ticketToEdit.TicketTypeId = Int32.Parse(ticketTypeId);

            // Tracking the History of changes in a ticket

            var userId = User.Identity.GetUserId();

            var currentUserName =
                           (from p in DbContext.Users
                            where p.Id == userId
                            select p.UserName).FirstOrDefault();

            var modifiedEntities = DbContext.ChangeTracker.Entries()
                .Where(p => p.State == EntityState.Modified).ToList();

            var dateChanged = DateTime.UtcNow;

            foreach (var change in modifiedEntities)
            {

                foreach (var prop in change.OriginalValues.PropertyNames)
                {

                    var originalValue = change.OriginalValues[prop].ToString();
                    var currentValue = change.CurrentValues[prop].ToString();

                    if (originalValue != currentValue)
                    {
                        if (prop == "ProjectId")
                        {
                            var originalValueInt = Int32.Parse(originalValue);
                            var currentValueInt = Int32.Parse(currentValue);
                            var oldValueQuery =
                                (from p in DbContext.Projects
                                 where p.Id == originalValueInt
                                 select p.Name).FirstOrDefault();

                            var currentValueQuery =
                             (from p in DbContext.Projects
                              where p.Id == currentValueInt
                              select p.Name).FirstOrDefault();

                            TicketHistory log = new TicketHistory()
                            {
                                Property = "Project",
                                OldValue = oldValueQuery,
                                NewValue = currentValueQuery,
                                DateChanged = dateChanged,
                                UserId = userId,
                                ChangedBy = currentUserName
                            };

                            ticketToEdit.TicketHistories.Add(log);
                        }
                        else if (prop == "TicketPriorityId")
                        {
                            var originalValueInt = Int32.Parse(originalValue);
                            var currentValueInt = Int32.Parse(currentValue);
                            var oldValueQuery =
                                (from p in DbContext.TicketPriorities
                                 where p.Id == originalValueInt
                                 select p.Name).FirstOrDefault();

                            var currentValueQuery =
                             (from p in DbContext.TicketPriorities
                              where p.Id == currentValueInt
                              select p.Name).FirstOrDefault();

                            TicketHistory log = new TicketHistory()
                            {
                                Property = "Priority",
                                OldValue = oldValueQuery,
                                NewValue = currentValueQuery,
                                DateChanged = dateChanged,
                                UserId = userId,
                                ChangedBy = currentUserName
                            };

                            ticketToEdit.TicketHistories.Add(log);
                        }


                        else if (prop == "TicketTypeId")
                        {
                            var originalValueInt = Int32.Parse(originalValue);
                            var currentValueInt = Int32.Parse(currentValue);
                            var oldValueQuery =
                                (from p in DbContext.TicketTypes
                                 where p.Id == originalValueInt
                                 select p.Name).FirstOrDefault();

                            var currentValueQuery =
                             (from p in DbContext.TicketTypes
                              where p.Id == currentValueInt
                              select p.Name).FirstOrDefault();

                            TicketHistory log = new TicketHistory()
                            {
                                Property = "Ticket Type",
                                OldValue = oldValueQuery,
                                NewValue = currentValueQuery,
                                DateChanged = dateChanged,
                                UserId = userId,
                                ChangedBy = currentUserName
                            };

                            ticketToEdit.TicketHistories.Add(log);
                        }

                        else if (prop == "TicketTypeId")
                        {
                            var originalValueInt = Int32.Parse(originalValue);
                            var currentValueInt = Int32.Parse(currentValue);
                            var oldValueQuery =
                                (from p in DbContext.TicketTypes
                                 where p.Id == originalValueInt
                                 select p.Name).FirstOrDefault();

                            var currentValueQuery =
                             (from p in DbContext.TicketTypes
                              where p.Id == currentValueInt
                              select p.Name).FirstOrDefault();

                            TicketHistory log = new TicketHistory()
                            {
                                Property = "Ticket Type",
                                OldValue = oldValueQuery,
                                NewValue = currentValueQuery,
                                DateChanged = dateChanged,
                                UserId = userId,
                                ChangedBy = currentUserName
                            };

                            ticketToEdit.TicketHistories.Add(log);
                        }

                        else
                        {
                            TicketHistory log = new TicketHistory()
                            {
                                Property = prop,
                                OldValue = originalValue,
                                NewValue = currentValue,
                                DateChanged = dateChanged,
                                UserId = userId
                            };

                            ticketToEdit.TicketHistories.Add(log);
                        }
                    }
                }
            }

            // Sending mail notification to developer for any change in the ticket

            var ticketAssignedUserEmail =
                           (from p in DbContext.Users
                            where p.Id == ticketToEdit.AssignedToId
                            select p.Email).FirstOrDefault();

            if (modifiedEntities.Any() && ticketAssignedUserEmail != null)
            {
                SendEmailNotification(ticketAssignedUserEmail, "Ticket Modified", "The ticket you are assigned to has been modified");
            }

            // Sending mail notification to Admins and Project Managers who are opted in receiving notifications for for any change in the ticket

            foreach (var user in ticketToEdit.EmailNotifications)
            {
                if (modifiedEntities.Any())
                {
                    SendEmailNotification(user.Email, "Ticket Modified", "This ticket has been modified");
                }
            }

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
                ticket => ticket.Id == id && ticket.Project.Archived == false);

            var comment = new Comment();

            comment.DateCreated = DateTime.Today;
            comment.TicketId = id;
            comment.TicketComment = TicketComment;
            comment.UserId = userId;

            ticketToEdit.Comments.Add(comment);

            // Sending mail notification to developer for any change in the ticket

            var ticketAssignedUserEmail =
                           (from p in DbContext.Users
                            where p.Id == ticketToEdit.AssignedToId
                            select p.Email).FirstOrDefault();

            if (ticketAssignedUserEmail != null)
            {
                SendEmailNotification(ticketAssignedUserEmail, "Ticket Modified", "The ticket you are assigned to has been modified");
            }

            // Sending mail notification to Admins and Project Managers who are opted in receiving notifications for for any change in the ticket

            foreach (var user in ticketToEdit.EmailNotifications)
            {
                    SendEmailNotification(user.Email, "Ticket Modified", "This ticket has been modified");
            }

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
                ticket => ticket.Id == id && ticket.Project.Archived == false);

            var comment = new Comment();

            comment.DateCreated = DateTime.Today;
            comment.TicketId = id;
            comment.TicketComment = TicketComment;
            comment.UserId = userId;

            ticketToEdit.Comments.Add(comment);

            // Sending mail notification to developer for any change in the ticket

            var ticketAssignedUserEmail =
                           (from p in DbContext.Users
                            where p.Id == ticketToEdit.AssignedToId
                            select p.Email).FirstOrDefault();

            if (ticketAssignedUserEmail != null)
            {
                SendEmailNotification(ticketAssignedUserEmail, "Ticket Modified", "The ticket you are assigned to has been modified");
            }

            // Sending mail notification to Admins and Project Managers who are opted in receiving notifications for for any change in the ticket

            foreach (var user in ticketToEdit.EmailNotifications)
            {
                SendEmailNotification(user.Email, "Ticket Modified", "This ticket has been modified");
            }
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
                ticket => ticket.Id == id && ticket.Project.Archived == false);

            var comment = new Comment();

            comment.DateCreated = DateTime.Today;
            comment.TicketId = id;
            comment.TicketComment = TicketComment;
            comment.UserId = userId;

            ticketToEdit.Comments.Add(comment);

            // Sending mail notification to developer for any change in the ticket

            var ticketAssignedUserEmail =
                           (from p in DbContext.Users
                            where p.Id == ticketToEdit.AssignedToId
                            select p.Email).FirstOrDefault();

            if (ticketAssignedUserEmail != null)
            {
                SendEmailNotification(ticketAssignedUserEmail, "Ticket Modified", "The ticket you are assigned to has been modified");
            }

            // Sending mail notification to Admins and Project Managers who are opted in receiving notifications for for any change in the ticket

            foreach (var user in ticketToEdit.EmailNotifications)
            {
                SendEmailNotification(user.Email, "Ticket Modified", "This ticket has been modified");
            }

            DbContext.SaveChanges();

            return RedirectToAction(nameof(TicketController.ListMyTickets));
        }

        // method to create attatchment folder and the path for it
        private string UploadFile(HttpPostedFileBase file)
        {
            if (file != null)
            {
                var uploadFolder = "/Upload/";
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
                ticket => ticket.Id == id && ticket.Project.Archived == false);

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

            // Sending mail notification to developer for any change in the ticket

            var ticketAssignedUserEmail =
                           (from p in DbContext.Users
                            where p.Id == ticketToEdit.AssignedToId
                            select p.Email).FirstOrDefault();

            if (ticketAssignedUserEmail != null)
            {
                SendEmailNotification(ticketAssignedUserEmail, "Ticket Modified", "The ticket you are assigned to has been modified");
            }

            // Sending mail notification to Admins and Project Managers who are opted in receiving notifications for for any change in the ticket

            foreach (var user in ticketToEdit.EmailNotifications)
            {
                SendEmailNotification(user.Email, "Ticket Modified", "This ticket has been modified");
            }

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
                ticket => ticket.Id == id && ticket.Project.Archived == false);

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

            // Sending mail notification to developer for any change in the ticket

            var ticketAssignedUserEmail =
                           (from p in DbContext.Users
                            where p.Id == ticketToEdit.AssignedToId
                            select p.Email).FirstOrDefault();

            if (ticketAssignedUserEmail != null)
            {
                SendEmailNotification(ticketAssignedUserEmail, "Ticket Modified", "The ticket you are assigned to has been modified");
            }

            // Sending mail notification to Admins and Project Managers who are opted in receiving notifications for for any change in the ticket

            foreach (var user in ticketToEdit.EmailNotifications)
            {
                SendEmailNotification(user.Email, "Ticket Modified", "This ticket has been modified");
            }

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
                 ticket => ticket.Id == id && ticket.Project.Archived == false);

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

            // Sending mail notification to developer for any change in the ticket

            var ticketAssignedUserEmail =
                           (from p in DbContext.Users
                            where p.Id == ticketToEdit.AssignedToId
                            select p.Email).FirstOrDefault();

            if (ticketAssignedUserEmail != null)
            {
                SendEmailNotification(ticketAssignedUserEmail, "Ticket Modified", "The ticket you are assigned to has been modified");
            }

            // Sending mail notification to Admins and Project Managers who are opted in receiving notifications for for any change in the ticket

            foreach (var user in ticketToEdit.EmailNotifications)
            {
                SendEmailNotification(user.Email, "Ticket Modified", "This ticket has been modified");
            }

            DbContext.SaveChanges();
            return RedirectToAction(nameof(TicketController.ListMyTickets));
        }

        [Authorize]
        [HttpGet]
        public ActionResult ViewTicketDetails(int id)
        {
            var ticketToView = DbContext.Tickets.FirstOrDefault(
                ticket => ticket.Id == id && ticket.Project.Archived == false);


            var ticketAttachments = (from attatch in DbContext.TicketAttachments
                                     where attatch.TicketId == id && attatch.Ticket.Project.Archived == false
                                     select attatch
                           ).ToList();

            ViewBag.TicketAttachments = ticketAttachments;

            var ticketHistory = (from history in DbContext.TicketHistories
                                 where history.TicketId == id && history.Ticket.Project.Archived == false
                                 select history
                          ).ToList();

            ViewBag.TicketHistories = ticketHistory;

            return View(ticketToView);
        }

        [Authorize]
        [HttpPost]
        public ActionResult ViewTicketDetails(int id, FormCollection formData)
        {
            var ticketToView = DbContext.Tickets.FirstOrDefault(
               ticket => ticket.Id == id && ticket.Project.Archived == false);

            var userId = User.Identity.GetUserId();

            var currentUser = DbContext.Users.FirstOrDefault(
               user => user.Id == userId);

            var emailOptIn = formData["emailNotification"];

            //Checking if user is admin or project manager and adds or removes them from a list of opted in users for the email notification service for the ticket
            if (User.IsInRole("Admin") || User.IsInRole("Project Manager"))
                    {
                        if (emailOptIn != null)
                        {
                            if (!ticketToView.EmailNotifications.Contains(currentUser))
                            {
                                ticketToView.EmailNotifications.Add(currentUser);
                            }
                        }
                        else
                        {
                            if (ticketToView.EmailNotifications.Contains(currentUser))
                            {
                                ticketToView.EmailNotifications.Remove(currentUser);
                            }
                        }
                    }

            DbContext.SaveChanges();
            return RedirectToAction(nameof(TicketController.ListAllTickets));
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
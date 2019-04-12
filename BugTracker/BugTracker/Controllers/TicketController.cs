using BugTracker.Models;
using BugTracker.Models.Domain;
using BugTracker.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
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
    }
}
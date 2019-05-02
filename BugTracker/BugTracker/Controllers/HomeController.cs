using BugTracker.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext DbContext;


        public HomeController()
        {
            DbContext = new ApplicationDbContext();
        }

        [Authorize]
        public ActionResult Index()
        {
            //Quering data to display on the Dashboard
            var userId = User.Identity.GetUserId();

            var currentUser = DbContext.Users.FirstOrDefault(
                   user => user.Id == userId);

            if (User.IsInRole("Admin") || User.IsInRole("Project Manager"))
            {
                var allProjects = (from project in DbContext.Projects
                                   where project.Archived == false
                                   select project
                             ).ToList();

                ViewBag.AllProjects = allProjects.Count;

                var openTickets = (from ticket in DbContext.Tickets
                                   where ticket.TicketStatus.Name == "Open" && ticket.Project.Archived == false
                                   select ticket
                                ).ToList();

                var resolvedTickets = (from ticket in DbContext.Tickets
                                       where ticket.TicketStatus.Name == "Resolved" && ticket.Project.Archived == false
                                       select ticket
                               ).ToList();

                var rejectedTickets = (from ticket in DbContext.Tickets
                                       where ticket.TicketStatus.Name == "Rejected" && ticket.Project.Archived == false
                                       select ticket
                              ).ToList();

                ViewBag.AllProjects = allProjects.Count;
                ViewBag.AllOpenTickets = openTickets.Count;
                ViewBag.AllResolvedTickets = resolvedTickets.Count;
                ViewBag.AllRejectedTickets = rejectedTickets.Count;
            }


            if (User.IsInRole("Developer"))
            {
                var developerAssignedProjects = (from project in currentUser.Projects
                                                 where project.Archived == false
                                                 select project
                                                ).ToList();

                var developerAssignedTickets = (from ticket in DbContext.Tickets 
                                                where ticket.AssignedToId == userId && ticket.Project.Archived == false
                                                select ticket
                                               ).ToList();

                ViewBag.CurrentDeveloperAssignedProjects = developerAssignedProjects.Count;

                ViewBag.CurrentDeveloperAssignedTickets = developerAssignedTickets.Count;
            }

            if (User.IsInRole("Submitter"))
            {
                var submitterAssignedProjects = (from project in currentUser.Projects
                                                 where project.Archived == false
                                                 select project
                                                ).ToList();

                var submitterCreatedTickets = (from ticket in DbContext.Tickets
                                                where ticket.CreatedById == userId && ticket.Project.Archived == false
                                               select ticket
                                              ).ToList();

                ViewBag.CurrentSubmitterAssignedProjects = submitterAssignedProjects.Count;
                ViewBag.CurrentSubmitterCreatedTickets = submitterCreatedTickets.Count;
            }

            return View();
        }

    }
}
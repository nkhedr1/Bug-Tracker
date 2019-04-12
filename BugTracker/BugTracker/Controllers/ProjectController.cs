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
    public class ProjectController : Controller
    {
        private ApplicationDbContext DbContext;

        public ProjectController()
        {
            DbContext = new ApplicationDbContext();
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult CreateProject()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult CreateProject(CreateProjectViewModel projectData)
        {
            Project currentProject;

            currentProject = new Project();
            currentProject.Name = projectData.Name;
            currentProject.Id = projectData.Id;
            currentProject.DateCreated = DateTime.Today;
            DbContext.Projects.Add(currentProject);
            DbContext.SaveChanges();
            return RedirectToAction(nameof(ProjectController.ListAllProjects));
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult EditProject(int id)
        {
            var userId = User.Identity.GetUserId();

            var projectToEdit = DbContext.Projects.FirstOrDefault(
                project => project.Id == id);

            var projectModel = new CreateProjectViewModel();
            projectModel.Name = projectToEdit.Name;
            projectModel.DateCreated = projectToEdit.DateCreated;
            projectModel.DateUpdated = projectToEdit.DateUpdated;
            return View(projectModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult EditProject(CreateProjectViewModel projectData)
        {
            var projectToEdit = DbContext.Projects.FirstOrDefault(
                project => project.Id == projectData.Id);

            projectToEdit.Name = projectData.Name;
            projectToEdit.DateUpdated = DateTime.Today;
            DbContext.SaveChanges();
            return RedirectToAction(nameof(ProjectController.ListAllProjects));
        }

        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult ListAllProjects()
        {
            var userId = User.Identity.GetUserId();
            List<CreateProjectViewModel> allProjects;

            allProjects = DbContext.Projects
                               .Select(project => new CreateProjectViewModel
                               {
                                   Id = project.Id,
                                   Name = project.Name,
                                   AssignedUsers = project.Users.Count,
                                   DateUpdated = project.DateUpdated,
                                   DateCreated = project.DateCreated
                               }).ToList();


            return View(allProjects);
        }

        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult AssignUsersToPrjoect(int id)
        {
            var projectToAdd = DbContext.Projects.FirstOrDefault(
               project => project.Id == id);

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


            ViewBag.AllUsers = modelUser;

            return View(projectToAdd);
        }

        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult AddUsersToProject(string userId, int projectId)
        {
            var projectToAddUsers = DbContext.Projects.FirstOrDefault(
              project => project.Id == projectId);

            var userToAdd = DbContext.Users.FirstOrDefault(
               user => user.Id == userId);

            projectToAddUsers.Users.Add(userToAdd);
           
            DbContext.SaveChanges();
            return RedirectToAction(nameof(ProjectController.ListAllProjects));
        }

        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult RemoveUserFromProject(string userId, int projectId)
        {
            var projectToAddUsers = DbContext.Projects.FirstOrDefault(
              project => project.Id == projectId);

            var userToRemove = DbContext.Users.FirstOrDefault(
               user => user.Id == userId);

            projectToAddUsers.Users.Remove(userToRemove);

            DbContext.SaveChanges();

            return RedirectToAction(nameof(ProjectController.ListAllProjects));
        }

        [Authorize]
        public ActionResult ListMyProjects()
        {
            var userId = User.Identity.GetUserId();

            var currentUser = DbContext.Users.FirstOrDefault(
              user => user.Id == userId);

            List<Project> myProjects;

            myProjects = currentUser.Projects.ToList();
            
            return View(myProjects);
        }

    }
}
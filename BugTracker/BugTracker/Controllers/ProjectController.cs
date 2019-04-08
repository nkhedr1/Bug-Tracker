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
            DbContext.Projects.Add(currentProject);
            DbContext.SaveChanges();
            return View();
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

            return View(projectModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult EditProject(CreateProjectViewModel projectData)
        {
            var projectToEdit = DbContext.Projects.FirstOrDefault(
                project => project.Id == projectData.Id);

            projectToEdit.Name = projectData.Name;          
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
                                   Users = project.Users                                
                               }).ToList();

            //Project currentProject;

            //currentProject = new Project();
            //currentProject.Name = projectData.Name;
            //currentProject.Id = projectData.Id;
            //DbContext.Projects.Add(currentProject);
            //DbContext.SaveChanges();
            return View(allProjects);
        }

    }
}
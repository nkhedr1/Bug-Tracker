using BugTracker.Models;
using BugTracker.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static BugTracker.Models.ViewModels.ManageRoleViewModel;

namespace BugTracker.Controllers
{
    public class UserController : Controller
    {
        private ApplicationDbContext DbContext;
        private UserManager<ApplicationUser> UserManager;

        public UserController()
        {
            DbContext = new ApplicationDbContext();
            UserManager =
               new UserManager<ApplicationUser>(
                       new UserStore<ApplicationUser>(DbContext));
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ManageUsers()
        {

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

  
            return View(modelUser);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult AssignUserRoles(string id)
        {

            var model = new ManageRoleViewModel();

            var selectedUser = DbContext.Users.FirstOrDefault(user => user.Id == id);

            var allRolesViewModel = DbContext.Roles.Select(role => new RoleViewModel
            {
                Id = role.Id,
                Name = role.Name
            }).ToList();

            var userRoleViewModel = (from userRoles in selectedUser.Roles
                                     join role in DbContext.Roles on userRoles.RoleId equals role.Id
                                     select new RoleViewModel
                                     {
                                         Id = role.Id,
                                         Name = role.Name
                                     }).ToList();

            model.AllRoles = allRolesViewModel;
            model.UserRoles = userRoleViewModel;

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult AssignUserRoles(string id, List<string> userRoleIds)
        {
            var user = DbContext.Users.FirstOrDefault(p => p.Id == id);

            var userRoles = user.Roles.ToList();

            foreach (var userRole in userRoles)
            {
                var role = DbContext.Roles.First(p => p.Id == userRole.RoleId).Name;
                UserManager.RemoveFromRole(user.Id, role);
            }

            foreach (var userRoleId in userRoleIds)
            {
                var role = DbContext.Roles.First(p => p.Id == userRoleId).Name;
                UserManager.AddToRole(user.Id, role);
            }

            return RedirectToAction(nameof(UserController.ManageUsers));
        }

        public ActionResult Settings()
        {
            var userId = User.Identity.GetUserId();

            var loggedInUser = DbContext.Users.FirstOrDefault(user =>
            user.Id == userId);

            var userSettingsViewPage = new UserSettingsViewModel();
            userSettingsViewPage.Email = loggedInUser.Email;
            userSettingsViewPage.FirstName = loggedInUser.FirstName;
            userSettingsViewPage.Id = loggedInUser.Id;

            return View(userSettingsViewPage);

        }

        public ActionResult ViewUserDetails(string id)
        {
            var userId = id;

            var loggedInUser = DbContext.Users.FirstOrDefault(user =>
            user.Id == userId);

            var userDetailsViewPage = new UserSettingsViewModel();
            userDetailsViewPage.Email = loggedInUser.Email;
            userDetailsViewPage.FirstName = loggedInUser.FirstName;
            userDetailsViewPage.Id = loggedInUser.Id;

            return View(userDetailsViewPage);
        }

        [HttpGet]
        public ActionResult EditUserDetails(string id)
        {
            var userId = id;

            var loggedInUser = DbContext.Users.FirstOrDefault(user =>
            user.Id == userId);

            var EditUserDetails = new UserSettingsViewModel();
            EditUserDetails.Email = loggedInUser.Email;
            EditUserDetails.FirstName = loggedInUser.FirstName;
            EditUserDetails.Id = loggedInUser.Id;

            return View(EditUserDetails);
        }

        [HttpPost]
        public ActionResult EditUserDetails(UserSettingsViewModel editUserData)
        {
            var loggedInUser = DbContext.Users.FirstOrDefault(user =>
            user.Id == editUserData.Id);

            loggedInUser.FirstName = editUserData.FirstName;

            DbContext.SaveChanges();
            return View();
        }

    }
}
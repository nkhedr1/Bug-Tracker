using BugTracker.Models;
using BugTracker.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    public class UserController : Controller
    {
        private ApplicationDbContext DbContext;

        public UserController()
        {
            DbContext = new ApplicationDbContext();
        }
        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ManageUsers()
        {

            var currentUsers = (from user in DbContext.Users
                                select new
                                {
                                    UserId = user.Id,
                                    Username = user.UserName,
                                    Name = user.FirstName,
                                    RoleNames = (from userRole in user.Roles
                                                 join role in DbContext.Roles on userRole.RoleId
                                                 equals role.Id
                                                 select role.Name)
                                }).ToList().Select(p => new ManageUsersViewModel()

                                {
                                    Id = p.UserId,
                                    FirstName = p.Name,
                                    Email = p.Username,
                                    Role = string.Join(",", p.RoleNames)
                                });



            return View(currentUsers);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult AssignUserRoles()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult AssignUserRoles(string role1)
        {
          
            var currentUsers = (from user in DbContext.Users
                                select new
                                {
                                    UserId = user.Id,
                                    Username = user.UserName,
                                    Name = user.FirstName,
                                    RoleNames = (from userRole in user.Roles
                                                 join role in DbContext.Roles on userRole.RoleId
                                                 equals role.Id
                                                 select role.Name)
                                }).ToList().Select(p => new ManageUsersViewModel()

                                {
                                    Id = p.UserId,
                                    FirstName = p.Name,
                                    Email = p.Username,
                                    Role = string.Join(",", p.RoleNames)
                                });

          //  var userToChangeRole = currentUsers.FirstOrDefault(user =>
          //user.Id == id);

          //  userToChangeRole.Role = currentRole;


            return View();
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
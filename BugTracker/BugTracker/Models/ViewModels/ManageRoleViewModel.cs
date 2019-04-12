using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModels
{
    public class ManageRoleViewModel
    {
        public List<RoleViewModel> UserRoles { get; set; }
        public List<RoleViewModel> AllRoles { get; set; }

        public ManageRoleViewModel()
        {
            UserRoles = new List<RoleViewModel>();
            AllRoles = new List<RoleViewModel>();
        }

    }
}
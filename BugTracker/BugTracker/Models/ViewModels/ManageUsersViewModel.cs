using BugTracker.Models.Domain;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModels
{
    public class ManageUsersViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public List<string> CurrentRoles { get; set; }
        public virtual List<Project> Projects { get; set; }
    }
}
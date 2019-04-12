namespace BugTracker.Migrations
{
    using BugTracker.Models;
    using BugTracker.Models.Domain;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<BugTracker.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(BugTracker.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.

            var roleManager =
               new RoleManager<IdentityRole>(
                   new RoleStore<IdentityRole>(context));

            var userManager =
                new UserManager<ApplicationUser>(
                        new UserStore<ApplicationUser>(context));

            if (!context.Roles.Any(user => user.Name == "Admin"))
            {
                var adminRole = new IdentityRole("Admin");
                roleManager.Create(adminRole);
            }

            if (!context.Roles.Any(user => user.Name == "Project Manager"))
            {
                var projectManagerRole = new IdentityRole("Project Manager");
                roleManager.Create(projectManagerRole);
            }

            if (!context.Roles.Any(user => user.Name == "Developer"))
            {
                var developerRole = new IdentityRole("Developer");
                roleManager.Create(developerRole);
            }

            if (!context.Roles.Any(user => user.Name == "Submitter"))
            {
                var submitterRole = new IdentityRole("Submitter");
                roleManager.Create(submitterRole);
            }

            ApplicationUser adminUser;

            if (!context.Users.Any(
                user => user.UserName == "admin@mybugtracker.com"))
            {
                adminUser = new ApplicationUser();
                adminUser.UserName = "admin@mybugtracker.com";
                adminUser.Email = "admin@mybugtracker.com";                
                userManager.Create(adminUser, "Password-1");
                userManager.AddToRole(adminUser.Id, "Admin");
            }
            else
            {
                adminUser = context
                    .Users
                    .First(user => user.UserName == "admin@mybugtracker.com");
            }

            if (!context.TicketTypes.Any(type => type.Name == "Bug"))
            {
                  TicketType ticketTypeBug = new TicketType()
            {
                Name = "Bug"
            };
            context.TicketTypes.Add(ticketTypeBug);

            }

            if (!context.TicketTypes.Any(type => type.Name == "Feature"))
            {
                TicketType ticketTypeFeature = new TicketType()
                {
                    Name = "Feature"
                };
                context.TicketTypes.Add(ticketTypeFeature);
            }

            if (!context.TicketTypes.Any(type => type.Name == "Database"))
            {
                TicketType ticketTypeDatabase = new TicketType()
                {
                    Name = "Database"
                };
                context.TicketTypes.Add(ticketTypeDatabase);
            }

            if (!context.TicketTypes.Any(type => type.Name == "Support"))
            {
                TicketType ticketTypeSupport = new TicketType()
                {
                    Name = "Support"
                };
                context.TicketTypes.Add(ticketTypeSupport);
            }

            if (!context.TicketPriorities.Any(priority => priority.Name == "Low"))
            {
                TicketPriority ticketPriorityLow = new TicketPriority()
                {
                    Name = "Low"
                };
                context.TicketPriorities.Add(ticketPriorityLow);
            }

            if (!context.TicketPriorities.Any(priority => priority.Name == "Medium"))
            {
                TicketPriority ticketPriorityMedium = new TicketPriority()
                {
                    Name = "Medium"
                };
                context.TicketPriorities.Add(ticketPriorityMedium);
            }

            if (!context.TicketPriorities.Any(priority => priority.Name == "High"))
            {
                TicketPriority ticketPriorityHigh = new TicketPriority()
                {
                    Name = "High"
                };
                context.TicketPriorities.Add(ticketPriorityHigh);
            }

            if (!context.TicketStatuses.Any(status => status.Name == "Open"))
            {
                TicketStatus ticketStatusOpen = new TicketStatus()
                {
                    Name = "Open"
                };
                context.TicketStatuses.Add(ticketStatusOpen);
            }

            if (!context.TicketStatuses.Any(status => status.Name == "Resolved"))
            {
                TicketStatus ticketStatusResolved = new TicketStatus()
                {
                    Name = "Resolved"
                };
                context.TicketStatuses.Add(ticketStatusResolved);
            }

            if (!context.TicketStatuses.Any(status => status.Name == "Rejected"))
            {
                TicketStatus ticketStatusRejected = new TicketStatus()
                {
                    Name = "Rejected"
                };
                context.TicketStatuses.Add(ticketStatusRejected);
            }

        }
    }
}

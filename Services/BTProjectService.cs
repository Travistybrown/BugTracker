using BugTracker.Data;
using BugTracker.Enums;
using BugTracker.Helper;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using System.Net.Sockets;

namespace BugTracker.Services
{
    public class BTProjectService : IBTProjectService

    {

        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;

        public BTProjectService(ApplicationDbContext context,
                                IBTRolesService rolesService)
        {
            _context = context;
            _rolesService = rolesService;
        }

        public async Task AddMembersToProjectAsync(IEnumerable<string> userIds, int projectId)
        {
            try
            {
                foreach (string userId in userIds)
                {
                    BTUser? btUser = _context.Users.Find(userId);
                    await AddMemberToProjectAsync(btUser!, projectId);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> AddMemberToProjectAsync(BTUser member, int projectId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId, member.CompanyId);

                bool IsOnProject = project.Members.Any(m => m.Id == member.Id);

                if (!IsOnProject)
                {
                    project.Members.Add(member);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddProjectAsync(Project project)
        {
            try
            {
                _context.Add(project);
                await _context.SaveChangesAsync();

            }

            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> AddProjectManagerAsync(string userId, int projectId, int companyId)
        {
            try
            {
                BTUser? currentPM = await GetProjectManagerAsync(projectId);
                BTUser? selectedPM = await _context.Users.FindAsync(userId);

                // Remove current PM
                if (currentPM != null)
                {
                    await RemoveProjectManagerAsync(projectId);
                }


                // Add new PM
                try
                {
                    await AddMemberToProjectAsync(selectedPM!, projectId);

                    return true;
                }
                catch (Exception)
                {

                    throw;
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task ArchiveProjectAsync(Project project)
        {
            try
            {
                project.Archived = true;
                await UpdateProjectAsync(project);


                //Archive the Tickets for the Project
                foreach (Ticket ticket in project.Tickets)
                {
                    ticket.ArchivedByProject = true;
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }


            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<ProjectPriority>> GetAllProjectPriorityAsync()
        {
            try
            {
                
                List<ProjectPriority> projects = new List<ProjectPriority>();

               
                projects = await _context.ProjectPriorities.OrderBy(p => p.Name).ToListAsync();
                return projects;
                
            }
            catch (Exception)
            {

                throw;
            }
            //throw new NotImplementedException();
        }

        public async Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Project> projects = new();

                projects = await _context.Projects.Where(p => p.CompanyId == companyId)
                                                  .Include(p => p.Company)
                                                  .Include(p => p.ProjectPriority)
                                                  .Include(p => p.Tickets)
                                                  .ThenInclude(p => p.Comments)
                                                  .Include(p => p.Tickets)
                                                  .ThenInclude(p => p.Attachments)
                                                  .Include(p=>p.Tickets)
                                                  .ThenInclude(p => p.History)
                                                  .Include(p => p.Tickets)
                                                  .ThenInclude(p => p.TicketPriority)
                                                  .Include(p => p.Tickets)
                                                  .ThenInclude(p => p.TicketStatus)
                                                  .Include(p => p.Tickets)
                                                  .ThenInclude(p => p.TicketType)
                                                  .Include(p => p.Tickets)
                                                  .ThenInclude(p => p.SubmitterUser)
                                                  .Include(p => p.Tickets)
                                                  .ThenInclude(p => p.DeveloperUser)
                                                  .Include(p => p.Members)
                                                  .ToListAsync();
                return projects;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Project>> GetArchivedProjectsByCompanyIdAsync(int companyId)
        {
            List<Project> projects = await _context.Projects.Where(p => p.CompanyId == companyId && p.Archived == true)
                                                    .Include(p => p.Members)
                                                    .Include(p => p.Tickets)
                                                        .ThenInclude(t => t.Comments)
                                                    .Include(p => p.Tickets)
                                                        .ThenInclude(t => t.Attachments)
                                                    .Include(p => p.Tickets)
                                                        .ThenInclude(t => t.History)
                                                    .Include(p => p.Tickets)
                                                        .ThenInclude(t => t.Notifications)
                                                    .Include(p => p.Tickets)
                                                        .ThenInclude(t => t.DeveloperUser)
                                                    .Include(p => p.Tickets)
                                                        .ThenInclude(t => t.SubmitterUser)
                                                    .Include(p => p.Tickets)
                                                        .ThenInclude(t => t.TicketStatus)
                                                    .Include(p => p.Tickets)
                                                        .ThenInclude(t => t.TicketPriority)
                                                    .Include(p => p.Tickets)
                                                        .ThenInclude(t => t.TicketType)
                                                    .Include(p => p.ProjectPriority)
                                                    .ToListAsync();

            return projects;

        }

        public async Task<Project> GetProjectByIdAsync(int projectId, int companyId)
        {
            try
            {



                Project? project = await _context.Projects
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.TicketPriority)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.TicketStatus)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.TicketType)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.DeveloperUser)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.Attachments)
                                                .Include(p => p.Members)
                                                .Include(p => p.ProjectPriority)
                                                .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);


                return project!;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<BTUser> GetProjectManagerAsync(int projectId)
        {
            try
            {
                Project? project = await _context.Projects
                                                  .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.TicketPriority)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.TicketStatus)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.TicketType)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.DeveloperUser)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.Attachments)
                                                .Include(p => p.Members)
                                                .Include(p => p.ProjectPriority)
                                                .FirstOrDefaultAsync(p => p.Id == projectId);

                foreach (BTUser member in project.Members)
                {
                    if (await _rolesService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager).ToString()))
                    {
                        return member;
                    }
                }

                return null!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string role)
        {
            try
            {
                Project? project = await _context.Projects
                                        .Include(p => p.Members)
                                        .FirstOrDefaultAsync(p => p.Id == projectId);

                List<BTUser> members = new();

                foreach (var user in project.Members)
                {
                    if (await _rolesService.IsUserInRoleAsync(user, role))
                    {
                        members.Add(user);
                    }
                }

                return members;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Project>> GetUnassignedProjectsAsync(int companyId)
        {
            List<Project> result = new();
            List<Project> projects = new();

            try
            {
                projects = await _context.Projects
                                         .Include(p => p.ProjectPriority)
                                         .Where(p => p.CompanyId == companyId).ToListAsync();

                foreach (Project project in projects)
                {
                    if ((await GetProjectMembersByRoleAsync(project.Id, nameof(BTRoles.ProjectManager))).Count == 0)
                    {
                        result.Add(project);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }

            return result;
        }

        public async Task<List<Project>?> GetUserProjectsAsync(string userId)
        {
            try
            {
                List<Project>? projects = (await _context.Users!
                    .Include(u => u.Projects)
                    .ThenInclude(u => u.Company)
                    .Include(u => u.Projects)
                    .ThenInclude(p => p.Members)
                    .Include(u => u.Projects)
                    .ThenInclude(p => p.Tickets)
                    
                    .FirstOrDefaultAsync(u => u.Id == userId))?.Projects.Where(t => t.Archived == false).ToList();




                return projects;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> IsUserOnProjectAsync(string userId, int projectId)
        {
            try
            {
                Project? project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);

                bool result = false;

                if (project != null)
                {
                    result = project.Members.Any(m => m.Id == userId);
                }

                return result;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RemoveCurrentMembersAsync(int projectId, int companyId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId, companyId);
                foreach (var member in project.Members)
                {
                    if (!await _rolesService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                    {
                        project.Members.Remove(member);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> RemoveMemberFromProjectAsync(BTUser member, int projectId, int companyId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId, companyId);

                bool IsOnProject = project.Members.Any(m => m.Id == member.Id);

                if (IsOnProject)
                {
                    project.Members.Remove(member);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task RemoveProjectManagerAsync(int projectId)
        {
            try
            {
                Project? project = await _context.Projects
                                                .Include(p => p.Members)
                                                .FirstOrDefaultAsync(p => p.Id == projectId);
                foreach (BTUser member in project?.Members)
                {
                    if (await _rolesService.IsUserInRoleAsync(member, BTRoles.ProjectManager.ToString()))
                    {
                        await RemoveUserFromProjectAsync(member.Id, projectId);
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task RemoveUserFromProjectAsync(string userId, int projectId)
        {
            try
            {
                BTUser? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                Project? project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

                try
                {
                    if (await IsUserOnProjectAsync(userId, projectId))
                    {
                        project!.Members.Remove(user);
                        await _context.SaveChangesAsync();
                    }

                }
                catch (Exception)
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"**** ERROR **** - Error Removing User from project.  --->  {ex.Message}");
            }
        }

        public async Task RestoreProjectAsync(Project project)
        {
            try
            {
                await UpdateProjectAsync(project);

                //Archive the Tickets for the Project
                foreach (Ticket ticket in project.Tickets)
                {
                    ticket.ArchivedByProject = false;
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task UpdateProjectAsync(Project project)
        {
            try
            {
                project.Created = PostgresDate.Format(project.Created);
                project.StartDate = PostgresDate.Format(project.StartDate);
                project.EndDate = PostgresDate.Format(project.EndDate);
                _context.Update(project);
                await _context.SaveChangesAsync();

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}

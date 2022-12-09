using BugTracker.Models;

namespace BugTracker.Services.Interfaces
{
    public interface IBTProjectService
    {
        
        public Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId);
        public Task<bool> AddProjectManagerAsync(string userId, int projectId, int comapnyId);
        public Task<bool> AddMemberToProjectAsync(BTUser member, int projectId);
        public Task<List<Project>> GetArchivedProjectsByCompanyIdAsync(int companyId);
        public Task AddProjectAsync(Project project);
        public Task<Project> GetProjectByIdAsync(int projectId, int companyId);
        public Task<BTUser> GetProjectManagerAsync( int projectId);
        public Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string role);
        public Task<List<Project>?> GetUserProjectsAsync(string userId);
        public Task UpdateProjectAsync(Project project);
        public Task<bool> IsUserOnProjectAsync(string userId, int projectId);
        public Task ArchiveProjectAsync(Project project);
        public Task RemoveProjectManagerAsync(int projectId);
        public Task RemoveUserFromProjectAsync(string userId, int projectId);
        public Task<bool> RemoveMemberFromProjectAsync(BTUser member, int projectId, int companyId);
        public Task RestoreProjectAsync(Project project);
        public Task<IEnumerable<ProjectPriority>> GetAllProjectPriorityAsync();

        public Task<List<Project>> GetUnassignedProjectsAsync(int companyId);



    }
}

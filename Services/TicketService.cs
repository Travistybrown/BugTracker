using BugTracker.Data;
using BugTracker.Enums;
using BugTracker.Helper;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;

namespace BugTracker.Services
{
    public class TicketService : ITicketService
    {

        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;
        private readonly IBTProjectService _projectService;



        public TicketService(ApplicationDbContext context)

        {
            _context = context;

        }

        public async Task AddCommentAsync(TicketComment ticketComment)
        {
            try
            {
                _context.Add(ticketComment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task AddDevloperAsync(int ticketId)
        {
            throw new NotImplementedException();
        }

        public async Task AddTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment)
        {
            try
            {
                await _context.AddAsync(ticketAttachment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task ArchiveTicketAsync(Ticket ticket)
        {
            try
            {
                ticket.Archived = true;
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

      

        public async Task<List<Ticket>> GetAllTicketsByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets = await _context.Projects
                                                     .Where(p => p.CompanyId == companyId)
                                                     .SelectMany(p => p.Tickets)
                                                        .Include(t => t.Attachments)
                                                        .Include(t => t.Comments)
                                                        .Include(t => t.DeveloperUser)
                                                        .Include(t => t.History)
                                                        .Include(t => t.SubmitterUser)
                                                        .Include(t => t.TicketPriority)
                                                        .Include(t => t.TicketStatus)
                                                        .Include(t => t.TicketType)
                                                        .Include(t => t.Project)
                                                     .ToListAsync();

                return tickets;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Ticket> GetTicketAsNoTrackingAsync(int ticketId, int companyId)
        {
            try
            {
                Ticket? ticket = await _context.Tickets
                                          .Include(t => t.DeveloperUser)
                                             .Include(t => t.SubmitterUser)
                                             .Include(t => t.Project)
                                             .Include(t => t.TicketPriority)
                                             .Include(t => t.TicketStatus)
                                             .Include(t => t.TicketType)
                                             .Include(t => t.Comments)
                                             .Include(t => t.Attachments)
                                             .Include(t => t.History)
                                             .AsNoTracking()
                                             .FirstOrDefaultAsync(t => t.Id == ticketId && t.Project!.CompanyId == companyId && t.Archived == false)!;

                return ticket!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Ticket?> GetTicketByIdAsync(int ticketId, int companyId)
        {
            try
            {
                Ticket? ticket = await _context.Tickets
                                              .Include(t => t.DeveloperUser)
                                                 .Include(t => t.SubmitterUser)
                                                 .Include(t => t.Project)
                                                 .Include(t => t.TicketPriority)
                                                 .Include(t => t.TicketStatus)
                                                 .Include(t => t.TicketType)
                                                 .Include(t => t.Comments)
                                                 .ThenInclude(t => t.User)
                                                 .Include(t => t.Attachments)
                                                 .Include(t => t.History)
                                                 .FirstOrDefaultAsync(t => t.Id == ticketId && t.Project!.CompanyId == companyId )!;

                return ticket ?? new Ticket();
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<List<Ticket>> GetTicketsByUserIdAsync(string userId, int companyId)
        {
            BTUser? btUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            List<Ticket>? tickets = (await _projectService.GetAllProjectsByCompanyIdAsync(companyId)).Where(p => p.Archived == false)
                                                    .SelectMany(p => p.Tickets!).Where(t => !(t.Archived | t.ArchivedByProject)).ToList();


            try
            {
                if (await _rolesService.IsUserInRoleAsync(btUser!, nameof(BTRoles.Admin)))
                {
                    return tickets;
                }
                else if (await _rolesService.IsUserInRoleAsync(btUser!, nameof(BTRoles.Developer)))
                {
                    return tickets.Where(t => t.Archived == false && t.DeveloperUserId == userId || t.SubmitterUserId == userId).ToList();

                }
                else if (await _rolesService.IsUserInRoleAsync(btUser!, nameof(BTRoles.Submitter)))
                {
                    return tickets.Where(t => t.SubmitterUserId == userId).ToList();
                }
                else if (await _rolesService.IsUserInRoleAsync(btUser!, nameof(BTRoles.ProjectManager)))
                {
                    List<Ticket>? projectTickets = (await _projectService.GetUserProjectsAsync(userId)).SelectMany(t => t.Tickets!).Where(t => t.Archived | t.ArchivedByProject).ToList();
                    List<Ticket>? submittedTickets = tickets.Where(t => t.SubmitterUserId == userId).ToList();
                    return tickets = projectTickets.Concat(submittedTickets).ToList();

                }

                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetUnassignedTicketsAsync(int companyId)
        {
            List<Ticket> tickets = new();

            try
            {
                tickets = (await GetAllTicketsByCompanyIdAsync(companyId)).Where(t => string.IsNullOrEmpty(t.DeveloperUserId)).ToList();
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

     

        public Task RestoreTicketAsync(Ticket ticket)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateTicketAsync(Ticket ticket)
        {
            try
            {
                ticket.Created = PostgresDate.Format(ticket.Created);
                _context.Update(ticket);
                await _context.SaveChangesAsync();



            }
            catch (Exception)
            {



                throw;
            }
        }
    }
}

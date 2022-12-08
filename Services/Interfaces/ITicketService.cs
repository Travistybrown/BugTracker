using BugTracker.Models;
using System.Runtime.CompilerServices;

namespace BugTracker.Services.Interfaces
{
    public interface ITicketService
    {
        public Task AddTicketAsync(Ticket ticket);
        public Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment);
        public Task AddCommentAsync(TicketComment ticketComment);
        public Task AddDevloperAsync(int ticketId);
        public Task UpdateTicketAsync(Ticket ticket);
        public Task<Ticket> GetTicketByIdAsync(int ticketId, int companyId);
        public Task<Ticket> GetTicketAsNoTrackingAsync(int ticketId, int companyId);
        public Task<List<Ticket>> GetTicketsByUserIdAsync(string userId, int companyId);
        public Task ArchiveTicketAsync(Ticket ticket);
        public Task RestoreTicketAsync(Ticket ticket);
        public Task<List<Ticket>> GetAllTicketsByCompanyIdAsync(int companyId);
        public Task<List<Ticket>> GetUnassignedTicketsAsync(int companyId);
       

    }
}

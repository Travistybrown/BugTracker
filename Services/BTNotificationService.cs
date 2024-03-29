﻿using BugTracker.Data;
using BugTracker.Enums;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Services
{
    public class BTNotificationService : IBTNotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailService;
        private readonly IBTRolesService _rolesService;

        public BTNotificationService(ApplicationDbContext context,
                                     IEmailSender emailService,
                                     IBTRolesService rolesService)
        {
            _context = context;
            _emailService = emailService;
            _rolesService = rolesService;
        }

        public async Task AddNotificationAsync(Notification notification)
        {
            try
            {
                await _context.AddAsync(notification);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AdminNotificationAsync(Notification notification, int companyId)
        {
            try
            {
                IEnumerable<string> adminIds = (await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Admin), companyId)).Select(a => a.Id);


                foreach (string adminId in adminIds)
                {
                    notification.Id = 0;
                    notification.RecipientId = adminId;

                    await _context.AddAsync(notification);
                }

                await _context.SaveChangesAsync();

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Notification>> GetNotificationsByUserIdAsync(string userId)
        {
            try
            {
                List<Notification> notifications = new();

                notifications = await _context.Notifications
                                              .Where(n => n.SenderId == userId || n.RecipientId == userId)
                                              .Include(n => n.Recipient)
                                              .Include(n => n.Sender)
                                              .ToListAsync();
                return notifications;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> SendEmailNotificationAsync(Notification notification, string emailSubject)
        {
            try
            {
                BTUser? btUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == notification.RecipientId);

                string userEmail = btUser!.Email;

                await _emailService.SendEmailAsync(userEmail, emailSubject, notification.Message);
                return true;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> SendAdminEmailNotificationAsync(Notification notification, string emailSubject, int companyId)
        {
            try
            {
                IEnumerable<string> adminEmails = (await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Admin), companyId)).Select(a => a.Email);


                foreach (string adminEmail in adminEmails)
                {
                    await _emailService.SendEmailAsync(adminEmail, emailSubject, notification.Message);
                }

                return true;

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}

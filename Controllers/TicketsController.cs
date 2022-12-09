using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Authorization;
using BugTracker.Enums;
using BugTracker.Services;
using Microsoft.AspNetCore.Identity;
using BugTracker.Services.Interfaces;
using BugTracker.Helper;
using BugTracker.Extensions;
using BugTracker.Models.ViewModels;
using System.ComponentModel.Design;

namespace BugTracker.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTRolesService _bTRolesService;
        private readonly ITicketService _ticketService;
        private readonly IBTProjectService _ProjectService;
        private readonly IFileService _fileService;
        private readonly IBTTicketHistoryService _historyService;

        public TicketsController(ApplicationDbContext context,
                                 UserManager<BTUser> userManager,
                                 IBTRolesService bTRolesService,
                                 ITicketService ticketService,
                                 IBTProjectService bTProjectService,
                                 IFileService fileService,
                                 IBTTicketHistoryService historyService)
        {
            _context = context;
            _userManager = userManager;
            _bTRolesService = bTRolesService;
            _ticketService = ticketService;
            _ProjectService = bTProjectService;
            _fileService = fileService;
            _historyService = historyService;
        }
        //My Tickets

        [HttpGet]
        public async Task<IActionResult> MyTickets()
        {
            string userId = _userManager.GetUserId(User);
            int companyId = User.Identity!.GetCompanyId();

            List<Ticket> ticket = new();
            if (User.IsInRole(nameof(BTRoles.Admin)))
            {
                // Make a call to the service for all projects
                ticket = await _ticketService.GetAllTicketsByCompanyIdAsync(companyId);
            }
            else
            {


                ticket = (await _ticketService.GetAllTicketsByCompanyIdAsync(companyId))
                                                       .Where(t => t.SubmitterUserId == userId).ToList();


            }
            return View(ticket);
        }

        //Unassigned Tickets
        [HttpGet]
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> UnassignedTickets()
        {
            int companyId = User.Identity.GetCompanyId();
            string btUserId = _userManager.GetUserId(User);

            List<Ticket> tickets = await _ticketService.GetUnassignedTicketsAsync(companyId);

            return View(tickets);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketAttachment([Bind("Id,FormFile,Description,TicketId")] TicketAttachment ticketAttachment)
        {
            string statusMessage;

            if (ModelState.IsValid && ticketAttachment.FormFile != null)
            {
                ticketAttachment.FileData = await _fileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
                ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
                ticketAttachment.FileType = ticketAttachment.FormFile.ContentType;

                ticketAttachment.Created = DateTime.UtcNow;
                ticketAttachment.UserId = _userManager.GetUserId(User);

                await _ticketService.AddTicketAttachmentAsync(ticketAttachment);
                statusMessage = "Success: New attachment added to Ticket.";
            }
            else
            {
                statusMessage = "Error: Invalid data.";

            }

            return RedirectToAction("Details", new { id = ticketAttachment.TicketId, message = statusMessage });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketComment([Bind("Id,Comment,Created,TicketId,UserId")] TicketComment ticketComment)
        {
            ModelState.Remove("UserId");
            if (ModelState.IsValid)
            {
                ticketComment.UserId = _userManager.GetUserId(User);

                ticketComment.Created = DateTime.UtcNow;

                _context.Add(ticketComment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Tickets", new { id = ticketComment.TicketId });
            }
            ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description", ticketComment.TicketId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", ticketComment.UserId);
            return View(ticketComment);
        }
        [HttpGet]
        [Authorize(Roles = nameof(BTRoles.Admin))]
        public async Task<IActionResult> AssignDeveloper(int? ticketId, int companyId)
        {
            if (ticketId == null)
            {
                return NotFound();
            }

            AssignDeveloperViewModel model = new();


            model.Ticket = await _ticketService.GetTicketByIdAsync(ticketId.Value, companyId);
            model.DeveloperList = new SelectList(await _ProjectService.GetProjectMembersByRoleAsync(model.Ticket.ProjectId, nameof(BTRoles.Developer)), "Id", "FullName");

            return View(model);
        }


        // GET: Tickets
        [Authorize(Roles = "Admin , Developer")]
        public async Task<IActionResult> Index()
        {
            int companyId = User.Identity!.GetCompanyId();

            List<Ticket> tickets = await _ticketService.GetAllTicketsByCompanyIdAsync(companyId);
            return View(tickets);
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.DeveloperUser)
                .Include(t => t.Project)
                .Include(t => t.SubmitterUser)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .Include(t => t.Comments)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            BTUser bTUser = await _userManager.GetUserAsync(User);
            List<Project>? projects = new();
            if (User.IsInRole(nameof(BTRoles.Admin)))
            {
                projects = await _ProjectService.GetAllProjectsByCompanyIdAsync(bTUser.CompanyId);
            }
            else
            {
                projects = await _ProjectService.GetUserProjectsAsync(bTUser.Id);
            }

            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name");
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description");
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name");
            return View();







        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,ProjectId,TicketTypeId,TicketPriorityId")] Ticket ticket)
        {






            ModelState.Remove("SubmitterUserId");



            if (ModelState.IsValid)
            {
                string userId = _userManager.GetUserId(User);
                // Created date
                ticket.Created = DateTime.UtcNow;
                //submitteruserid
                ticket.SubmitterUserId = userId;

                ticket.TicketStatusId = (await _context.TicketStatuses.FirstOrDefaultAsync(t => t.Name == nameof(BTTicketStatuses.New)))!.Id;



                // call the ticket service
                await _ticketService.AddTicketAsync(ticket);



                //Add History Record


                int companyId = User.Identity.GetCompanyId();
                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);

                await _historyService.AddHistoryAsync(null!, newTicket, userId);



                return RedirectToAction(nameof(Index));


            }



            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);

        }

        // GET: Tickets/Edit/5
        [Authorize(Roles = "Admin , Developer")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            // call the ticket service
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value, companyId);

            BTUser user = await _userManager.GetUserAsync(User);

            bool canEdit = ticket.SubmitterUserId == user.Id || ticket.DeveloperUserId == user.Id || // is the user the submitter or developer?
                            (ticket.Project?.CompanyId == user.CompanyId && User.IsInRole(nameof(BTRoles.Admin))) || // is the user an admin of the company?
                            ((await _ProjectService.GetProjectManagerAsync(ticket.ProjectId))?.Id == user.Id); // is the user the PM?

            if (ticket == null)
            {
                return NotFound();
            }

            if (canEdit == null)
            {
                return Unauthorized();
            }


            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["DeveloperUserId"] = new SelectList(await _bTRolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), user.CompanyId), "Id", "FullName", ticket.DeveloperUserId);
            ViewData["SubmitterUserId"] = new SelectList(await _bTRolesService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), user.CompanyId), "Id", "FullName", ticket.SubmitterUserId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin , Developer")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId,SubmitterUserId")] Ticket ticket)
        {
            BTUser user = await _userManager.GetUserAsync(User);

            if (id != ticket.Id)
            {
                return NotFound();
            }


            bool canEdit = ticket.SubmitterUserId == user.Id || ticket.DeveloperUserId == user.Id || // is the user the submitter or developer?
                            (ticket.Project?.CompanyId == user.CompanyId && User.IsInRole(nameof(BTRoles.Admin))) || // is the user an admin of the company?
                            ((await _ProjectService.GetProjectManagerAsync(ticket.ProjectId))?.Id == user.Id); // is the user the PM?

            if (canEdit == null)
            {
                return Unauthorized();
            }



            if (ModelState.IsValid)
            {

                int companyId = User.Identity!.GetCompanyId();
                string userId = _userManager.GetUserId(User);
                Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);

                try
                {
                    ticket.Created = DateTime.SpecifyKind(ticket.Created, DateTimeKind.Utc);
                    ticket.Updated = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);


                    await _ticketService.UpdateTicketAsync(ticket);
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }


                // Add History
                Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);
                await _historyService.AddHistoryAsync(oldTicket, newTicket, userId);



                return RedirectToAction(nameof(Index));
            }
            //ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.DeveloperUserId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);
            //ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.SubmitterUserId);
            ViewData["DeveloperUserId"] = new SelectList(await _bTRolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), user.CompanyId), "Id", "FullName", ticket.DeveloperUserId);
            ViewData["SubmitterUserId"] = new SelectList(await _bTRolesService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), user.CompanyId), "Id", "FullName", ticket.SubmitterUserId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);

            return View(ticket);
        }


        // GET: Projects/Archive/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;
            // TODO: call the service
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value, companyId);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Projects/Archive/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            // TODO: call project service
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id, companyId);

            if (ticket != null)
            {
                // TODO: call project Servce (ArchiveProjectasync)
                // 1. set the "Archived" propery to "true"
                ticket.Archived = true;
                //2. send the project to the service for update.
                await _ticketService.UpdateTicketAsync(ticket);








            }



            return RedirectToAction("Index", "Home");
        }

        // GET: Projects/Restore/5
        [Authorize(Roles = "Admin , ProjectManager")]
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;
            // TODO: call the service
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value, companyId);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Projects/Restore/5
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            // TODO: call project service
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id, companyId);

            if (ticket != null)
            {
                // TODO: call project Servce (ArchiveProjectasync)
                // 1. set the "Archived" propery to "true"

                //2. send the project to the service for update.




                ticket.Archived = false;
                await _ticketService.UpdateTicketAsync(ticket); 




            }



            return RedirectToAction(nameof(Index));
        }
        //Archived Tickets
        public async Task<IActionResult> ArchivedTickets()
        {
            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;
            List<Ticket> tickets = (await _ticketService.GetAllTicketsByCompanyIdAsync(companyId)).Where(p => p.Archived == true).ToList();

            return View(tickets);
        }
        private async Task<bool> TicketExists(int id)
        {
            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            return (await _ticketService.GetAllTicketsByCompanyIdAsync(companyId)).Any(t => t.Id == id);
        }
    }
}

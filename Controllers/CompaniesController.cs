using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Models.ViewModels;
using BugTracker.Extensions;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace BugTracker.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CompaniesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTCompanyService _companyService;
        private readonly IBTRolesService _rolesService;
        private readonly UserManager<BTUser> _userManager;

        public CompaniesController(ApplicationDbContext context,
                                   IBTCompanyService companyService,
                                   IBTRolesService rolesService,
                                   UserManager<BTUser> userManager)
        {
            _context = context;
            _companyService = companyService;
            _rolesService = rolesService;
            _userManager = userManager;
        }

        // GET: Companies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Companies.ToListAsync());
        }

        // GET: Companies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserRoles()
        {
            // Add an instance of the ViewModel as a List (model)
            List<ManageUserRolesViewModel> model = new();

            // Get CompanyId
            int companyId = User.Identity!.GetCompanyId();

            // Get all company users
            List<BTUser> members = await _companyService.GetMembersAsync(companyId);


            string btUserId = _userManager.GetUserId(User);

            foreach (BTUser member in members)
            {
                if (string.Compare(btUserId,member.Id) != 0)
                {
                    ManageUserRolesViewModel viewModel = new();

                    IEnumerable<string> currentRoles = await _rolesService.GetUserRolesAsync(member);

                    viewModel.BTUser = member;
                    viewModel.Roles = new MultiSelectList(await _rolesService.GetRolesAsync(), "Name", "Name", currentRoles);

                    model.Add(viewModel);
                }
            }

            return View(model);

        }

        public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel model)
        {
            // Get the company Id
            int companyId = User.Identity!.GetCompanyId();

            // Instantiate the BTUser
            BTUser? btUser = (await _companyService.GetMembersAsync(companyId)).FirstOrDefault(m => m.Id == model.BTUser!.Id);

            // Get Role for the User
            IEnumerable<string> currentRoles = await _rolesService.GetUserRolesAsync(btUser!);

            // Get Selected Role(s) for the User
            string? selectedRole = model.SelectedRoles!.FirstOrDefault();

            // Remove current role(s) and Add new role
            if (!string.IsNullOrEmpty(selectedRole))
            {
                if (await _rolesService.RemoveUserFromRoleAsync(btUser!, selectedRole))
                {
                    await _rolesService.AddUserToRoleAsync(btUser!, selectedRole);
                }
            }

            // Navigate
            return RedirectToAction(nameof(ManageUserRoles));
        }
    }
}
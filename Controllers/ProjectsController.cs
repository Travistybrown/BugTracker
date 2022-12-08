using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using BugTracker.Services.Interfaces;
using BugTracker.Helper;
using BugTracker.Enums;
using BugTracker.Extensions;
using Microsoft.AspNetCore.CookiePolicy;
using BugTracker.Models.ViewModels;
using System.ComponentModel.Design;


namespace BugTracker.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IFileService _fileService;
        private readonly IBTProjectService _projectService;
        private readonly IBTRolesService _btRolesService;

        public ProjectsController(ApplicationDbContext context,
                                  UserManager<BTUser> userManager,
                                  IFileService fileService,
                                  IBTProjectService projectService,
                                  IBTRolesService bTRolesService)
        {
            _context = context;
            _userManager = userManager;
            _fileService = fileService;
            _projectService = projectService;
            _btRolesService = bTRolesService;
        }
        //[HttpGet]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> ArchivedProjects()
        //{
        //    BTUser user = await _userManager.GetUserAsync(User);
        //    int companyId = user.CompanyId;
        //    List<Project> project = await _projectService.GetArchivedProjectsByCompanyIdAsync(companyId);

        //    return View(project);
        //}
        // GET: Projects

        public IActionResult Index()
        {
            return RedirectToAction(nameof(AllProjects));
        }

        public async Task<IActionResult> AllProjects()
        {
            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            List<Project> projects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);

            return View(projects);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnassignedProjects()
        {
            int companyId = User.Identity.GetCompanyId();

            List<Project> projects = await _projectService.GetUnassignedProjectsAsync(companyId);

            return View(projects);

        }
        //Assign Project Managers to Projects
        [HttpGet]
        [Authorize(Roles = nameof(BTRoles.Admin))]

        public async Task<IActionResult> AssignPM(int? projectId)
        {
            if (projectId == null)
            {
                return NotFound();
            }
            List<BTUser> projectManagers = await _btRolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), User.Identity!.GetCompanyId());
            BTUser? currentPm = await _projectService.GetProjectManagerAsync(projectId.Value);

            AssignPmViewModel viewModel = new()
            {
                Project = await _projectService.GetProjectByIdAsync(projectId.Value, User.Identity!.GetCompanyId()),
                PmList = new SelectList(projectManagers, "Id", "FullName", currentPm?.Id),
                PMId = currentPm?.Id,
            };
            return View(viewModel);


         
        }

        [HttpPost]
        [Authorize(Roles = nameof(BTRoles.Admin))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPM(AssignPmViewModel viewModel, int companyId)
        {
            if (viewModel.Project?.Id != null)
            {
                if (!string.IsNullOrEmpty(viewModel.PMId))
                {
                    await _projectService.AddProjectManagerAsync(viewModel.PMId, viewModel.Project.Id, companyId);
                }
                else
                {
                    await _projectService.RemoveProjectManagerAsync(viewModel.Project.Id);
                }

            }
            return RedirectToAction(nameof(Details), new { id = viewModel.Project?.Id });
        }


        // GET: Useers Projects

        public async Task<IActionResult> MyProjects()
        {
            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            List<Project> projects = new();
            if (User.IsInRole(nameof(BTRoles.Admin)))
            {
                // Make a call to the service for all projects
                projects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);
            }
            else
            {
                // 1. create a new service to get user projects???
                // 2. make a call to the service
                string userId = _userManager.GetUserId(User);
                projects = await _projectService.GetUserProjectsAsync(userId);
            }



            return View(projects);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> ArchivedProjects()
        {
            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;
            List<Project> projects = (await _projectService.GetAllProjectsByCompanyIdAsync(companyId)).Where(p => p.Archived == true).ToList();

            return View(projects);
        }


        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }


        [Authorize(Roles = "Admin")]
        // GET: Projects/Create
        public async Task<IActionResult> Create()
        {
            //ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name");
            List<BTUser> projectManagers = await _btRolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), User.Identity!.GetCompanyId());
           ViewData["ProjectManagersList"] = new SelectList(projectManagers, "Id", "FullName");
            // TODO: call project service
            ViewData["ProjectPriorityId"] = new SelectList(await _projectService.GetAllProjectPriorityAsync(), "Id", "Name");
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CompanyId,Name,Description,Created,StartDate,EndDate,ProjectPriorityId,ImageFileData,ImageFileType,Archived")] Project project)
        {
            if (ModelState.IsValid)
            {
                // get company id 

                project.CompanyId = (await _userManager.GetUserAsync(User)).CompanyId;


                // set created date
                if (project.ImageFormFile != null)
                {
                    project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                    project.ImageFileType = project.ImageFormFile.ContentType;
                }

                project.Created = DateTime.UtcNow;
                project.StartDate = DateTime.SpecifyKind(project.StartDate, DateTimeKind.Utc);
                project.EndDate = DateTime.SpecifyKind(project.EndDate, DateTimeKind.Utc);

                //TO: call project service
                await _projectService.AddProjectAsync(project);

                _context.Update(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(AllProjects));
            }

            ViewData["ProjectPriorityId"] = new SelectList(await _projectService.GetAllProjectPriorityAsync(), "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        [Authorize(Roles = "Admin")]
        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            // TODO: call project service
            List<BTUser> projectManagers = await _btRolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), User.Identity!.GetCompanyId());
            BTUser? currentPm = await _projectService.GetProjectManagerAsync(id.Value);

            AssignPmViewModel viewModel = new()
            {
                Project = await _projectService.GetProjectByIdAsync(id.Value, User.Identity!.GetCompanyId()),
                PmList = new SelectList(projectManagers, "Id", "FullName", currentPm?.Id),
                PMId = currentPm?.Id,
            };
            Project project = viewModel.Project;
            if (project == null)
            {
                return NotFound();
            }
            //ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(await _projectService.GetAllProjectPriorityAsync(), "Id", "Name", project.ProjectPriorityId);
            return View(viewModel);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AssignPmViewModel viewModel)
        {
            Project project = viewModel.Project;

            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //Dates
                    project.Created = PostgresDate.Format(project.Created);
                    project.StartDate = PostgresDate.Format(project.StartDate);
                    project.EndDate = PostgresDate.Format(project.EndDate);

                    //Image
                    if (project.ImageFormFile != null)
                    {
                        project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                        project.ImageFileType = project.ImageFormFile.ContentType;
                    }

                    await _projectService.UpdateProjectAsync(project);

                    if (viewModel.Project?.Id != null)
                    {
                        if (!string.IsNullOrEmpty(viewModel.PMId))
                        {
                            await _projectService.AddProjectManagerAsync(viewModel.PMId, viewModel.Project.Id,  User.Identity!.GetCompanyId());
                        }
                        else
                        {
                            await _projectService.RemoveProjectManagerAsync(viewModel.Project.Id);
                        }

                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(await _projectService.GetAllProjectPriorityAsync(), "Id", "Name", project.ProjectPriorityId);
            return View(project);
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
            var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
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
            var project = await _projectService.GetProjectByIdAsync(id, companyId);

            if (project != null)
            {
                // TODO: call project Servce (ArchiveProjectasync)
                // 1. set the "Archived" propery to "true"
                project.Archived = true;
                //2. send the project to the service for update.
                await _projectService.UpdateProjectAsync(project);








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
            var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
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
            var project = await _projectService.GetProjectByIdAsync(id, companyId);

            if (project != null)
            {
                // TODO: call project Servce (ArchiveProjectasync)
                // 1. set the "Archived" propery to "true"

                //2. send the project to the service for update.




                project.Archived = false;
                await _projectService.UpdateProjectAsync(project);




            }



            return RedirectToAction(nameof(AllProjects));
        }


        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}

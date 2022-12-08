using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;

namespace BugTracker.Areas
{
    [Area("TicketStatController.cs")]
    public class TicketStatController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TicketStatController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TicketStatController.cs/TicketStat
        public async Task<IActionResult> Index()
        {
              return View(await _context.TicketStatuses.ToListAsync());
        }

        // GET: TicketStatController.cs/TicketStat/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.TicketStatuses == null)
            {
                return NotFound();
            }

            var ticketStatus = await _context.TicketStatuses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticketStatus == null)
            {
                return NotFound();
            }

            return View(ticketStatus);
        }

        // GET: TicketStatController.cs/TicketStat/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TicketStatController.cs/TicketStat/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] TicketStatus ticketStatus)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ticketStatus);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ticketStatus);
        }

        // GET: TicketStatController.cs/TicketStat/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.TicketStatuses == null)
            {
                return NotFound();
            }

            var ticketStatus = await _context.TicketStatuses.FindAsync(id);
            if (ticketStatus == null)
            {
                return NotFound();
            }
            return View(ticketStatus);
        }

        // POST: TicketStatController.cs/TicketStat/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] TicketStatus ticketStatus)
        {
            if (id != ticketStatus.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticketStatus);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketStatusExists(ticketStatus.Id))
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
            return View(ticketStatus);
        }

        // GET: TicketStatController.cs/TicketStat/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.TicketStatuses == null)
            {
                return NotFound();
            }

            var ticketStatus = await _context.TicketStatuses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticketStatus == null)
            {
                return NotFound();
            }

            return View(ticketStatus);
        }

        // POST: TicketStatController.cs/TicketStat/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.TicketStatuses == null)
            {
                return Problem("Entity set 'ApplicationDbContext.TicketStatuses'  is null.");
            }
            var ticketStatus = await _context.TicketStatuses.FindAsync(id);
            if (ticketStatus != null)
            {
                _context.TicketStatuses.Remove(ticketStatus);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketStatusExists(int id)
        {
          return _context.TicketStatuses.Any(e => e.Id == id);
        }
    }
}

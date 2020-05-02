using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FamilyTreeWebTools.Data;
using FamilyTreeWebApp.Data;

namespace FamilyTreeServices.Pages.IssueResults
{
  public class EditModel : PageModel
  {
    private readonly FamilyTreeDbContext _context;

    public EditModel(FamilyTreeDbContext context)
    {
      _context = context;
    }

    [BindProperty]
    public Issue Issue { get; set; }
    public Profile Profile1 { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      Issue = await _context.Issues.FirstOrDefaultAsync(m => m.Id == id);

      if (Issue == null)
      {
        return NotFound();
      }
      Profile1 = await _context.Profiles.FindAsync(Issue.ProfileId);

      if (Profile1 == null)
      {
        return NotFound();
      }
      return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
      if (!ModelState.IsValid)
      {
        return Page();
      }

      _context.Attach(Issue).State = EntityState.Modified;

      try
      {
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!ProblemExists(Issue.Id))
        {
          return NotFound();
        }
        else
        {
          throw;
        }
      }

      return Redirect("./Edit?Id=" + Issue.Id);
    }

    private bool ProblemExists(int id)
    {
      return _context.Issues.Any(e => e.Id == id);
    }
  }
}

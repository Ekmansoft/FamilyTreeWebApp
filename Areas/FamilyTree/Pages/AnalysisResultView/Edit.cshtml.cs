using FamilyTreeWebApp.Data;
using FamilyTreeWebTools.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace FamilyTreeServices.Pages.AnalysisResultView
{
  [Authorize]
  public class EditModel : PageModel
  {
    private readonly FamilyTreeDbContext _context;

    public EditModel(FamilyTreeDbContext context)
    {
      _context = context;
    }

    [BindProperty]
    public Analysis Analysis { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      Analysis = await _context.Analyses.FirstOrDefaultAsync(m => m.Id == id);

      if (Analysis == null)
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

      _context.Attach(Analysis).State = EntityState.Modified;

      try
      {
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!AnalysisExists(Analysis.Id))
        {
          return NotFound();
        }
        else
        {
          throw;
        }
      }

      return RedirectToPage("./Index");
    }

    private bool AnalysisExists(int id)
    {
      return _context.Analyses.Any(e => e.Id == id);
    }
  }
}

using FamilyTreeWebApp.Data;
using FamilyTreeWebTools.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FamilyTreeServices.Pages.IssueResults
{
  public class DeleteModel : PageModel
  {
    private readonly FamilyTreeDbContext _context;

    public DeleteModel(FamilyTreeDbContext context)
    {
      _context = context;
    }

    [BindProperty]
    public Issue Issue { get; set; }

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
      return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      Issue = await _context.Issues.FindAsync(id);

      if (Issue != null)
      {
        _context.Issues.Remove(Issue);
        await _context.SaveChangesAsync();
      }

      return RedirectToPage("./Index");
    }
  }
}

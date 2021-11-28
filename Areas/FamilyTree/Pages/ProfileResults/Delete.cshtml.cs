using Ekmansoft.FamilyTree.WebApp.Data;
using Ekmansoft.FamilyTree.WebTools.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FamilyTreeServices.Pages.ProfileResults
{
  public class DeleteModel : PageModel
  {
    private readonly FamilyTreeDbContext _context;

    public DeleteModel(FamilyTreeDbContext context)
    {
      _context = context;
    }

    [BindProperty]
    public Profile Profile { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      Profile = await _context.Profiles.FirstOrDefaultAsync(m => m.Id == id);

      if (Profile == null)
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

      Profile = await _context.Profiles.FindAsync(id);

      if (Profile != null)
      {
        _context.Profiles.Remove(Profile);
        await _context.SaveChangesAsync();
      }

      return RedirectToPage("./Index");
    }
  }
}

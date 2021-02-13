using FamilyTreeWebApp.Data;
using FamilyTreeWebTools.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FamilyTreeServices.Pages.ProfileResults
{
  public class DetailsModel : PageModel
  {
    private readonly FamilyTreeDbContext _context;

    public DetailsModel(FamilyTreeDbContext context)
    {
      _context = context;
    }

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
  }
}

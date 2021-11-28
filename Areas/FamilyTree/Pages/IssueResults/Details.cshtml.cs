using Ekmansoft.FamilyTree.WebApp.Data;
using Ekmansoft.FamilyTree.WebTools.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FamilyTreeServices.Pages.IssueResults
{
  public class DetailsModel : PageModel
  {
    private readonly FamilyTreeDbContext _context;

    public DetailsModel(FamilyTreeDbContext context)
    {
      _context = context;
    }

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
  }
}

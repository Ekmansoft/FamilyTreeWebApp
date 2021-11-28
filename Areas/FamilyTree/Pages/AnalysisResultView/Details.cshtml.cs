using Ekmansoft.FamilyTree.WebApp.Data;
using Ekmansoft.FamilyTree.WebTools.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FamilyTreeServices.Pages.AnalysisResultView
{
  [Authorize]
  public class DetailsModel : PageModel
  {
    private readonly FamilyTreeDbContext _context;

    public DetailsModel(FamilyTreeDbContext context)
    {
      _context = context;
    }

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
  }
}

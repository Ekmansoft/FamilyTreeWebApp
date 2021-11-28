using Ekmansoft.FamilyTree.WebApp.Data;
using Ekmansoft.FamilyTree.WebTools.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace FamilyTreeServices.Pages.AnalysisResultView
{
  [Authorize]
  public class CreateModel : PageModel
  {
    private readonly FamilyTreeDbContext _context;

    public CreateModel(FamilyTreeDbContext context)
    {
      _context = context;
    }

    public IActionResult OnGet()
    {
      return Page();
    }

    [BindProperty]
    public Analysis Analysis { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
      if (!ModelState.IsValid)
      {
        return Page();
      }

      _context.Analyses.Add(Analysis);
      await _context.SaveChangesAsync();

      return RedirectToPage("./Index");
    }
  }
}
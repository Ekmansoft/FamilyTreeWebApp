using FamilyTreeWebApp.Data;
using FamilyTreeWebTools.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace FamilyTreeServices.Pages.IssueResults
{
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
    public Issue Issue { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
      if (!ModelState.IsValid)
      {
        return Page();
      }

      _context.Issues.Add(Issue);
      await _context.SaveChangesAsync();

      return RedirectToPage("./Index");
    }
  }
}
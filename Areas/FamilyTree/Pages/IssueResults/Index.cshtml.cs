using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FamilyTreeWebTools.Data;
using FamilyTreeWebApp.Data;

namespace FamilyTreeServices.Pages.IssueResults
{
  public class IndexModel : PageModel
  {
    private readonly FamilyTreeDbContext _context;

    public IndexModel(FamilyTreeDbContext context)
    {
      _context = context;
    }

    public IList<Issue> Issues { get; set; }
    public Profile Profile1 { get; set; }

    public async Task OnGetAsync(int ProfileId)
    {
      Issues = await _context.Issues.Where(p => p.ProfileId == ProfileId).ToListAsync();
      Profile1 = await _context.Profiles.FindAsync(ProfileId);
    }
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AspNetCoreWebApp3.Pages
{
  [AllowAnonymous]
  public class JobModel : PageModel
  {
    public string Message { get; set; }

    public IActionResult OnGet()
    {
      Message = "Improve your family tree.";
      return Redirect("/FamilyTree/AnalysisResultView/Index");
    }
  }
}

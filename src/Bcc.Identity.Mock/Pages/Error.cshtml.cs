using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bcc.Identity.Mock.Pages;

public class ErrorModel : PageModel
{
    [BindProperty(SupportsGet = true, Name = "error")]
    public string? Error { get; set; }

    [BindProperty(SupportsGet = true, Name = "error_description")]
    public string? ErrorDescription { get; set; }

    public void OnGet()
    {
        Error ??= "unknown_error";
        ErrorDescription ??= "Det finnes ingen tilgjengelig feildetalj.";
    }
}

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bcc.Identity.Mock.Pages;

public class LogoutModel(LogoutContextStore logoutContextStore) : PageModel
{
    private const string DefaultReturnUrl = "https://localhost:15000";

    [BindProperty(SupportsGet = true)]
    public string? LogoutId { get; set; }

    public bool ShowPrompt { get; private set; }

    public string PostLogoutRedirectUri { get; private set; } = DefaultReturnUrl;

    public void OnGet()
    {
        LoadState();
        ShowPrompt = User.Identity?.IsAuthenticated == true;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        LoadState();

        if (User.Identity?.IsAuthenticated == true)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        ShowPrompt = false;
        return Page();
    }

    private void LoadState()
    {
        PostLogoutRedirectUri = logoutContextStore.GetPostLogoutRedirectUri(LogoutId) ?? DefaultReturnUrl;
    }
}

using System.Security.Claims;
using System.Text.Json;
using BccCode.Core.Api.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Bcc.Identity.Mock.Pages;

public class LoginModel(
    ICoreApiClient client,
    IOpenIddictApplicationManager appManager) : PageModel
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [BindProperty]
    public Guid? PersonUid { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public IReadOnlyList<Organization> Organizations { get; private set; } = [];

    public string OrganizationsJson { get; private set; } = "[]";

    public string InitialStateJson { get; private set; } = "{}";

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadAsync();

        if (PersonUid is null || PersonUid == Guid.Empty)
        {
            ModelState.AddModelError(nameof(PersonUid), "Velg en person.");
            return Page();
        }

        var person = Organizations
            .SelectMany(organization => organization.Persons)
            .FirstOrDefault(candidate => candidate.PersonUid == PersonUid.Value);

        if (person is null)
        {
            ModelState.AddModelError(nameof(PersonUid), "Den valgte personen finnes ikke.");
            return Page();
        }

        var claims = new List<Claim>
        {
            new(Claims.Subject, person.PersonUid.ToString()),
            new(Claims.Name, person.Name),
            new(Claims.Email, person.Email),
            new("https://login.bcc.no/claims/personUid", person.PersonUid.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var properties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            properties);

        return LocalRedirect(OidcRequestContext.GetLocalReturnUrl(ReturnUrl));
    }

    private async Task LoadAsync()
    {
        var organizations = await RoleEndpoint.GetOrganisationRoles(client) ?? [];

        Organizations = organizations
            .OrderBy(organization => organization.Name, StringComparer.OrdinalIgnoreCase)
            .Select(organization => new Organization
            {
                Name = organization.Name,
                Persons = organization.Persons
                    .OrderBy(person => person.Name, StringComparer.OrdinalIgnoreCase)
                    .ToArray()
            })
            .ToArray();

        OrganizationsJson = JsonSerializer.Serialize(Organizations, JsonOptions);

        var requestContext = OidcRequestContext.Parse(ReturnUrl);
        var selectedPerson = ResolveSelectedPerson(requestContext.LoginHint);
        var selectedOrganization = selectedPerson is null
            ? Organizations.FirstOrDefault()
            : Organizations.FirstOrDefault(organization =>
                organization.Persons.Any(person => person.PersonUid == selectedPerson.PersonUid));

        string? clientName = null;
        if (!string.IsNullOrWhiteSpace(requestContext.ClientId))
        {
            var application = await appManager.FindByClientIdAsync(requestContext.ClientId);
            if (application is not null)
                clientName = await appManager.GetDisplayNameAsync(application);
        }

        InitialStateJson = JsonSerializer.Serialize(new
        {
            client = clientName ?? requestContext.ClientId,
            personUid = selectedPerson?.PersonUid,
            organizationName = selectedOrganization?.Name
        }, JsonOptions);
    }

    private Person? ResolveSelectedPerson(string? loginHint)
    {
        if (PersonUid is not null && PersonUid != Guid.Empty)
        {
            return Organizations
                .SelectMany(organization => organization.Persons)
                .FirstOrDefault(person => person.PersonUid == PersonUid.Value);
        }

        if (!string.IsNullOrWhiteSpace(loginHint))
        {
            return Organizations
                .SelectMany(organization => organization.Persons)
                .FirstOrDefault(person => string.Equals(person.Email, loginHint, StringComparison.OrdinalIgnoreCase));
        }

        return Organizations.FirstOrDefault()?.Persons.FirstOrDefault();
    }
}

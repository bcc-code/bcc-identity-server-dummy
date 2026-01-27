using Bcc.Identity.Mock;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();

builder.Services.AddAuthorization();

builder.Services.AddIdentityServer(options =>
{
    options.Authentication.CookieSameSiteMode = SameSiteMode.Lax;
    options.Authentication.CheckSessionCookieSameSiteMode = SameSiteMode.Lax;

    options.UserInteraction.LoginUrl = "/login.html";
    options.UserInteraction.LogoutUrl = "/logout.html";
    options.UserInteraction.ErrorUrl = "/error.html";

    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;

    options.EmitStaticAudienceClaim = true;
})
.AddInMemoryIdentityResources(Config.IdentityResources(builder.Configuration))
.AddInMemoryApiScopes(Config.ApiScopes(builder.Configuration))
.AddInMemoryClients(Config.Clients(builder.Configuration))
.AddProfileService<JustAddAllClaimsProfileService>()
.AddServerSideSessions();

var app = builder.Build();

app.UseDeveloperExceptionPage();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseIdentityServer();
app.UseAuthorization();

app.MapOidcEndpoints();
app.MapUserRoleEndpoint();

app.Run();

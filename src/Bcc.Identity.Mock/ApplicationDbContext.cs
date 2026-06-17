using Microsoft.EntityFrameworkCore;

namespace Bcc.Identity.Mock;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder) => builder.UseOpenIddict();
}

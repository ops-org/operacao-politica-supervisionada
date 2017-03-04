using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using OPS.Entities;
using OPS.Providers;

namespace OPS
{
    // You can add profile data for the user by adding more properties to your AppUser
    // class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; }

        //public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<AppUser> manager)
        //{
        //    // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
        //    var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
        //    // Add custom user claims here
        //    return userIdentity;
        //}
    }

    public class AuthContext : IdentityDbContext<IdentityUser>
    {
        //static AuthContext()
        //{
        //    Database.SetInitializer(new MySqlInitializer());
        //}

        public AuthContext()
            : base("AuditoriaContext", false)
        {
            this.Configuration.LazyLoadingEnabled = false;
        }

        //public static ApplicationDbContext Create()
        //{
        //    return new ApplicationDbContext();
        //}

        public DbSet<Client> Clients { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }

}
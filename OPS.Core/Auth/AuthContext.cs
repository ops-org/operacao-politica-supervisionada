using System;
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.Threading.Tasks;
using OPS.Core.Auth.Entities;

namespace OPS
{
	// You can add profile data for the user by adding more properties to your ApplicationUser
	// class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
	public class ApplicationUser : IdentityUser
	{
		public string FullName { get; set; }
		//public string Image { get; set; }
		//public string ProfileLinkGoogle { get; set; }
		//public string ProfileLinkFacebook { get; set; }

		public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType = DefaultAuthenticationTypes.ApplicationCookie)
		{
			// Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
			var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
			// Add custom user claims here
			return userIdentity;
		}
	}

	//public class ApplicationUserManager : UserManager<ApplicationUser>
	//{
	//	public ApplicationUserManager(IUserStore<ApplicationUser> store)
	//	: base(store) { }

	//	public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUser> options, IOwinContext context)
	//	{
	//		var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<AuthContext>()));
	//		return manager;
	//	}

	//	public ApplicationUserManager(IUserStore<ApplicationUser> store, IDataProtectionProvider dataProtectionProvider)
	//		: base(store)
	//	{
	//		// Configure validation logic for usernames
	//		this.UserValidator = new UserValidator<ApplicationUser>(this)
	//		{
	//			AllowOnlyAlphanumericUserNames = false,
	//			RequireUniqueEmail = true
	//		};

	//		// Configure validation logic for passwords
	//		this.PasswordValidator = new PasswordValidator
	//		{
	//			RequiredLength = 6,
	//			RequireNonLetterOrDigit = false,
	//			RequireDigit = false,
	//			RequireLowercase = false,
	//			RequireUppercase = false,
	//		};

	//		// Configure user lockout defaults
	//		this.UserLockoutEnabledByDefault = true;
	//		this.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
	//		this.MaxFailedAccessAttemptsBeforeLockout = 5;

	//		// Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
	//		// You can write your own provider and plug it in here.
	//		//this.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
	//		//{
	//		//    MessageFormat = "Your security code is {0}"
	//		//});
	//		////this.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
	//		////{
	//		////    Subject = "Security Code",
	//		////    BodyFormat = "Your security code is {0}"
	//		////});


	//		//this.EmailService = new EmailService();
	//		//this.SmsService = new SmsService();

	//		//this.UserTokenProvider = new TotpSecurityStampBasedTokenProvider<ApplicationUser, string>();
	//		//// var dataProtectionProvider = Startup.DataProtectionProvider;
	//		////var dataProtectionProvider = options.DataProtectionProvider;
	//		//if (dataProtectionProvider != null)
	//		//{
	//		//	IDataProtector dataProtector = dataProtectionProvider.Create("ASP.NET Identity");

	//		//	this.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(dataProtector);
	//		//}
	//	}
	//}

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

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<IdentityUser>()
				.ToTable("users");

			modelBuilder.Entity<IdentityRole>()
				.ToTable("roles");

			modelBuilder.Entity<IdentityUserRole>()
				.ToTable("user_roles");

			modelBuilder.Entity<IdentityUserClaim>()
				.ToTable("user_claims");

			modelBuilder.Entity<IdentityUserLogin>()
				.ToTable("user_logins");

			modelBuilder.Entity<Client>()
				   .ToTable("clients");

			modelBuilder.Entity<RefreshToken>()
				.ToTable("refresh_tokens");
		}

		public DbSet<Client> Clients { get; set; }
		public DbSet<RefreshToken> RefreshTokens { get; set; }
	}

}
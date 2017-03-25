using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.DataProtection;
using OPS.Core;
using OPS.Entities;
using OPS.Models;
using OPS.WebApi;

namespace OPS
{

	public class AuthRepository : IDisposable
	{
		private AuthContext _ctx;

		private UserManager<ApplicationUser> _userManager;

		public AuthRepository()
		{
			_ctx = new AuthContext();

			_userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_ctx));
			_userManager.UserTokenProvider = new TotpSecurityStampBasedTokenProvider<ApplicationUser, string>();
		}

		public async Task<IdentityResult> RegisterUser(UserModel userModel, string sBaseUrl)
		{
			ApplicationUser user = new ApplicationUser
			{
				Email = userModel.Email,
				UserName = userModel.Email,
				FullName = userModel.FullName
			};

			var result = await _userManager.CreateAsync(user, userModel.Password);
			if (result.Succeeded)
			{
				var token = await _userManager.GenerateEmailConfirmationTokenAsync(user.Id);
				var callbackUrl = string.Format("{0}#/verify-email?id={1}&token={2}", sBaseUrl, user.Id, HttpUtility.UrlEncode(token));
				var subject = "OPS :: Confirmação seu e-mail";
				var body = string.Format(@"Confirme seu e-mail clicando aqui: <a href=""{0}"">{0}</a>", callbackUrl);
				await Utils.SendMailAsync(new MailAddress(user.Email), subject, body);
			}

			return result;
		}

		public async Task<ApplicationUser> FindUser(string userName, string password)
		{
			return await _userManager.FindAsync(userName, password);
		}

		public Client FindClient(string clientId)
		{
			var client = _ctx.Clients.Find(clientId);

			return client;
		}

		public async Task<bool> AddRefreshToken(RefreshToken token)
		{
			var existingToken = _ctx.RefreshTokens.Where(r => r.Subject == token.Subject && r.ClientId == token.ClientId).SingleOrDefault();

			if (existingToken != null)
			{
				await RemoveRefreshToken(existingToken);
			}

			_ctx.RefreshTokens.Add(token);

			return await _ctx.SaveChangesAsync() > 0;
		}

		public async Task<bool> RemoveRefreshToken(string refreshTokenId)
		{
			var refreshToken = await _ctx.RefreshTokens.FindAsync(refreshTokenId);

			if (refreshToken != null)
			{
				_ctx.RefreshTokens.Remove(refreshToken);
				return await _ctx.SaveChangesAsync() > 0;
			}

			return false;
		}

		public async Task<bool> RemoveRefreshToken(RefreshToken refreshToken)
		{
			_ctx.RefreshTokens.Remove(refreshToken);
			return await _ctx.SaveChangesAsync() > 0;
		}

		public async Task<RefreshToken> FindRefreshToken(string refreshTokenId)
		{
			var refreshToken = await _ctx.RefreshTokens.FindAsync(refreshTokenId);

			return refreshToken;
		}

		public List<RefreshToken> GetAllRefreshTokens()
		{
			return _ctx.RefreshTokens.ToList();
		}

		public async Task<ApplicationUser> FindAsync(UserLoginInfo loginInfo)
		{
			return await _userManager.FindAsync(loginInfo);
		}

		public async Task<IdentityResult> CreateAsync(ApplicationUser user, string sBaseUrl)
		{
			var result = await _userManager.CreateAsync(user);
			if (result.Succeeded)
			{
				var token = await _userManager.GenerateEmailConfirmationTokenAsync(user.Id);
				var callbackUrl = string.Format("{0}#/verify-email?id={1}&token={2}", sBaseUrl, user.Id, HttpUtility.UrlEncode(token));
				var subject = "OPS :: Confirmação seu e-mail";
				var body = string.Format(@"Confirme seu e-mail clicando aqui: <a href=""{0}"">{0}</a>", callbackUrl);

				await Utils.SendMailAsync(new MailAddress(user.Email), subject, body);
			}
			else if (result.Errors.ToArray()[0].Contains(user.UserName))
			{
				// Usuário já possui outro tipo de acesso.
				var dbUser = await _userManager.FindByNameAsync(user.UserName);

				dbUser.FullName = user.FullName;
				result = await _userManager.UpdateAsync(dbUser);

				user.Id = dbUser.Id;
			}

			return result;
		}

		public async Task<IdentityResult> AddLoginAsync(string userId, UserLoginInfo login)
		{
			return await _userManager.AddLoginAsync(userId, login);
		}

		internal async Task ResetPassword(ApplicationUser user, string sBaseUrl)
		{
			await _userManager.UpdateSecurityStampAsync(user.Id);
			var token = await _userManager.GeneratePasswordResetTokenAsync(user.Id);

			var callbackUrl = string.Format("{0}#/set-password?id={1}&token={2}", sBaseUrl, user.Id, HttpUtility.UrlEncode(token));
			var subject = "OPS :: Redefinição de senha";
			var body = string.Format(@"Redefina sua senha clicando aqui: <a href=""{0}"">{0}</a>", callbackUrl);
			await Utils.SendMailAsync(new MailAddress(user.Email), subject, body);
		}

		public async Task<IdentityResult> SetPassword(PasswordRecoverModel user)
		{
			return await _userManager.ResetPasswordAsync(user.UserId, user.Token, user.NewPassword);
		}

		public async Task<ApplicationUser> FindByEmailAsync(string userName)
		{
			return await _userManager.FindByNameAsync(userName);
		}

		internal async Task<IdentityResult> VerifyEmail(VerifyEmailModel user)
		{
			return await _userManager.ConfirmEmailAsync(user.UserId, user.Token);
		}

		public async Task<IList<string>> GetRolesAsync(string userId)
		{
			return await _userManager.GetRolesAsync(userId);
		}

		public async Task<bool> IsInRoleAsync(string userId, string role)
		{
			return await _userManager.IsInRoleAsync(userId, role);
		}

		public void Dispose()
		{
			_ctx.Dispose();
			_userManager.Dispose();

		}
	}
}
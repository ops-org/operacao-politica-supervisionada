using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.DataProtection;
using OPS.Core;
using OPS.Entities;
using OPS.Models;

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
        }

        public async Task<IdentityResult> RegisterUser(UserModel userModel)
        {
            ApplicationUser user = new ApplicationUser
            {
                Email = userModel.Email,
                UserName = userModel.Email,
                FullName = userModel.Name
            };

            var result = await _userManager.CreateAsync(user, userModel.Password);

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

        public async Task<IdentityResult> CreateAsync(ApplicationUser user)
        {
            return await _userManager.CreateAsync(user);
        }

        public async Task<IdentityResult> AddLoginAsync(string userId, UserLoginInfo login)
        {
            return await _userManager.AddLoginAsync(userId, login);
        }

        public void Dispose()
        {
            _ctx.Dispose();
            _userManager.Dispose();

        }

        internal async Task ResetPassword(ApplicationUser user, string sBaseUrl)
        {
            var provider = new DpapiDataProtectionProvider("OPS");
            _userManager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser, string>(provider.Create("ASP.NET Identity"));

            var code = await _userManager.GeneratePasswordResetTokenAsync(user.Id);
            var callbackUrl = string.Format("{0}#/ResetPassword/{1}/{2}", sBaseUrl, user.Id, code);


            await _userManager.SendEmailAsync(user.Id, "OPS :: Redefinição de senha",
               string.Format(@"Redefina sua senha clicando aqui: <a href=""{0}"">{0}</a>", callbackUrl));
        }

        public async Task SetPassword(PasswordRecoverModel user)
        {
            await _userManager.ResetPasswordAsync(user.UserId, user.Token, user.NewPassword);
        }

        public async Task<ApplicationUser> FindByEmailAsync(string userName)
        {
            return await _userManager.FindByNameAsync(userName);
        }

        internal async Task<IdentityResult> VerifyEmail(VerifyEmailModel user)
        {
            return await _userManager.ConfirmEmailAsync(user.UserId, user.Token);
        }
    }
}
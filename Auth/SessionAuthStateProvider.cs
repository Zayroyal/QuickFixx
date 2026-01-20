using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using BlazorApp2.Services;

namespace BlazorApp2.Auth
{
    public class SessionAuthStateProvider : AuthenticationStateProvider
    {
        private const string Key = "blazorapp2_user_id";

        private readonly ProtectedSessionStorage _session;
        private readonly AuthService _auth;

        public SessionAuthStateProvider(ProtectedSessionStorage session, AuthService auth)
        {
            _session = session;
            _auth = auth;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var principal = new ClaimsPrincipal(new ClaimsIdentity());

            var stored = await _session.GetAsync<int>(Key);

            if (stored.Success && stored.Value > 0)
            {
                var user = await _auth.GetByIdAsync(stored.Value);

                if (user != null)
                {
                    var identity = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.Email),
                    }, authenticationType: "BlazorApp2Session");

                    principal = new ClaimsPrincipal(identity);
                }
            }

            return new AuthenticationState(principal);
        }

        public async Task SignInAsync(int userId)
        {
            await _session.SetAsync(Key, userId);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task SignOutAsync()
        {
            await _session.DeleteAsync(Key);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}

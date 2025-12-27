using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Hospital.Web.Client.Auth;

public class ApiAuthStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _http;
    private Task<AuthenticationState>? _stateTask;

    public ApiAuthStateProvider(HttpClient http)
    {
        _http = http;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
        => _stateTask ??= FetchAsync();

    public void Refresh()
    {
        _stateTask = FetchAsync();
        NotifyAuthenticationStateChanged(_stateTask);
    }

    private async Task<AuthenticationState> FetchAsync()
    {
        try
        {
            var me = await _http.GetFromJsonAsync<MeResponse>("/api/auth/me");

            if (me != null && me.isAuthenticated && !string.IsNullOrWhiteSpace(me.email))
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, me.email)
                };

                if (!string.IsNullOrWhiteSpace(me.fullName))
                    claims.Add(new Claim("fullName", me.fullName));

                if (me.roles != null)
                {
                    foreach (var r in me.roles)
                        claims.Add(new Claim(ClaimTypes.Role, r));
                }

                var identity = new ClaimsIdentity(claims, "Cookies");
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
        }
        catch
        {
        }

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    private sealed class MeResponse
    {
        public bool isAuthenticated { get; set; }
        public string? email { get; set; }
        public string[]? roles { get; set; }
        public string? fullName { get; set; }
    }
}

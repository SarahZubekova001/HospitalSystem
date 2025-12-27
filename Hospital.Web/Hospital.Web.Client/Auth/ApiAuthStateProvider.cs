using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
namespace Hospital.Web.Client.Auth;


public class ApiAuthStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _http;
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());

    public ApiAuthStateProvider(HttpClient http) => _http = http;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var me = await _http.GetFromJsonAsync<MeResponse>("api/auth/me");
            if (me is null || !me.isAuthenticated)
                return new AuthenticationState(Anonymous);

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, me.email ?? "")
            };

            if (!string.IsNullOrWhiteSpace(me.fullName))
                claims.Add(new("fullName", me.fullName));

            if (me.roles is not null)
                claims.AddRange(me.roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var identity = new ClaimsIdentity(claims, "Cookies");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            return new AuthenticationState(Anonymous);
        }
    }

    public void Refresh() => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    public record MeResponse(bool isAuthenticated, string? email, string[]? roles, string? fullName);
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Umbraco.Community.EncryptionPropertyEditor.Interfaces;
using Umbraco.Extensions;

namespace Umbraco.Community.EncryptionPropertyEditor.Services;
public class BackofficeUserAccessor : IBackofficeUserAccessor
{
    private readonly IOptionsSnapshot<CookieAuthenticationOptions> _cookieOptionsSnapshot;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BackofficeUserAccessor(
        IOptionsSnapshot<CookieAuthenticationOptions> cookieOptionsSnapshot,
        IHttpContextAccessor httpContextAccessor)
    {
        _cookieOptionsSnapshot = cookieOptionsSnapshot;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Updated to use ChunkingCookieManager as per Sean Maloney's answer on our.umbraco.com
    /// https://our.umbraco.com/forum/umbraco-9/106857-how-do-i-determine-if-a-backoffice-user-is-logged-in-from-a-razor-view#comment-341847
    /// </summary>
    public ClaimsIdentity BackofficeUser
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null)
                return new ClaimsIdentity();

            var cookieOptions = _cookieOptionsSnapshot.Get(Cms.Core.Constants.Security.BackOfficeAuthenticationType);
            var cookieManager = new ChunkingCookieManager();
            var backOfficeCookie = cookieManager.GetRequestCookie(httpContext, cookieOptions.Cookie.Name!);

            if (string.IsNullOrEmpty(backOfficeCookie))
                return new ClaimsIdentity();

            var unprotected = cookieOptions.TicketDataFormat.Unprotect(backOfficeCookie!);
            var backOfficeIdentity = unprotected?.Principal.GetUmbracoIdentity();

            return backOfficeIdentity ?? new ClaimsIdentity();
        }
    }
}

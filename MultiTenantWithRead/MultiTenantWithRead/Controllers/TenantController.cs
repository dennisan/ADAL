using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MultiTenantWithRead.Utils;

namespace MultiTenantWithRead.Controllers
{
    public class TenantController : Controller
    {
        private static readonly string ClientId = ConfigurationManager.AppSettings["ida:ClientID"];
        private const string ConsentUrlFormat = "https://account.activedirectory.windowsazure.com/Consent.aspx?ClientId={0}";

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SignUp()
        {
            string signupToken = Guid.NewGuid().ToString();
            string replyUrl = Url.Action("SignUpCallback", "Tenant", routeValues: new { signupToken = signupToken }, protocol: Request.Url.Scheme);
            DatabaseIssuerNameRegistry.CleanUpExpiredSignupTokens();
            DatabaseIssuerNameRegistry.AddSignupToken(signupToken: signupToken, expirationTime: DateTimeOffset.UtcNow.AddMinutes(5));

            // Redirect to the Active Directory consent page asking for permissions.
            return new RedirectResult(CreateConsentUrl(
                clientId: ClientId,
                requestedPermissions: "DirectoryReaders",
                consentReturnUrl: replyUrl));
        }

        public ActionResult SignUpCallback()
        {
            string tenantId = Request.QueryString["TenantId"];
            string signupToken = Request.QueryString["signupToken"];
            if (DatabaseIssuerNameRegistry.ContainsTenant(tenantId))
            {
                // The tenant is already registered, show the completion page.
                return View();
            }

            string consent = Request.QueryString["Consent"];
            if (!String.IsNullOrEmpty(tenantId) &&
                String.Equals(consent, "Granted", StringComparison.OrdinalIgnoreCase))
            {
                // Register the tenant when the permission is granted.
                if (DatabaseIssuerNameRegistry.TryAddTenant(tenantId, signupToken))
                {
                    return View();
                }
            }

            return View("Error");
        }

        private string CreateConsentUrl(string clientId, string requestedPermissions,
                                          string consentReturnUrl)
        {
            string consentUrl = String.Format(
                CultureInfo.InvariantCulture,
                ConsentUrlFormat,
                HttpUtility.UrlEncode(clientId));

            if (!String.IsNullOrEmpty(requestedPermissions))
            {
                consentUrl += "&RequestedPermissions=" + HttpUtility.UrlEncode(requestedPermissions);
            }
            if (!String.IsNullOrEmpty(consentReturnUrl))
            {
                consentUrl += "&ConsentReturnURL=" + HttpUtility.UrlEncode(consentReturnUrl);
            }

            return consentUrl;
        }
    }
}
using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Security.ActiveDirectory;
using System.Configuration;

namespace TodoSPA
{
	public partial class Startup
	{
		public void ConfigureAuth(IAppBuilder app)
		{
			app.UseWindowsAzureActiveDirectoryBearerAuthentication(
				new WindowsAzureActiveDirectoryBearerAuthenticationOptions
				{
					Audience = ConfigurationManager.AppSettings["ida:Audience"],
					Tenant = ConfigurationManager.AppSettings["ida:Tenant"],

					// TokenValidationParameters below to prevent validation of Issuer in order to support mutli-tenants
					TokenValidationParameters = new System.IdentityModel.Tokens.TokenValidationParameters() { ValidateIssuer = false }
				});
		}

	}
}

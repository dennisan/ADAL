using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Hosting;
using System.Xml.Linq;
using MultiTenantWithRead.Models;

namespace MultiTenantWithRead.Utils
{
    public class DatabaseIssuerNameRegistry : ValidatingIssuerNameRegistry
    {
        public static bool ContainsTenant(string tenantId)
        {
            using (TenantDbContext context = new TenantDbContext())
            {
                return context.Tenants
                    .Where(tenant => tenant.Id == tenantId)
                    .Any();
            }
        }

        public static bool ContainsKey(string thumbprint)
        {
            using (TenantDbContext context = new TenantDbContext())
            {
                return context.IssuingAuthorityKeys
                    .Where(key => key.Id == thumbprint)
                    .Any();
            }
        }

        public static void RefreshKeys(string metadataLocation)
        {
            IssuingAuthority issuingAuthority = ValidatingIssuerNameRegistry.GetIssuingAuthority(metadataLocation);

            bool newKeys = false;
            bool refreshTenant = false;
            foreach (string thumbprint in issuingAuthority.Thumbprints)
            {
                if (!ContainsKey(thumbprint))
                {
                    newKeys = true;
                    refreshTenant = true;
                    break;
                }
            }

            foreach (string issuer in issuingAuthority.Issuers)
            {
                if (!ContainsTenant(GetIssuerId(issuer)))
                {
                    refreshTenant = true;
                    break;
                }
            }

            if (newKeys || refreshTenant)
            {
                using (TenantDbContext context = new TenantDbContext())
                {
                    if (newKeys)
                    {
                        context.IssuingAuthorityKeys.RemoveRange(context.IssuingAuthorityKeys);
                        foreach (string thumbprint in issuingAuthority.Thumbprints)
                        {
                            context.IssuingAuthorityKeys.Add(new IssuingAuthorityKey { Id = thumbprint });
                        }
                    }

                    if (refreshTenant)
                    {
                        // Add the default tenant to the registry. 
                        // Comment or remove the following code if you do not wish to have the default tenant use the application.
                        foreach (string issuer in issuingAuthority.Issuers)
                        {
                            string issuerId = GetIssuerId(issuer);
                            if (!ContainsTenant(issuerId))
                            {
                                context.Tenants.Add(new Tenant { Id = issuerId });
                            }
                        }
                    }
                    context.SaveChanges();
                }
            }
        }

        public static bool TryAddTenant(string tenantId, string signupToken)
        {
            if (!ContainsTenant(tenantId))
            {
                using (TenantDbContext context = new TenantDbContext())
                {
                    SignupToken existingToken = context.SignupTokens.Where(token => token.Id == signupToken).FirstOrDefault();
                    if (existingToken != null)
                    {
                        context.SignupTokens.Remove(existingToken);
                        context.Tenants.Add(new Tenant { Id = tenantId });
                        context.SaveChanges();
                        return true;
                    }
                }
            }

            return false;
        }

        public static void AddSignupToken(string signupToken, DateTimeOffset expirationTime)
        {
            using (TenantDbContext context = new TenantDbContext())
            {
                context.SignupTokens.Add(new SignupToken
                {
                    Id = signupToken,
                    ExpirationDate = expirationTime
                });
                context.SaveChanges();
            }
        }

        public static void CleanUpExpiredSignupTokens()
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            using (TenantDbContext context = new TenantDbContext())
            {
                IQueryable<SignupToken> tokensToRemove = context.SignupTokens.Where(token => token.ExpirationDate <= now);
                if (tokensToRemove.Any())
                {
                    context.SignupTokens.RemoveRange(tokensToRemove);
                    context.SaveChanges();
                }
            }
        }

        private static string GetIssuerId(string issuer)
        {
            return issuer.TrimEnd('/').Split('/').Last();
        }

        protected override bool IsThumbprintValid(string thumbprint, string issuer)
        {
            return ContainsTenant(GetIssuerId(issuer)) &&
                ContainsKey(thumbprint);
        }
    }
}
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Hosting;
using System.Xml.Linq;
using MultiTenantSignIn.Models;

namespace MultiTenantSignIn.Utils
{
    public class MemoryIssuerNameRegistry : ValidatingIssuerNameRegistry
    {
        
        public static bool ContainsTenant(string tenantId)
        {
            using (var context = MemoryContext.GetContext())
            {
                return context.Tenants.Any(tenant => tenant.Id == tenantId);
            }
        }

        public static bool ContainsKey(string thumbprint)
        {
            using (var context = MemoryContext.GetContext())
            {
                return context.IssuingAuthorityKeys.Any(key => key.Id == thumbprint);
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
                using (var context = MemoryContext.GetContext())
                {
                    if (newKeys)
                    {
                        //context.IssuingAuthorityKeys.RemoveRange(context.IssuingAuthorityKeys);
                        context.IssuingAuthorityKeys.RemoveAll(key => key.Id.Length > 0);
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
                using (var context = MemoryContext.GetContext())
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
            using (var context = MemoryContext.GetContext())
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
            using (var context = MemoryContext.GetContext())
            {
                IEnumerable<SignupToken> tokensToRemove = context.SignupTokens.Where(token => token.ExpirationDate <= now);
                if (tokensToRemove.Any())
                {
                    //context.SignupTokens.RemoveRange(tokensToRemove);
                    context.SignupTokens.RemoveAll(token => token.ExpirationDate <= now);
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
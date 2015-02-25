using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Hosting;
using System.Xml.Linq;
using SingleTenantSignIn.Models;

namespace SingleTenantSignIn.Utils
{
    public class MemoryIssuerNameRegistry : ValidatingIssuerNameRegistry
    {
        public class TenantData
        {
            private readonly List<IssuingAuthorityKey> issuingAuthorityKeys = new List<IssuingAuthorityKey>();
            private readonly List<Tenant> tenants = new List<Tenant>();

            public List<IssuingAuthorityKey> IssuingAuthorityKeys
            {
                get { return issuingAuthorityKeys; }
            }

            public List<Tenant> Tenants
            {
                get { return tenants; }
            }
        }
        private static readonly TenantData Data= new TenantData();
        
        public static bool ContainsTenant(string tenantId)
        {
            return Data.Tenants.Any(tenant => tenant.Id == tenantId);
        }

        public static bool ContainsKey(string thumbprint)
        {
            return Data.IssuingAuthorityKeys.Any(key => key.Id == thumbprint);
        }

        public static void RefreshKeys(string metadataLocation)
        {
            var issuingAuthority = GetIssuingAuthority(metadataLocation);

            var newKeys = false;
            var refreshTenant = false;

            if (issuingAuthority.Thumbprints.Any(thumbprint => !ContainsKey(thumbprint)))
            {
                newKeys = true;
                refreshTenant = true;
            }

            if (issuingAuthority.Issuers.Any(issuer => !ContainsTenant(GetIssuerId(issuer))))
            {
                refreshTenant = true;
            }

            if (newKeys || refreshTenant)
            {
                if (newKeys)
                {
                    Data.IssuingAuthorityKeys.Clear();
                    foreach (string thumbprint in issuingAuthority.Thumbprints)
                    {
                        Data.IssuingAuthorityKeys.Add(new IssuingAuthorityKey { Id = thumbprint });
                    }
                }

                if (refreshTenant)
                {
                    foreach (string issuer in issuingAuthority.Issuers)
                    {
                        string issuerId = GetIssuerId(issuer);
                        if (!ContainsTenant(issuerId))
                        {
                            Data.Tenants.Add(new Tenant { Id = issuerId });
                        }
                    }
                }
            }
        }

        private static string GetIssuerId(string issuer)
        {
            return issuer.TrimEnd('/').Split('/').Last();
        }

        protected override bool IsThumbprintValid(string thumbprint, string issuer)
        {
            return ContainsTenant(GetIssuerId(issuer))
                && ContainsKey(thumbprint);
        }
    }
}

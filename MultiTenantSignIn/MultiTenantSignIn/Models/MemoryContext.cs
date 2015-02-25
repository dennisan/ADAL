using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Services.Client;

namespace MultiTenantSignIn.Models
{
    public class MemoryContext : IDisposable
    {
        private static MemoryContext _context;
        readonly List<Tenant> _tenants ;
        readonly List<IssuingAuthorityKey> _issuingAuthorityKey ;
        readonly List<SignupToken> _signupToken ;

        private MemoryContext()
        {
            _tenants = new List<Tenant>();
            _issuingAuthorityKey = new List<IssuingAuthorityKey>();
            _signupToken = new List<SignupToken>();
        }
        
        public static MemoryContext GetContext()
        {
            return _context ?? (_context = new MemoryContext());
        }

        public List<Tenant> Tenants { get {return _tenants;} }

        public List<IssuingAuthorityKey> IssuingAuthorityKeys { get {return _issuingAuthorityKey;} }

        public List<SignupToken> SignupTokens { get{return _signupToken;}}

        public int SaveChanges()
        {
            return 0; //throw new NotImplementedException();
        }

        public void Dispose()
        {
            ; //throw new NotImplementedException();
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SingleTenantSignIn.Models
{
    public class IssuingAuthorityKey
    {
        public string Id { get; set; }
    }

    public class Tenant
    {
        public string Id { get; set; }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SingleTenantSignIn.Models
{
    public class UserProfile
    {
        public string DisplayName { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
    }
}

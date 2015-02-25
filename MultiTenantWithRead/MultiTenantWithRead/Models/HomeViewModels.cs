﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiTenantWithRead.Models
{
	public class UserProfile
	{
		public string DisplayName { get; set; }
		public string GivenName { get; set; }
		public string Surname { get; set; }

		public string StreetAddress { get; set; }
		public string City { get; set; }
		public string State { get; set; }
		public string PostalCode { get; set; }
		public string Country { get; set; }

		public string JobTitle { get; set; }
		public string Department { get; set; }

		public string Mail { get; set; }
		public string Mobile { get; set; }
		public string TelephoneNumber { get; set; }

		public string UserPrincipalName { get; set; }
	}
}

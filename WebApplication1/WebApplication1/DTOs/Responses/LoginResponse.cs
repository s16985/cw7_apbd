using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.DTOs.Responses
{
	public class LoginResponse
	{
		public string Password { get; set; }
		public string Salt { get; set; }
		public string Message { get; set; }
	}
}

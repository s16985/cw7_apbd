using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.DTOs.Requests
{
	public class SaveRefreshTokenRequest
	{
		public string refreshToken { get; set; }
		public string indexNumber { get; set; }
	}
}

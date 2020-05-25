using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.DTOs.Requests
{
	public class PromoteStudentsRequest
	{
		public string Studies { get; set; }
		public int Semester { get; set; }
	}
}

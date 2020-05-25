using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.DAL
{
	public class MockDbService : IDbService
	{
		private static IEnumerable<Student> _students;

		static MockDbService()
		{
			_students = new List<Student>
			{
				new Student { IndexNumber = "1", FirstName = "Jan", LastName = "Kowalski" },
				new Student { IndexNumber = "2", FirstName = "Anna", LastName = "Malewski" },
				new Student { IndexNumber = "3", FirstName = "Andrzej", LastName = "Andrzejewski" }
			};
		}

		public IEnumerable<Student> GetStudents()
		{
			return _students;
		}
	}
}

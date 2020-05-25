using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.DTOs.Requests;
using WebApplication1.DTOs.Responses;

namespace WebApplication1.Services
{
	public interface IStudentsDbService
	{
		EnrollStudentResponse EnrollStudent(EnrollStudentRequest request);
		PromoteStudentsResponse PromoteStudents(PromoteStudentsRequest request);
		bool CheckIndex(string indexNumber);
		LoginResponse LoginStudentResponse(LoginRequestDto request);
		SaveRefreshTokenResponse SaveRefreshToken(SaveRefreshTokenRequest request);
		RefreshTokenResponse RefreshToken(RefreshTokenRequest request);
	}
}

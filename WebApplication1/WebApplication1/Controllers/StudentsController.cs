using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.DAL;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using WebApplication1.DTOs.Requests;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using WebApplication1.Services;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/students")]
    
    public class StudentsController : ControllerBase
    {
        private IStudentsDbService _service;

        private IConfiguration Configuration { get; set; }

        public StudentsController(IConfiguration configuration, IStudentsDbService service)
        {
            Configuration = configuration;
            _service = service;
        }

        [HttpGet]
        public IActionResult GetStudents()
        {
            List<Student> _students = new List<Student>();

            using (var con = new SqlConnection("Data Source=db-mssql;Initial Catalog=s16985;Integrated Security=True"))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                com.CommandText = "select st.FirstName, st.Lastname, st.BirthDate, sd.Name, e.Semester from Student st join Enrollment e on st.IdEnrollment=e.IdEnrollment join Studies sd on e.IdStudy=sd.IdStudy;";

                

                con.Open();
                var dr = com.ExecuteReader();
                while (dr.Read())
                {
                    var st = new Student();
                    st.FirstName = dr["FirstName"].ToString();
                    st.LastName = dr["LastName"].ToString();
                    st.BirthDate = DateTime.Parse(dr["BirthDate"].ToString());
                    st.StudiesName = dr["Name"].ToString();
                    st.Semester = int.Parse(dr["Semester"].ToString());

                    _students.Add(st);
                }
            }

            return Ok(_students);
        }
        public string GetStudent()
        {
            return "Kowalski, Malewski, Andrzejewski";
        }

        [HttpGet("{indexNumber}")]
        public IActionResult GetStudent(string indexNumber) // w celu usunięcia tabeli Student przez atak SQLInjection należy podać jako indexNumber ';%20DROP%20TABLE%20Student;--
        {
            var enrollment = new Enrollment();

            using (var con = new SqlConnection("Data Source=db-mssql;Initial Catalog=s16985;Integrated Security=True"))
            using (var com = new SqlCommand())
            {
                com.Connection = con;             
                com.CommandText = "select e.* from Student st join Enrollment e on st.IdEnrollment=e.IdEnrollment join Studies sd on e.IdStudy=sd.IdStudy where st.indexNumber=@indexNumber;";
                com.Parameters.AddWithValue("indexNumber", indexNumber);

                con.Open();
                var dr = com.ExecuteReader();
                while (dr.Read())
                {
                    enrollment.IdEnrollment = int.Parse(dr["IdEnrollment"].ToString());
                    enrollment.Semester = int.Parse(dr["Semester"].ToString());
                    enrollment.IdStudy = int.Parse(dr["IdStudy"].ToString());
                    enrollment.StartDate = DateTime.Parse(dr["StartDate"].ToString());
                }
            }

            return Ok(enrollment);
        }


        [HttpPost]
        public IActionResult CreateStudent(Student student)
        {
     //       student.IndexNumber = $"s{new Random().Next(1, 20000)}";
            return Ok(student);
        }

        [HttpPut]
        public IActionResult UpDateStudent(int id)
        {

            return Ok("Aktualizacja dokończona");
        }

        [HttpDelete]
        public IActionResult DeleteStudent(int id)
        {
            return Ok("Usuwanie ukończone");
        }

        [HttpPost("login")]
        [AllowAnonymous]
        // dla każdego loginu (IndexNumber) hasło to: asd123
        public IActionResult Login(LoginRequestDto request)
        {
            var response = _service.LoginStudentResponse(request);
            if (Validate(request.Haslo, response.Salt, response.Password))
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, request.Login),
                    new Claim(ClaimTypes.Name, request.Login),
                    new Claim(ClaimTypes.Role, "student")
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken
                (
                    issuer: "Gakko",
                    audience: "Students",
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(10),
                    signingCredentials: creds
                );


                var tokenData = (new
                {
                    accessToken = new JwtSecurityTokenHandler().WriteToken(token),
                    refreshToken = Guid.NewGuid()
                });

                var refreshToken = new SaveRefreshTokenRequest();
                refreshToken.indexNumber = request.Login;
                refreshToken.refreshToken = tokenData.refreshToken.ToString();

                var saveRefreshTokenResponse = _service.SaveRefreshToken(refreshToken);

                return Ok("Poprawnie zalogowano");
            }
            else
            {
                return Ok("Błąd logowania");
            }
        }


        public static string Create(string value, string salt)
        {
            var valueBytes = KeyDerivation.Pbkdf2(
                                password: value,
                                salt: Encoding.UTF8.GetBytes(salt),
                                prf: KeyDerivationPrf.HMACSHA512,
                                iterationCount: 10000,
                                numBytesRequested: 256 / 8);
            return Convert.ToBase64String(valueBytes);
        }

        public static bool Validate(string value, string salt, string hash)
        {
            return Create(value, salt) == hash;
        }

        [HttpPost("refreshToken/{token}")]
        public IActionResult RefreshToken(RefreshTokenRequest refToken)
        {
            var response = _service.RefreshToken(refToken);
            if (null == response.IndexNumber)
            {
                return Ok(response.Message);
            }

            var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, response.IndexNumber),
                    new Claim(ClaimTypes.Name, response.IndexNumber),
                    new Claim(ClaimTypes.Role, "student")
                };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
            (
                issuer: "Gakko",
                audience: "Students",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: creds
            );


            var tokenData = (new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken = Guid.NewGuid()
            });

            var newToken = new SaveRefreshTokenRequest();
            newToken.indexNumber = response.IndexNumber;
            newToken.refreshToken = tokenData.refreshToken.ToString();

            var saveRefreshTokenResponse = _service.SaveRefreshToken(newToken);


            return Ok(response.Message + "\n" + "Nowy Refresh Token: " + newToken.refreshToken.ToString());
        }
    }
}
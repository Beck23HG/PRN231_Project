using API.DTO;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly Project_PRN231Context _context;
        private readonly IConfiguration _configuration;

        public UserController(Project_PRN231Context context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("ViewStudent/{cid}")]
        [Authorize]
        public IActionResult GetStudentInClass(int cid)
        {
            try
            {
                var students = _context.Users
                    .Where(u => u.RoleId == 1 && u.Classes.Any(c => c.ClassId == cid))
                    .Select(u => new
                    {
                        u.UserId,
                        u.Username,
                        u.Fullname,
                        Classes = u.Classes.Where(c => c.ClassId == cid)
                    })
                    .ToList();

                if (students == null || !students.Any())
                {
                    return NotFound("No students found in this class.");
                }

                return Ok(students);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.Fullname))
                {
                    return BadRequest("Invalid input data.");
                }

                if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                {
                    return Conflict("Username already exists. Please choose a different username.");
                }

                var user = new User
                {
                    Username = model.Username,
                    Password = HashSHA256(model.Password),
                    Fullname = model.Fullname,
                    RoleId = 1
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                if (model.SelectedClassIds != null && model.SelectedClassIds.Any())
                {
                    var selectedClasses = await _context.Classes
                        .Where(c => model.SelectedClassIds.Contains(c.ClassId))
                        .ToListAsync();

                    if (selectedClasses.Count != model.SelectedClassIds.Count)
                    {
                        return BadRequest("One or more selected class IDs are invalid.");
                    }

                    foreach (var classItem in selectedClasses)
                    {
                        user.Classes.Add(classItem);
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new { Message = "Registration successful!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = $"Registration failed: {ex.Message}" });
            }
        }

        private string HashSHA256(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}

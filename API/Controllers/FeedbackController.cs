using API.DTO;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly Project_PRN231Context _context;
        private readonly IConfiguration _configuration;

        public FeedbackController(Project_PRN231Context context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("Class/{id}")]
        [Authorize]
        public IActionResult TeacherGetClass(int id)
        {
            try
            {
                var data = _context.Users
                    .Include(u => u.Classes)
                    .Where(u => u.UserId == id)
                    .SelectMany(u => u.Classes)
                    .Select(c => new
                    {
                        c.ClassId,
                        c.ClassName,
                        c.SubjectName,
                        c.Status,
                        Lecture = c.Users.Where(u => u.RoleId == 2).Select(u => new { u.Fullname }).FirstOrDefault(),
                        Feedback = c.Feedbacks.Where(f => f.StudentId == id).FirstOrDefault()
                    })
                    .ToList();

                if (data == null || !data.Any())
                {
                    return NotFound("No classes found for this user.");
                }

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("ClassStudent/{id}")]
        [Authorize]
        public IActionResult StudentGetClass(int id)
        {
            try
            {
                var data = _context.Users
                    .Include(u => u.Classes)
                    .Where(u => u.UserId == id)
                    .SelectMany(u => u.Classes)
                    .Where(c => c.Status == 1)
                    .Select(c => new
                    {
                        c.ClassId,
                        c.ClassName,
                        c.SubjectName,
                        Lecture = c.Users.Where(u => u.RoleId == 2).Select(u => new { u.Fullname }).FirstOrDefault(),
                        Feedback = c.Feedbacks.Where(f => f.StudentId == id).FirstOrDefault()
                    })
                    .ToList();

                if (data == null || !data.Any())
                {
                    return NotFound("No classes found for this user.");
                }

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("ClassById/{id}")]
        [Authorize]
        public IActionResult GetClassByClassId(int id)
        {
            try
            {
                var data = _context.Classes
                    .Where(c => c.ClassId == id)
                    .Select(c => new
                    {
                        c.ClassId,
                        c.ClassName,
                        c.SubjectName,
                        Lecture = c.Users
                            .Where(u => u.RoleId == 2)
                            .Select(u => new { u.Fullname })
                            .FirstOrDefault()
                    })
                    .FirstOrDefault();

                if (data == null)
                {
                    return NotFound("No classes found");
                }

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("ListClasses")]
        public IActionResult GetAllClass()
        {
            try
            {
                var data = _context.Classes
                    .Select(c => new
                    {
                        c.ClassId,
                        c.ClassName,
                        c.SubjectName,
                        c.Status,
                        Lecture = c.Users.Where(u => u.RoleId == 2).Select(u => new { u.Fullname }).FirstOrDefault(),
                    })
                    .ToList();

                if (data == null)
                {
                    return NotFound("No classes found");
                }

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("FeedbackTitle")]
        public IActionResult GetFeedbackDescription()
        {
            try
            {
                var data = _context.FeedbackTitles
                    .Select(f => f.TitleName)
                    .ToList();

                if (data == null || !data.Any())
                {
                    return NotFound("Can't found");
                }

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("ViewFeedback/{id}")]
        [Authorize]
        public IActionResult GetFeedbackInClass(int id)
        {
            try
            {
                // Lấy RoleId từ token
                var roleIdClaim = User.FindFirst("RoleId")?.Value;
                if (string.IsNullOrEmpty(roleIdClaim) || int.Parse(roleIdClaim) != 2)
                {
                    return Forbid("Only users with role is Teacher can access this endpoint.");
                }

                var data = _context.Feedbacks
                    .Where(f => f.ClassId == id)
                    .Select(f => new
                    {
                        f.FeedbackId,
                        f.Title1,
                        f.Title2,
                        f.Title3,
                        f.Content,
                        f.Status,
                        f.Class
                    })
                    .ToList();

                if (data == null || !data.Any())
                {
                    return NotFound("No feedback found for this class.");
                }

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("OpenFeedback/{classId}")]
        [Authorize]
        public IActionResult OpenFeedback([FromRoute] int classId, [FromBody] int status)
        {
            try
            {
                var roleIdClaim = User.FindFirst("RoleId")?.Value;
                if (string.IsNullOrEmpty(roleIdClaim) || int.Parse(roleIdClaim) != 2)
                {
                    return Forbid("Only users with role is Teacher can access this endpoint.");
                }
                Class cl = _context.Classes.FirstOrDefault(cl => cl.ClassId == classId);
                if (cl == null)
                {
                    return NotFound("Cannot found class");
                }
                cl.Status = status;
                _context.Classes.Update(cl);
                _context.SaveChanges();
                return Ok(cl);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("DoFeedback")]
        [Authorize]
        public IActionResult DoFeedback([FromBody] Feedback feedback)
        {
            // Lấy RoleId từ token
            var roleIdClaim = User.FindFirst("RoleId")?.Value;
            if (string.IsNullOrEmpty(roleIdClaim) || int.Parse(roleIdClaim) != 1)
            {
                return Forbid("Only users with role is Student can access this endpoint.");
            }
            _context.Feedbacks.Add(feedback);
            _context.SaveChanges();
            return Created("", feedback);
        }

        [HttpPut("EditFeedback/{id}")]
        [Authorize]
        public IActionResult EditFeedback([FromRoute] int id, [FromBody] FeedbackDTO feedback)
        {
            // Lấy RoleId từ token
            var roleIdClaim = User.FindFirst("RoleId")?.Value;
            if (string.IsNullOrEmpty(roleIdClaim) || int.Parse(roleIdClaim) != 1)
            {
                return Forbid("Only users with role is Student can access this endpoint.");
            }
            List<Feedback> feedbacks = _context.Feedbacks.ToList();
            Feedback fb = feedbacks.FirstOrDefault(f => f.FeedbackId == id);
            if(fb == null)
            {
                return BadRequest("Feedback doesn't exit");
            }
            else
            {
                fb.Title1 = feedback.Title1;
                fb.Title2 = feedback.Title2;
                fb.Title3 = feedback.Title3;
                fb.Content = feedback.Content;
                _context.Update(fb);
                _context.SaveChanges();
                return Created("", feedback);
            }
        }

        [HttpGet("GetStudentFeedback/{studentId}/{classId}")]
        [Authorize]
        public IActionResult GetStudentFeedback(int studentId, int classId)
        {
            try
            {
                var roleIdClaim = User.FindFirst("RoleId")?.Value;
                if (string.IsNullOrEmpty(roleIdClaim) || int.Parse(roleIdClaim) != 1)
                {
                    return Forbid("Only users with role is Student can access this endpoint.");
                }

                var feedbacks = _context.Feedbacks
                    .Where(f => f.StudentId == studentId && f.ClassId == classId)
                    .FirstOrDefault();

                return Ok(feedbacks);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}


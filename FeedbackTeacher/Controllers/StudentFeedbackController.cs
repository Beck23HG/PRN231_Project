using FeedbackTeacher.DTO;
using FeedbackTeacher.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace FeedbackTeacher.Controllers
{
    public class StudentFeedbackController : Controller
    {
        private readonly Manager manager = new Manager();


        [Authorize(Roles = "1")]
        public async Task<IActionResult> ListFeedback()
        {
            string token = HttpContext.Session.GetString("Token");
            UserInfo userInfo = manager.GetUserInfoFromToken(token);
            var (classes, error) = await manager.GetClassById(userInfo.UserId, token);
            ViewBag.Classes = classes;
            if (!string.IsNullOrEmpty(error))
            {
                ViewBag.Message = error;
            }
            return View();
        }

        [Authorize(Roles = "1")]
        public async Task<IActionResult> DoFeedback(int classId)
        {
            string token = HttpContext.Session.GetString("Token");
            UserInfo userInfo = manager.GetUserInfoFromToken(token);
            bool isStudentInClass = await manager.IsStudentInClass(userInfo.UserId, classId, token);
            if (!isStudentInClass)
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            List<string> titles = await manager.GetFeedbackQuestions();
            ViewBag.Titles = titles;
            ClassDTO classdto = await manager.GetClassByClassId(classId, token);
            ViewBag.Class = classdto.ClassName;
            ViewBag.Subject = classdto.SubjectName;
            ViewBag.Teacher = classdto.Lecture.Fullname;
            ViewBag.ClassId = classId;
            ViewBag.UserId = userInfo.UserId;
            return View();
        }

        [Authorize(Roles = "1")]
        [HttpPost]
        public async Task<IActionResult> SubmitFeedback(int ClassId, int UserId, string Comments, 
            int Rating_0, int Rating_1, int Rating_2)
        {
            string token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var feedback = new Feedback
                {
                    StudentId = UserId,
                    ClassId = ClassId,
                    Content = Comments,
                    Title1 = Rating_0,
                    Title2 = Rating_1,
                    Title3 = Rating_2,
                    Status = 1
                };

                await manager.DoFeedback(feedback, token);

                TempData["SuccessMessage"] = "Feedback submitted successfully!";
                return RedirectToAction("ListFeedback");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to submit feedback: {ex.Message}";
                ViewBag.Titles = await manager.GetFeedbackQuestions();
                ViewBag.ClassId = ClassId;
                ViewBag.UserId = UserId;
                return View("DoFeedback");
            }
        }

        [Authorize(Roles = "1")]
        [HttpGet]
        public async Task<IActionResult> EditFeedback(int classId)
        {
            string token = HttpContext.Session.GetString("Token");
            UserInfo userInfo = manager.GetUserInfoFromToken(token);
            bool isStudentInClass = await manager.IsStudentInClass(userInfo.UserId, classId, token);
            if (!isStudentInClass)
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            ViewBag.UserId = userInfo.UserId;
            List<string> titles = await manager.GetFeedbackQuestions();
            ViewBag.Titles = titles;
            ClassDTO classdto = await manager.GetClassByClassId(classId, token);
            ViewBag.Class = classdto.ClassName;
            ViewBag.Subject = classdto.SubjectName;
            ViewBag.Teacher = classdto.Lecture.Fullname;
            ViewBag.ClassId = classId;
            Feedback feedback = await manager.GetStudentFeedback(userInfo.UserId, classId, token);
            ViewBag.Feedback = feedback;
            return View();
        }

        [Authorize(Roles = "1")]
        [HttpPost]
        public async Task<IActionResult> EditFeedback(int ClassId, int UserId, int FeedbackId,
            string Comments, int Rating_0, int Rating_1, int Rating_2)
        {
            string token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var feedback = new Feedback
                {
                    Content = Comments,
                    Title1 = Rating_0,
                    Title2 = Rating_1,
                    Title3 = Rating_2,
                };

                await manager.EditFeedback(FeedbackId, feedback, token);

                TempData["SuccessMessage"] = "Edit feedback successfully!";
                return RedirectToAction("ListFeedback");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to edit feedback: {ex.Message}";
                ViewBag.Titles = await manager.GetFeedbackQuestions();
                ViewBag.ClassId = ClassId;
                ViewBag.UserId = UserId;
                return View("EditFeedback");
            }
        }

    }
}

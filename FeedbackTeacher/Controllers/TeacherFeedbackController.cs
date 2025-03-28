using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackTeacher.Controllers
{
    public class TeacherFeedbackController : Controller
    {
        private readonly Manager manager = new Manager();

        [Authorize(Roles = "2")]
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

        [Authorize(Roles = "2")]
        public async Task<IActionResult> FeedbackDetail(int classId)
        {
            string token = HttpContext.Session.GetString("Token");
            UserInfo userInfo = manager.GetUserInfoFromToken(token);
            bool isStudentInClass = await manager.IsStudentInClass(userInfo.UserId, classId, token);
            if (!isStudentInClass)
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            var feedbacks = await manager.GetFeedbackInClass(classId, token);
            ViewBag.Feedbacks = feedbacks;
            List<string> titles = await manager.GetFeedbackQuestions();
            ViewBag.Title1 = titles[0];
            ViewBag.Title2 = titles[1];
            ViewBag.Title3 = titles[2];

            double avgTitle1 = feedbacks.Average(f => f.Title1);
            double avgTitle2 = feedbacks.Average(f => f.Title2);
            double avgTitle3 = feedbacks.Average(f => f.Title3);

            ViewBag.AvgTitle1 = avgTitle1;
            ViewBag.AvgTitle2 = avgTitle2;
            ViewBag.AvgTitle3 = avgTitle3;

            double average = (avgTitle1 + avgTitle2 + avgTitle3) / 3.0;
            string title;
            if(average >= 0 && average <= 1.99)
            {
                title = "Bad";
            }
            else if (average >= 2 && average <= 3.99)
            {
                title = "Medium";
            }
            else
            {
                title = "Good";
            }
            ViewBag.OverallAverage = average;
            ViewBag.OverallRating = title;
            return View();
        }

        [Authorize(Roles = "2")]
        public async Task<IActionResult> OpenFeedback(int classId)
        {
            string token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                await manager.OpenFeedback(classId, 1, token);
                TempData["Message"] = "Feedback has been opened successfully.";

            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to open feedback: {ex.Message}";
            }

            return RedirectToAction("ListFeedback");
        }

        [Authorize(Roles = "2")]
        public async Task<IActionResult> CloseFeedback(int classId)
        {
            string token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                await manager.OpenFeedback(classId, 0, token);
                TempData["Message"] = "Feedback has been closed successfully.";

            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to close feedback: {ex.Message}";
            }

            return RedirectToAction("ListFeedback");
        }
    }
}

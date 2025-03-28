using FeedbackTeacher.DTO;
using FeedbackTeacher.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace FeedbackTeacher.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Manager manager = new Manager();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View("Login");
        }
        public async Task<IActionResult> Home()
        {
            string token = HttpContext.Session.GetString("Token");
            UserInfo userInfo = manager.GetUserInfoFromToken(token);
            ViewBag.User = userInfo;

            var (classes, error) = await manager.GetClassById(userInfo.UserId, token);
            ViewBag.Classes = classes;
            if (!string.IsNullOrEmpty(error))
            {
                ViewBag.Message = error;
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ViewClassDetails(int classId)
        {
            string token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            UserInfo userInfo = manager.GetUserInfoFromToken(token);
            ViewBag.User = userInfo;
            bool isStudentInClass = await manager.IsStudentInClass(userInfo.UserId, classId, token);
            if (!isStudentInClass)
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            var (classes, error) = await manager.GetClassById(userInfo.UserId, token);
            ViewBag.Classes = classes;
            if (!string.IsNullOrEmpty(error))
            {
                ViewBag.Message = error;
            }

            List<User> students = await manager.GetStudentInClass(classId, token);
            Class c = students[0].Classes.FirstOrDefault();
            ViewBag.Class = c;
            ViewBag.Students = await manager.GetStudentInClass(classId, token);

            return View("Home");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO login)
        {
            try
            {
                string token = await manager.Login(login);
                HttpContext.Session.SetString("Token", token);

                var userInfo = manager.GetUserInfoFromToken(token);
                HttpContext.Session.SetString("Fullname", userInfo.Fullname);
                HttpContext.Session.SetInt32("UserId", userInfo.UserId);
                return RedirectToAction("Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(login);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            ViewBag.Classes = await manager.GetAllClasses();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDTO model)
        {
            try
            {
                bool success = await manager.Register(model);
                if (success)
                {
                    TempData["Message"] = "Registration successful! You can now log in.";
                    return RedirectToAction("Login");
                }
                else
                {
                    TempData["Error"] = "Registration failed.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("Token");
            HttpContext.Session.Remove("Fullname");
            return RedirectToAction("Login");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public static string HashSHA256(string input)
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
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}

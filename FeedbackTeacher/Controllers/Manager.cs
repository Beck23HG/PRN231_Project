using FeedbackTeacher.DTO;
using FeedbackTeacher.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FeedbackTeacher.Controllers
{
    public class Manager
    {
        private readonly String url = "https://localhost:7169/api/";

        internal async Task<string> Login(LoginDTO login)
        {
            using (HttpClient client = new HttpClient())
            {
                string json = JsonConvert.SerializeObject(login);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage res = await client.PostAsync(url + "Auth/login", content);

                if (res.IsSuccessStatusCode)
                {
                    string data = await res.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<LoginResponseDTO>(data);
                    return response.Token;
                }
                else
                {
                    throw new Exception("Login failed: " + res.ReasonPhrase);
                }
            }
        }

        internal UserInfo GetUserInfoFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var claims = jwtToken.Claims;

            return new UserInfo
            {
                UserId = int.Parse(claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value),
                Username = claims.First(c => c.Type == ClaimTypes.Name).Value,
                Fullname = claims.First(c => c.Type == "Fullname").Value,
                RoleId = int.Parse(claims.First(c => c.Type == "RoleId").Value)
            };
        }

        public async Task<bool> Register(RegisterDTO model)
        {
            using (HttpClient client = new HttpClient())
            {
                string json = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage res = await client.PostAsync(url + "User/Register", content);

                if (res.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    string error = await res.Content.ReadAsStringAsync();
                    throw new Exception($"Registration failed: {error}");
                }
            }
        }

        internal async Task<(List<ClassDTO> Classes, string Error)> GetClassById(int id, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                using (HttpResponseMessage res = await client.GetAsync(url + "Feedback/Class/" + id))
                {
                    if (!res.IsSuccessStatusCode)
                    {
                        string error = await res.Content.ReadAsStringAsync();
                        return (new List<ClassDTO>(), $"{res.StatusCode} - {error}");
                    }

                    using (HttpContent content = res.Content)
                    {
                        string data = await content.ReadAsStringAsync();
                        List<ClassDTO> classes = JsonConvert.DeserializeObject<List<ClassDTO>>(data);
                        return (classes ?? new List<ClassDTO>(), null);
                    }
                }
            }
        }

        internal async Task<(List<ClassDTO> Classes, string Message)> StudentGetClassById(int id, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                using (HttpResponseMessage res = await client.GetAsync(url + "Feedback/ClassStudent/" + id))
                {
                    if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return (new List<ClassDTO>(), "Hiện tại không có lớp nào được mở feedback.");
                    }

                    if (!res.IsSuccessStatusCode)
                    {
                        string error = await res.Content.ReadAsStringAsync();
                        throw new Exception($"Failed to get classes: {res.StatusCode} - {error}");
                    }

                    using (HttpContent content = res.Content)
                    {
                        string data = await content.ReadAsStringAsync();
                        List<ClassDTO> classes = JsonConvert.DeserializeObject<List<ClassDTO>>(data);
                        return (classes ?? new List<ClassDTO>(), null);
                    }
                }
            }
        }

        internal async Task<ClassDTO> GetClassByClassId(int id, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                using (HttpResponseMessage res = await client.GetAsync(url + "Feedback/ClassById/" + id))
                {
                    if (!res.IsSuccessStatusCode)
                    {
                        string error = await res.Content.ReadAsStringAsync();
                        throw new Exception($"Failed to get classes: {res.StatusCode} - {error}");
                    }

                    using (HttpContent content = res.Content)
                    {
                        string data = await content.ReadAsStringAsync();
                        ClassDTO classes = JsonConvert.DeserializeObject<ClassDTO>(data);
                        return classes;
                    }
                }
            }
        }

        internal async Task<List<ClassDTO>> GetAllClasses()
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage res = await client.GetAsync(url + "Feedback/ListClasses"))
                {
                    if (!res.IsSuccessStatusCode)
                    {
                        string error = await res.Content.ReadAsStringAsync();
                        return (new List<ClassDTO>());
                    }

                    using (HttpContent content = res.Content)
                    {
                        string data = await content.ReadAsStringAsync();
                        List<ClassDTO> classes = JsonConvert.DeserializeObject<List<ClassDTO>>(data);
                        return (classes ?? new List<ClassDTO>());
                    }
                }
            }
        }

        internal async Task<List<string>> GetFeedbackQuestions()
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage res = await client.GetAsync(url + "Feedback/FeedbackTitle"))
                {
                    if (!res.IsSuccessStatusCode)
                    {
                        string error = await res.Content.ReadAsStringAsync();
                        throw new Exception($"Failed to get questions: {res.StatusCode} - {error}");
                    }

                    using (HttpContent content = res.Content)
                    {
                        string data = await content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<List<string>>(data) ?? new List<string>();
                    }
                }
            }
        }

        internal async Task DoFeedback(Feedback feedback, string token)
        {

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                string json = JsonConvert.SerializeObject(feedback);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage res = await client.PostAsync(url + "Feedback/DoFeedback", content);

            }
        }

        internal async Task<Feedback> GetStudentFeedback(int sid, int cid, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                using (HttpResponseMessage res = await client.GetAsync(url + $"Feedback/GetStudentFeedback/{sid}/{cid}"))
                {
                    if (!res.IsSuccessStatusCode)
                    {
                        string error = await res.Content.ReadAsStringAsync();
                        throw new Exception($"Failed to get classes: {res.StatusCode} - {error}");
                    }

                    using (HttpContent content = res.Content)
                    {
                        string data = await content.ReadAsStringAsync();
                        Feedback feedback = JsonConvert.DeserializeObject<Feedback>(data);
                        return feedback;
                    }
                }
            }
        }

        internal async Task EditFeedback(int fid, Feedback feedback, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                string json = JsonConvert.SerializeObject(feedback);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage res = await client.PutAsync(url + $"Feedback/EditFeedback/{fid}", content);
            }
        }

        internal async Task OpenFeedback(int classId, int status, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                string json = JsonConvert.SerializeObject(status);
                StringContent content = new StringContent (json, Encoding.UTF8, "application/json");
                HttpResponseMessage res = await client.PutAsync(url + "Feedback/OpenFeedback/" + classId, content);
            }
        }

        internal async Task<List<Feedback>> GetFeedbackInClass(int classId, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                using (HttpResponseMessage res = await client.GetAsync(url + "Feedback/ViewFeedback/" + classId))
                {
                    if (!res.IsSuccessStatusCode)
                    {
                        string error = await res.Content.ReadAsStringAsync();
                        throw new Exception($"Failed to get classes: {res.StatusCode} - {error}");
                    }

                    using (HttpContent content = res.Content)
                    {
                        string data = await content.ReadAsStringAsync();
                        List<Feedback> feedbacks = JsonConvert.DeserializeObject<List<Feedback>>(data);
                        return feedbacks;
                    }
                }
            }
        }

        internal async Task<List<User>> GetStudentInClass(int cid, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                using (HttpResponseMessage res = await client.GetAsync(url + "User/ViewStudent/" + cid))
                {
                    if (!res.IsSuccessStatusCode)
                    {
                        string error = await res.Content.ReadAsStringAsync();
                        throw new Exception($"Failed to get classes: {res.StatusCode} - {error}");
                    }

                    using (HttpContent content = res.Content)
                    {
                        string data = await content.ReadAsStringAsync();
                        List<User> users = JsonConvert.DeserializeObject<List<User>>(data);
                        return users;
                    }
                }
            }
        }

        public async Task<bool> IsStudentInClass(int userId, int classId, string token)
        {
            var (classes, error) = await GetClassById(userId, token);
            if (!string.IsNullOrEmpty(error))
            {
                return false;
            }

            return classes.Any(c => c.ClassId == classId);
        }

    }

    public class UserInfo
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Fullname { get; set; }
        public int RoleId { get; set; }
    }
}

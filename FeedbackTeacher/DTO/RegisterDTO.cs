namespace FeedbackTeacher.DTO
{
    public class RegisterDTO
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Fullname { get; set; } = null!;
        public List<int> SelectedClassIds { get; set; } = new List<int>();
    }
}

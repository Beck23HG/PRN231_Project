using FeedbackTeacher.Models;

namespace FeedbackTeacher.DTO
{
    public class ClassDTO
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = null!;
        public string SubjectName { get; set; } = null!;
        public int? Status { get; set; }
        public LectureDTO Lecture { get; set; }
        public Feedback? Feedback { get; set; }

    }
}

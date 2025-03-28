using System;
using System.Collections.Generic;

namespace API.Models
{
    public partial class Feedback
    {
        public int FeedbackId { get; set; }
        public int StudentId { get; set; }
        public int ClassId { get; set; }
        public int Title1 { get; set; }
        public int Title2 { get; set; }
        public int Title3 { get; set; }
        public string? Content { get; set; }
        public int? Status { get; set; }

        public virtual Class? Class { get; set; } = null!;
        public virtual User? Student { get; set; } = null!;
    }
}

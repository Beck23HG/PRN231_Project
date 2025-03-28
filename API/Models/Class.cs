using System;
using System.Collections.Generic;

namespace API.Models
{
    public partial class Class
    {
        public Class()
        {
            Feedbacks = new HashSet<Feedback>();
            Users = new HashSet<User>();
        }

        public int ClassId { get; set; }
        public string ClassName { get; set; } = null!;
        public string SubjectName { get; set; } = null!;
        public int? Status { get; set; }

        public virtual ICollection<Feedback> Feedbacks { get; set; }

        public virtual ICollection<User> Users { get; set; }
    }
}

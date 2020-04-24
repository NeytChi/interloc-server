using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace common
{
    public partial class Question
    {
        public Question()
        {
            this.answers = new HashSet<Answer>();
        }
        [Key]
        public int question_id { get; set; }

        [Column("question", TypeName="text CHARACTER SET utf8 COLLATE utf8_general_ci")]
        public string question { get; set; }
        public long created_at { get; set; }
        public bool deleted { get; set; }
        public virtual ICollection<Answer> answers { get; set; }
    }
}
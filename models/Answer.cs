using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace common
{
    public partial class Answer
    {
        public Answer()
        {
        }
        [Key]
        public int answer_id { get; set; }
        [ForeignKey("question")]
        public int question_id { get; set; }
        [Column("answer", TypeName="text CHARACTER SET utf8 COLLATE utf8_general_ci")]
        public int answer { get; set; }
        public long created_at { get; set; }
        public bool deleted { get; set; }
        public virtual Question question { get; set; }
    }
}
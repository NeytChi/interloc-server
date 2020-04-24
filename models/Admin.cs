using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace common
{
    public partial class Admin
    {
        public Admin()
        {
        }
        [Key]
        public int admin_id { get; set; }
        [Column("admin_email", TypeName="varchar(100)")]
        public string admin_email { get; set; }
        public int admin_role { get; set; }
        [Column("admin_password", TypeName="varchar(256)")]
        public string admin_password { get; set; }
        [Column("admin_hash", TypeName="varchar(100)")]
        public string activate_hash { get; set; }
        public long created_at { get; set; }
        public long last_login_at { get; set; }
        public int? recovery_code { get; set; }
        public bool deleted { get; set; }
    }
    public class AdminCache
    {
        public int admin_id { get; set; }
        [StringLength(100, ErrorMessage = "Email can't exceed 100 characters.")]
        public string admin_email { get; set; }
        public string admin_password { get; set; }
    }
}

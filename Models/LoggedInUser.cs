using System.ComponentModel.DataAnnotations;

namespace localshopyNew.Models
{
    public class LoggedInUser
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public string EmailId { get; set; } = string.Empty;
        [Required]
        public string LoggedInType { get; set; } = string.Empty;
        [Required]
        public string Role { get; set; } = string.Empty;
        [Required]
        public DateTime LoggedInTime { get; set; } = DateTime.UtcNow;
    }
}

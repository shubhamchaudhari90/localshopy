using System.ComponentModel.DataAnnotations;

namespace localshopyNew.Models
{
    public class Flag
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Key { get; set; } = string.Empty;

        [Required]
        public string Value { get; set; } = string.Empty;
    }
}

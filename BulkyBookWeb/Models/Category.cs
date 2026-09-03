using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

namespace BulkyBookWeb.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }


    }
}

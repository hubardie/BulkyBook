using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BulkyBook.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "ISBN")]
        public string ISBN { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Author")]
        public string Author { get; set; } = string.Empty;

        [Required]
        [Range(1, 1000, ErrorMessage = "Must be a vaue between 1 and 1000")]
        [Display(Name = "List Price")]
        public double ListPrice { get; set; }
        
        [Required]
        [Range(1, 1000, ErrorMessage = "Must be a vaue between 1 and 1000")]
        [Display(Name = "Price for 1-50")]
        public double Price { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "Must be a vaue between 1 and 1000")]
        [Display(Name = "Price for 50+")]
        public double Price50 { get; set; }

        [Required]
        [Display(Name = "Price for 100+")]
        public double Price100 { get; set; }

        public int CategoryId { get; set; }
        
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }


        [ValidateNever]
        [Display(Name = "Product Image")]
        public string? ImageUrl { get; set; }

    }
}

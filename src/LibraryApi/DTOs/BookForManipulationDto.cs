using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs
{
    public abstract class BookForManipulationDto
    {
        [Required(ErrorMessage = "The Title is required")]
        [MaxLength(100, ErrorMessage = "The length of Title cannot be more that 100 characters")]
        public string Title { get; set; }

        [MaxLength(500, ErrorMessage = "The description length is a maximum of 500 characters")]
        public virtual string Description { get; set; }
    }
}

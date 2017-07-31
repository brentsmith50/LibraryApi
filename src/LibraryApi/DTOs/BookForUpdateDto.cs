using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs
{
    public class BookForUpdateDto : BookForManipulationDto
    {
        [Required(ErrorMessage = "A description is required to update an existing book")]
        public override string Description
        {
            get
            {
                return base.Description;
            }

            set
            {
                base.Description = value;
            }
        }
    }
}

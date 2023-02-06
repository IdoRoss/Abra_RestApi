using System.ComponentModel.DataAnnotations;

namespace Abra_RestApi.Models
{
    // ------------ PART 2 -------------//
    public class User
    {
        [Required]
        public int Id { get; set; }
        public string Name { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [RegularExpression("male|female", ErrorMessage = "The Gender must be either 'male' or 'female' only.")]
        public string Gender { get; set; }
        [Range(0, 120)]
        public int Age { get; set; }
    }
}

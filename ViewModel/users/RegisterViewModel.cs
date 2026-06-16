using System.ComponentModel.DataAnnotations;
namespace Book_Store.ViewModel.users 
{
    public class RegisterViewModel 
    {
        public string FullName { get; set; } = string.Empty;
       public string Username { get; set; } = string.Empty; 

        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày sinh không được để trống.")]
        [DataType(DataType.Date, ErrorMessage = "Ngày sinh không hợp lệ.")]
        public DateTime? DateOfBirth { get; set; }

        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
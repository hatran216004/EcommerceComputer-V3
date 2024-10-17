using System.ComponentModel.DataAnnotations;

namespace Store_EF.Models
{
    public class CheckOutForm
    {
        [Required(ErrorMessage = "Họ và tên không được để trống!")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống!")]
        [Display(Name = "Số điện thoại")]
        [RegularExpression("^0[0-9]{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ!")]
        [Phone]
        public string Phone { get; set; }

        [Required]
        [Display(Name = "Thành phố / Tỉnh")]
        public int Province { get; set; }

        [Required]
        [Display(Name = "Quận / Huyện")]
        public int District { get; set; }

        [Required]
        [Display(Name = "Xã / Phường")]
        public int Ward { get; set; }

        [Required]
        [Display(Name = "Số nhà và tên đường")]
        public string Home { get; set; }

        [Display(Name = "Ghi chú")]
        public string Note { get; set; }

        [Required(ErrorMessage = "Hãy chọn 1 trong 2 phương thức thanh toán!")]
        [Display(Name = "Phương thức thanh toán")]
        [RegularExpression("^(Bank|Cash)$", ErrorMessage = "Phương thức thanh toán không hợp lệ!")]
        public string PaymentMethod { get; set; }

        public CheckOutForm() { }

        public CheckOutForm(UserDetail detail)
        {
            FullName = detail.Name;
            Phone = detail.Phone;
        }
    }
}

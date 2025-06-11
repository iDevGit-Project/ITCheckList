using System.ComponentModel.DataAnnotations;

namespace ITCheckList.Models
{
    public class TBL_CheckItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "وارد کردن بخش الزامی است.")]
        public string Section { get; set; }

        [Required(ErrorMessage = "شرح مورد الزامی است.")]
        public string Description { get; set; }

        public bool Status { get; set; }

        public string? Note { get; set; }

        public string? Duration { get; set; } // ذخیره زمان به صورت MM:SS

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

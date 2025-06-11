using ITCheckList.Extentions;

namespace ITCheckList.Models.VModels
{
    public class CheckItemViewModels
    {
        public int Id { get; set; }
        public string Section { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Note { get; set; }

        public DateTime CreatedAt { get; set; }

        public string CreatedAtShamsi => CreatedAt.ToShamsi(); // تبدیل شمسی
    }
}

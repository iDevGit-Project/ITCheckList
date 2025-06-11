namespace ITCheckList.Models
{
    public class TBL_CheckItemArchive
    {
        public int Id { get; set; }
        public string Section { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Note { get; set; }
        public bool Status { get; set; }
        public DateTime ArchivedAt { get; set; } // زمان بایگانی
        public string? Duration { get; set; } // زمان صرف‌شده به صورت رشته‌ای مثل "01:45"
    }

}

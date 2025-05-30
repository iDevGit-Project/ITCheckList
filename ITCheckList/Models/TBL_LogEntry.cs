namespace ITCheckList.Models
{
    public class TBL_LogEntry
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Username { get; set; } // نام کاربر یا "ناشناس"
        public string Action { get; set; } // Create / Edit / Delete / Login / Error / ...
        public string EntityName { get; set; } // مانند TBLCheckItem
        public string EntityId { get; set; } // مثلاً 12
        public string Description { get; set; } // توضیحات دلخواه
        public string IP { get; set; } // آدرس IP
    }
}

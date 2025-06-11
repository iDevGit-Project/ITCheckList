namespace ITCheckList.Models
{
    public class TableOptimizationReport
    {
        public string TableName { get; set; }
        public long RowCountBefore { get; set; }
        public long RowCountAfter { get; set; }
        public long SizeBeforeKB { get; set; }
        public long SizeAfterKB { get; set; }
        public bool WasOptimized { get; set; }
    }

}

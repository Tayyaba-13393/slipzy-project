
namespace SlipzyD2.Models
{
    public class ReceiptFile
    {
        public int ReceiptFileId { get; set; }

        public int PurchaseId { get; set; }

        public string OriginalFileName { get; set; } = "";

        public string RelativePath { get; set; } = "";

        public DateTime UploadedAt { get; set; }
    }
}

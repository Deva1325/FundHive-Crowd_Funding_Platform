using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Crowd_Funding_Platform.Helpers
{
    public class PdfHelper
    {
        // Returns PDF as byte array (already present)
        public static byte[] GenerateReceipt(string userName, string campaignTitle, decimal amount, string transactionId)
        {
            using (var ms = new MemoryStream())
            {
                Document doc = new Document();
                PdfWriter.GetInstance(doc, ms);
                doc.Open();
                doc.Add(new Paragraph("Donation Receipt"));
                doc.Add(new Paragraph($"Name: {userName}"));
                doc.Add(new Paragraph($"Campaign: {campaignTitle}"));
                doc.Add(new Paragraph($"Amount: ₹{amount}"));
                doc.Add(new Paragraph($"Transaction ID: {transactionId}"));
                doc.Add(new Paragraph($"Date: {DateTime.Now:dd-MM-yyyy HH:mm}"));
                doc.Close();
                return ms.ToArray();
            }
        }

        // ✅ New method: Returns base64 string of PDF
        public static string GenerateReceiptBase64(string userName, string campaignTitle, decimal amount, string transactionId)
        {
            byte[] pdfBytes = GenerateReceipt(userName, campaignTitle, amount, transactionId);
            return Convert.ToBase64String(pdfBytes);
        }
    }
}



//using System.IO;
//using iTextSharp.text;
//using iTextSharp.text.pdf;

//namespace Crowd_Funding_Platform.Helpers
//{
//    public class PdfHelper
//    {
//        public static byte[] GenerateReceipt(string userName, string campaignTitle, decimal amount, string transactionId)
//        {
//            using (var ms = new MemoryStream())
//            {
//                Document doc = new Document();
//                PdfWriter.GetInstance(doc, ms);
//                doc.Open();
//                doc.Add(new Paragraph("Donation Receipt"));
//                doc.Add(new Paragraph($"Name: {userName}"));
//                doc.Add(new Paragraph($"Campaign: {campaignTitle}"));
//                doc.Add(new Paragraph($"Amount: ₹{amount}"));
//                doc.Add(new Paragraph($"Transaction ID: {transactionId}"));
//                doc.Add(new Paragraph($"Date: {DateTime.Now:dd-MM-yyyy HH:mm}"));
//                doc.Close();
//                return ms.ToArray();
//            }
//        }
//    }
//}

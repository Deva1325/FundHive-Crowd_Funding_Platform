using iTextSharp.text.pdf;
using iTextSharp.text;

namespace Crowd_Funding_Platform.Helpers
{
    public class PdfCertificateGenerator
    {
        public byte[] CreateCertificate(string userName, string rewardLevel, decimal totalAmount)
        {
            using (var ms = new MemoryStream())
            {
                var document = new Document(PageSize.A4);
                var writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 24);
                var bodyFont = FontFactory.GetFont(FontFactory.HELVETICA, 14);

                document.Add(new Paragraph("Certificate of Appreciation", titleFont));
                document.Add(new Paragraph($"This certificate is proudly presented to {userName}.", bodyFont));
                document.Add(new Paragraph($"For contributing ₹{totalAmount} and achieving the {rewardLevel} level.", bodyFont));
                document.Add(new Paragraph($"Date: {DateTime.Now:dd MMM yyyy}", bodyFont));

                document.Close();
                return ms.ToArray();
            }
        }
    }
}


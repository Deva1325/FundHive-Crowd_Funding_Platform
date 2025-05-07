using Crowd_Funding_Platform.Models;
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace Crowd_Funding_Platform.Helpers
{
    public class CertificateHelper
    {
        public static string GenerateCertificatePDF(User user, string badgeTitle, decimal totalContribution)
        {
            string certFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Certificates");
            if (!Directory.Exists(certFolder))
                Directory.CreateDirectory(certFolder);

            string fileName = $"User_{user.UserId}_{badgeTitle}.pdf";
            string filePath = Path.Combine(certFolder, fileName);

            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                Rectangle customSize = new Rectangle(PageSize.A4.Width, 595f);  // Keep height ~595 to stay one page
                Document doc = new Document(customSize, 40f, 40f, 40f, 40f);  // Increased margin for better breathing
                PdfWriter writer = PdfWriter.GetInstance(doc, fs);
                doc.Open();

                PdfContentByte cb = writer.DirectContent;
                cb.SetColorStroke(new BaseColor(212, 175, 55));
                cb.SetLineWidth(3f);
                cb.Rectangle(20, 20, doc.PageSize.Width - 40, doc.PageSize.Height - 40);
                cb.Stroke();

                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 24, BaseColor.BLACK);
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, BaseColor.DARK_GRAY);
                var nameFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, BaseColor.BLUE);
                var contentFont = FontFactory.GetFont(FontFactory.HELVETICA, 13, BaseColor.BLACK);
                var footerFont = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 11, BaseColor.DARK_GRAY);

                doc.Add(new Paragraph("\n\n"));  // Slight breathing space

                // Title at the top
                doc.Add(new Paragraph("Certificate of Achievement", titleFont) { Alignment = Element.ALIGN_CENTER });

                // More space for separation
                doc.Add(new Paragraph("\n\n"));

                // Main Content block in the middle
                Paragraph content = new Paragraph
                {
                    Alignment = Element.ALIGN_CENTER
                };
                content.Add(new Phrase("This is to certify that\n\n", contentFont));
                content.Add(new Phrase(user.Username + "\n\n", nameFont));
                content.Add(new Phrase("has successfully achieved the\n\n", contentFont));
                content.Add(new Phrase($"{badgeTitle.ToUpper()} Badge\n\n", headerFont));
                content.Add(new Phrase($"With a total contribution of ₹{totalContribution}\n\n", contentFont));
                doc.Add(content);

                // Badge Image
                string badgePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "RewardsImages", $"{badgeTitle.ToLower()}.png");
                if (File.Exists(badgePath))
                {
                    Image badge = Image.GetInstance(badgePath);
                    badge.ScaleAbsolute(70f, 70f);
                    badge.Alignment = Element.ALIGN_CENTER;
                    doc.Add(badge);
                }

                doc.Add(new Paragraph("\n"));

                doc.Add(new Paragraph($"Date of Achievement: {DateTime.Now:dd MMMM yyyy}", contentFont) { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph("FundHive - Crowdfunding Platform", footerFont) { Alignment = Element.ALIGN_CENTER });

                // Table for Signature and Seal
                PdfPTable table = new PdfPTable(2);
                table.WidthPercentage = 100;
                table.SpacingBefore = 60f;
                table.DefaultCell.Border = Rectangle.NO_BORDER;

                // Signature Cell
                PdfPCell leftCell = new PdfPCell { Border = Rectangle.NO_BORDER, FixedHeight = 80f, VerticalAlignment = Element.ALIGN_MIDDLE };
                string signaturePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "signature.jpeg");
                if (File.Exists(signaturePath))
                {
                    Image signature = Image.GetInstance(signaturePath);
                    signature.ScaleAbsolute(100f, 50f);
                    signature.Alignment = Element.ALIGN_LEFT;
                    leftCell.AddElement(signature);
                }
                leftCell.AddElement(new Paragraph("Instructor's Signature", contentFont) { Alignment = Element.ALIGN_LEFT });

                // Seal Cell
                PdfPCell rightCell = new PdfPCell { Border = Rectangle.NO_BORDER, FixedHeight = 80f, VerticalAlignment = Element.ALIGN_MIDDLE };
                string sealPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "FundHive_Certificate_Logo.JPG");
                if (File.Exists(sealPath))
                {
                    Image seal = Image.GetInstance(sealPath);
                    seal.ScaleAbsolute(60f, 60f);
                    seal.Alignment = Element.ALIGN_RIGHT;
                    rightCell.AddElement(seal);
                }
                rightCell.AddElement(new Paragraph("Official Seal", contentFont) { Alignment = Element.ALIGN_RIGHT });

                table.AddCell(leftCell);
                table.AddCell(rightCell);
                doc.Add(table);

                doc.Add(new Paragraph("\n"));  // Bottom spacing

                doc.Close();
            }

            return filePath;
        }

        public static string GenerateCreatorCertificatePDF(User user)
        {
            string certFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "CreatorCertificates");
            if (!Directory.Exists(certFolder))
                Directory.CreateDirectory(certFolder);

            string fileName = $"Creator_{user.UserId}_CertifiedCertificate.pdf";
            string filePath = Path.Combine(certFolder, fileName);

            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                Rectangle customSize = new Rectangle(PageSize.A4.Width, 595f);
                Document doc = new Document(customSize, 40f, 40f, 40f, 40f);
                PdfWriter writer = PdfWriter.GetInstance(doc, fs);
                doc.Open();

                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 24, BaseColor.BLACK);
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, BaseColor.DARK_GRAY);
                var nameFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, BaseColor.BLUE);
                var contentFont = FontFactory.GetFont(FontFactory.HELVETICA, 13, BaseColor.BLACK);
                var footerFont = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 11, BaseColor.DARK_GRAY);

                doc.Add(new Paragraph("\n\n"));
                doc.Add(new Paragraph("Certificate of Recognition", titleFont) { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph("\n\n"));

                Paragraph content = new Paragraph { Alignment = Element.ALIGN_CENTER };
                content.Add(new Phrase("This is to certify that\n\n", contentFont));
                content.Add(new Phrase(user.Username + "\n\n", nameFont));
                content.Add(new Phrase("has been officially recognized as a\n\n", contentFont));
                content.Add(new Phrase("Certified Campaign Creator\n\n", headerFont));
                content.Add(new Phrase("on the FundHive Crowdfunding Platform.\n\n", contentFont));
                doc.Add(content);

                // Optional badge or icon
                string badgePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "FundHive_Certificate_MainLogo.JPG");
                if (File.Exists(badgePath))
                {
                    Image badge = Image.GetInstance(badgePath);
                    badge.ScaleAbsolute(70f, 70f);
                    badge.Alignment = Element.ALIGN_CENTER;
                    doc.Add(badge);
                }

                doc.Add(new Paragraph($"\nDate of Certification: {DateTime.Now:dd MMMM yyyy}", contentFont) { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph("FundHive - Crowdfunding Platform", footerFont) { Alignment = Element.ALIGN_CENTER });

                // FundHive logo at bottom
                //string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "FundHive_Certificate_Logo.JPG");
                //if (File.Exists(logoPath))
                //{
                //    Image logo = Image.GetInstance(logoPath);
                //    logo.ScaleAbsolute(100f, 100f);
                //    logo.Alignment = Element.ALIGN_CENTER;
                //    logo.SpacingBefore = 30f;
                //    doc.Add(logo);
                //}

                doc.Close();
            }

            return filePath;
        }


        //public static string GenerateCertificatePDF(User user, string badgeTitle, decimal totalContribution)
        //{
        //    string certFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Certificates");
        //    if (!Directory.Exists(certFolder))
        //        Directory.CreateDirectory(certFolder);

        //    string fileName = $"User_{user.UserId}_{badgeTitle}.pdf";
        //    string filePath = Path.Combine(certFolder, fileName);

        //    using (FileStream fs = new FileStream(filePath, FileMode.Create))
        //    {
        //        Document doc = new Document(PageSize.A4, 40, 40, 40, 40);
        //        PdfWriter writer = PdfWriter.GetInstance(doc, fs);
        //        doc.Open();

        //        // Border
        //        PdfContentByte cb = writer.DirectContent;
        //        cb.SetColorStroke(new BaseColor(212, 175, 55)); // ✅ Gold color defined manually
        //        cb.SetLineWidth(4f);
        //        cb.Rectangle(20, 20, doc.PageSize.Width - 40, doc.PageSize.Height - 40);
        //        cb.Stroke();

        //        // Fonts
        //        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 28, BaseColor.BLACK);
        //        var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, BaseColor.DARK_GRAY);
        //        var nameFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 22, BaseColor.BLUE);
        //        var contentFont = FontFactory.GetFont(FontFactory.HELVETICA, 14, BaseColor.BLACK);
        //        var footerFont = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 12, BaseColor.DARK_GRAY);

        //        // Title
        //        doc.Add(new Paragraph("🏆 Certificate of Achievement", titleFont) { Alignment = Element.ALIGN_CENTER });
        //        doc.Add(Chunk.NEWLINE);

        //        // Subtitle
        //        doc.Add(new Paragraph("This is to certify that", contentFont) { Alignment = Element.ALIGN_CENTER });
        //        doc.Add(Chunk.NEWLINE);

        //        // Username
        //        doc.Add(new Paragraph(user.Username, nameFont) { Alignment = Element.ALIGN_CENTER });
        //        doc.Add(Chunk.NEWLINE);

        //        // Achievement description
        //        doc.Add(new Paragraph($"has successfully achieved the", contentFont) { Alignment = Element.ALIGN_CENTER });
        //        doc.Add(new Paragraph($"{badgeTitle.ToUpper()} Badge", headerFont) { Alignment = Element.ALIGN_CENTER });
        //        doc.Add(new Paragraph($"With a total contribution of ₹{totalContribution}", contentFont) { Alignment = Element.ALIGN_CENTER });
        //        doc.Add(Chunk.NEWLINE);

        //        // Badge image
        //        string badgePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "RewardsImages", $"{badgeTitle.ToLower()}.png");
        //        if (File.Exists(badgePath))
        //        {
        //            Image badge = Image.GetInstance(badgePath);
        //            badge.ScaleAbsolute(80f, 80f);
        //            badge.Alignment = Element.ALIGN_CENTER;
        //            doc.Add(badge);
        //            doc.Add(Chunk.NEWLINE);
        //        }

        //        // Date
        //        doc.Add(new Paragraph($"Date of Achievement: {DateTime.Now:dd MMMM yyyy}", contentFont) { Alignment = Element.ALIGN_CENTER });
        //        doc.Add(Chunk.NEWLINE);

        //        // Footer Info
        //        doc.Add(new Paragraph("FundHive - Crowdfunding Platform", footerFont) { Alignment = Element.ALIGN_CENTER });
        //        doc.Add(Chunk.NEWLINE);

        //        // Signature and Seal
        //        PdfPTable table = new PdfPTable(2);
        //        table.WidthPercentage = 100;

        //        PdfPCell left = new PdfPCell();
        //        string signaturePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "signature.jpeg");
        //        if (File.Exists(signaturePath))
        //        {
        //            Image signature = Image.GetInstance(signaturePath);
        //            signature.ScaleToFit(100f, 50f);
        //            signature.Alignment = Element.ALIGN_LEFT;
        //            left.AddElement(signature);
        //        }
        //        left.Border = Rectangle.NO_BORDER;
        //        left.HorizontalAlignment = Element.ALIGN_LEFT;
        //        left.PaddingTop = 10f;
        //        left.AddElement(new Paragraph("Instructor's Signature", contentFont));

        //        PdfPCell right = new PdfPCell();
        //        string sealPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "FundHive_Certificate_Logo.JPG");
        //        if (File.Exists(sealPath))
        //        {
        //            Image seal = Image.GetInstance(sealPath);
        //            seal.ScaleToFit(70f, 70f);
        //            seal.Alignment = Element.ALIGN_RIGHT;
        //            right.AddElement(seal);
        //        }
        //        right.Border = Rectangle.NO_BORDER;
        //        right.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        right.PaddingTop = 10f;
        //        right.AddElement(new Paragraph("Official Seal", contentFont) { Alignment = Element.ALIGN_RIGHT });

        //        table.AddCell(left);
        //        table.AddCell(right);

        //        doc.Add(table);
        //        doc.Close();
        //    }

        //    return filePath;
        //}


    }
}
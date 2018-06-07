using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using XMLParsingToList.Models;
using Syncfusion.Pdf.Parsing;
using static AllStateBenefits.Models.RextandText;

namespace XMLParsingToList.Controllers
{
    public class MappingController : Controller
    {
        WritePDFFile objWriteFile = new WritePDFFile();
        public static string PDFPath = @"C:\Users\jag27\Documents\Personal\Infosys\AllState\Projects\XmlPdfMapper2\XmlPdfMapper\XmlPdfMapper\Assets\ABJ45A1AL.pdf";
        public static string XMLPath = @"C:\Users\jag27\Downloads\TestSample.xml";
        public string outputPdf = @"C:\Users\jag27\Documents\Personal\Infosys\AllState\Projects\XmlPdfMapper2\XmlPdfMapper\XmlPdfMapper\Assets\output.pdf";
        // GET: Mapping
        public ActionResult Index()
        {
            List<string> values = objWriteFile.ReadPdfFile(PDFPath);
            //objWriteFile.GetPDFValues(@"D:\Karthikeyan Documents\Sample.pdf");

            ViewBag.TextFound = values;
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(XMLPath);
            }
            catch (XmlException e)
            {
                throw e;
            }


            foreach (XmlNode node in xmlDoc)
            {
                if (node.NodeType == XmlNodeType.XmlDeclaration)
                {
                    xmlDoc.RemoveChild(node);
                }
            }
            StringWriter sw = new StringWriter();
            XmlTextWriter xw = new XmlTextWriter(sw);
            xmlDoc.WriteTo(xw);

            ViewBag.data = sw.ToString();

            return View();
        }

        public ActionResult GetXMLNodes()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SavePDFMapping(List<PDFMapping> PDFMapping)
        {


            return Json("Success", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult PreviewPDFWithMappedFields(List<PDFMapping> PDFMapping)
        {
            PdfReader reader = new PdfReader(PDFPath);
            using (PdfStamper stamper = new PdfStamper(reader, new FileStream(outputPdf, FileMode.Create)))
            {
                XfaForm form = new XfaForm(reader);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(XMLPath);

                //form.DatasetsNode.InnerXml = xmlDoc.InnerXml;

                form.FindDatasetsNode("Cell1").InnerXml = PDFMapping[0].XmlField;
                form.FindDatasetsNode("Cell2").InnerXml = PDFMapping[1].XmlField;
                form.FindDatasetsNode("Cell3").InnerXml = PDFMapping[2].XmlField;
                form.FindDatasetsNode("Cell5").InnerXml = PDFMapping[3].XmlField;

                form.Changed = true;

                XfaForm.SetXfa(form, stamper.Reader, stamper.Writer);
            }

            TempData["OutputPDF"] = outputPdf;

            return PartialView("_PreviewPDF");

            //PdfStamper stamper = new PdfStamper(reader, new FileStream(Temp_PDF_Copy_Path, FileMode.CreateNew, FileAccess.ReadWrite));
            //AcroFields form = stamper.AcroFields;
            //XfaForm xfa = form.Xfa;

            //XmlDocument xmldoc = new XmlDocument();
            //xmldoc.LoadXml(form.Xfa.DatasetsNode.FirstChild.OuterXml);

            ////Load the existing PDF document
            //Syncfusion.Pdf.Parsing.PdfLoadedXfaDocument loadedDocument = new PdfLoadedXfaDocument("input.pdf");
            ////Load the existing XFA form
            //PdfLoadedXfaForm loadedForm = loadedDocument.XfaForm;
            ////Get the loaded text box field.
            //PdfLoadedXfaTextBoxField loadedTextBox = (loadedForm.Fields["subform1[0]"] as PdfLoadedXfaForm).Fields["text[0]"] as PdfLoadedXfaTextBoxField;
            ////fill the text box
            //loadedTextBox.Text = "First Name";
            ////Save the document
            //loadedDocument.Save("output.pdf");
            ////Close the document
            //loadedDocument.Close();

            //FileStream pdfTemplate = new FileStream(PDFPath, FileMode.Open, FileAccess.Read);
            //FileStream xmlFormData = new FileStream(XMLPath, FileMode.Open, FileAccess.Read);
            //FileStream outputStream = new FileStream(outputPdf, FileMode.Create, FileAccess.Write);

            //using (iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(pdfTemplate))
            //{
            //    using (iTextSharp.text.pdf.PdfStamper stamper = new iTextSharp.text.pdf.PdfStamper(reader, outputStream))
            //    {
            //        stamper.Writer.CloseStream = false;
            //        stamper.AcroFields.Xfa.FillXfaForm(xmlFormData);
            //    }
            //}

            //outputStream.Close();
            //xmlFormData.Close();
            //pdfTemplate.Close();

            //pdfTemplate = null;
            //xmlFormData = null;
            //outputPdf = null;         
        }

        public ActionResult GetPDFPreview()
        {
            string file = TempData["OutputPDF"].ToString();
            FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);

            return File(fs, "application/pdf");
        }

        //    public ActionResult GetHTMLVersionOfPDF()
        //    {            
        //        using (Stream inputStream = File.OpenRead("../../data/notification.pdf"))
        //        {
        //            using (FixedDocument doc = new FixedDocument(inputStream))
        //            {
        //                // create output file
        //                using (TextWriter writer =
        //new StreamWriter(File.Create("c:/out.html"), Encoding.UTF8))
        //                {
        //                    Page page = doc.Pages[0];
        //                    // write returned html string to file
        //                    writer.Write(page.ConvertToHtml(TextExtractionOptions.HtmlPage));
        //                }
        //            }
        //        }

        //        Process.Start("c:/out.html");

        //        return View();
        //    }

        //[HttpPost]
        //public ActionResult XMLUpload(HttpPostedFileBase file)
        //{
        //    try
        //    {
        //        XmlDocument xmldoc = new XmlDocument();
        //        xmldoc.Load(@"D:\Karthikeyan Documents\testfile1.xml");
        //        PolicyDetailModel policyDetail;
        //        XmlNodeList usernodes = xmldoc.SelectNodes("/Users/User");
        //        foreach (XmlNode usr in usernodes)
        //        {
        //            policyDetail = new PolicyDetailModel();
        //            policyDetail.Id = Convert.ToInt32(usr["id"].InnerText);
        //            policyDetail.Name = usr["name"].InnerText;
        //            policyDetail.Gender = usr["gender"].InnerText;
        //            policyDetail.Mobile = usr["mobile"].InnerText;
        //            XmlNodeList addrNodes = usr.SelectNodes("address");
        //            foreach (XmlNode addrn in addrNodes)
        //            {
        //                policyDetail.StreetName = addrn["street"].InnerText;
        //                policyDetail.City = addrn["city"].InnerText;
        //                policyDetail.Pin = addrn["pincode"].InnerText;
        //            }
        //            TempData["policyData"] = policyDetail;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }


        //}
    }
}
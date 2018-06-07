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
using AllStateBenefits.Models;
using XMLParsingToList.Models;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Globalization;
using static AllStateBenefits.Models.RextandText;

namespace AllStateBenefits.Controllers
{
    public class MappingController : Controller
    {
        WritePDFFile objWriteFile = new WritePDFFile();
        //public static string PDFPath = @"C:\Users\jag27\Documents\Personal\Infosys\AllState\Projects\XmlPdfMapper2\XmlPdfMapper\XmlPdfMapper\Assets\ABJ45A1AL.pdf";
        public static string PDFPath = @"C:\Users\jag27\Documents\Personal\Infosys\AllState\Projects\XmlPdfMapper2\XmlPdfMapper\XmlPdfMapper\Assets\ABJ45A1AL EOI L70 PA - Dynamic XML Format-Save as PDF.pdf";
        public static string XMLPath = @"C:\Users\jag27\Documents\Personal\Infosys\AllState\Projects\XmlPdfMapper2\XmlPdfMapper\XmlPdfMapper\Assets\TestSample.xml";
        public static string outputPdf = @"C:\Users\jag27\Documents\Personal\Infosys\AllState\Projects\XmlPdfMapper2\XmlPdfMapper\XmlPdfMapper\Assets\SampleOutput.pdf";
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

            //THIS SHOULD BE REMOVED EVENTUALLY
            //I am assuming that this method is going to receive a list of string that represent Xpaths found in the PDF nodes' name attribute 

            List<string> paths = new List<string>() {
                "Case_Location_Address_City","Case_location_application_people_person_IsDependent","Case_location_application_people_person_employee_monthlysalary",
                "Case_Location_CallCenter"
            };

            List<string> xmlpath = new List<string>() {
                "Case_Location_Address_City","Case_Location_Application_People_Person_IsDependent","Case_Location_Application_People_Person_Employee_MonthlySalary",
                "Case_Location_CallCenter"
            };


            PdfReader reader = new PdfReader(PDFPath);
            using (PdfStamper stamper = new PdfStamper(reader, new FileStream(outputPdf, FileMode.Create)))
            {

                XfaForm form = new XfaForm(reader);

                XElement xmlDoc = XElement.Load(XMLPath);

                string xmllist = form.DomDocument.InnerXml;
                XDocument doc = XDocument.Parse(xmllist);


                for (var field = 0; field < paths.Count - 1; field++)
                {

                    //Steps to find the node from the data xml file using its xPath
                    var arr = xmlpath[field].Split('_');

                    var realPath = String.Join("/", arr);

                    XElement data = xmlDoc.XPathSelectElement("//" + realPath);

                    //Determine the type of input the 

                    XElement pdfNode = (from elem in doc.Descendants()
                                        where elem.Attribute("name") != null &&
                                        elem.Attribute("name").Value.Contains(paths[field])
                                        select elem).First();

                    string val;

                    if (pdfNode.Name.ToString().Contains("field"))
                    {
                        //Is the field node a checkbox
                        List<XElement> ckButton = (from elem in pdfNode.Descendants()
                                                   where elem.Name.ToString().Contains("checkButton")
                                                   select elem).ToList();
                        if (ckButton.Count > 0)
                        {
                            //Is the value stored in the xml data node written true/false(incorrect format) or 1/0 (correct format)
                            if (data.Value.ToUpper() == "FALSE") val = "0";
                            else if (data.Value.ToUpper() == "TRUE") val = "1";
                            else val = data.Value;
                        }

                        else// then it is a normal field
                        {
                            //Is the field node an numeric input or text input
                            List<XElement> numField = (from elem in pdfNode.Descendants()
                                                       where elem.Name.ToString().Contains("numericEdit")
                                                       select elem).ToList();

                            //Im not sure what to put in here, bc I am able to fill in numeric fields without any issues
                            if (numField.Count > 0) //Its a numeric field
                            {

                            }

                            else //Its a text field
                            {

                            }

                            val = data.Value;

                        }

                    }
                    else //then it must be a exclGroup
                    {
                        //I need to think about this more, since this node is an exclGroup and not a field node
                        //I have to match of the xml node value with the fields stored in the exclGroup
                        val = data.Value;
                    }

                    form.FindDatasetsNode(paths[field]).InnerXml = val;

                }

                form.Changed = true;



                XfaForm.SetXfa(form, stamper.Reader, stamper.Writer);
            }

            return Json("success", JsonRequestBehavior.AllowGet);
            //TempData["OutputPDF"] = outputPdf;

            //return PartialView("_PreviewPDF");

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

        public ActionResult PreviewMappedPDF()
        {
            return View();
        }

        public ActionResult GetPDFPreview()
        {
            string file = outputPdf;
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
using System;
using System.IO;
using System.Text.RegularExpressions;
using HtmlAgilityPack; 

namespace ConsoleApplication
{
    public class Program
    {

        public static void parse_ngElements(HtmlDocument html, string tagName, string attribute) // HtmlNodeCollection nodes)
        {
            var nodes = html.DocumentNode.SelectNodes(tagName);
            if (nodes != null)
            {
                foreach (HtmlNode item in nodes)
                {
                    if (item.HasAttributes)
                    {
                        var attributes = item.Attributes;
                        foreach (HtmlAttribute attrib in attributes)
                        {
                            if (attrib.Name.Contains(attribute))
                            {
                                foreach (Match match in Regex.Matches(attrib.Value, "\'([^\"]*)\'"))
                                {
                                    Console.WriteLine(match.ToString());
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void parse_ngContents(HtmlDocument html, string tagName, string attribute) // HtmlNodeCollection nodes)
        {
            var nodes = html.DocumentNode.SelectNodes(tagName);
            if (nodes != null)
            {
                foreach (HtmlNode item in nodes)
                {
                    if (item.HasAttributes)
                    {
                        var attributes = item.Attributes;
                        foreach (HtmlAttribute attrib in attributes)
                        {
                            // Hack to break out of messages/message containment
                            if (attrib.Name.Contains(attribute+'s')) {
                                continue;
                            }
                            if (attrib.Name.Contains(attribute)) {
                                // Skip the ones with code inside them. 
                                MatchCollection matches = Regex.Matches(item.InnerText, "{{([^\"]*)}");
                                if (matches.Count == 0) {
                                    // Trim the CR and LF too
                                    Console.WriteLine(item.InnerText.Trim().Trim('\r', '\n'));
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void parse_htmlElements(HtmlDocument html, string tagName)
        {
            var nodes = html.DocumentNode.SelectNodes(tagName);
            if (nodes != null)
            {
                foreach (HtmlNode item in nodes)
                {
                    // Dump from the child nodes in case of embedded SPAN tags
                    foreach (HtmlNode child in item.ChildNodes) {
                        // Skip the ones with code inside them. 
                        MatchCollection matches = Regex.Matches(child.InnerText, "{{([^\"]*)}");
                        if (matches.Count == 0) {
                            var cleanString = child.InnerHtml.Trim().TrimEnd(':');
                            if(cleanString.Contains("&nbsp;")) {
                                Console.WriteLine("*** WARNING: Following String had non-breaking spaces");
                                cleanString = cleanString.Replace("&nbsp;", " ");
                            }
                            Console.WriteLine(cleanString);
                        }
                    }
                }
            }
        }


        public static string[] finance_files = new string[] {
                "ExpenseClaims.aspx",
                "FinanceApprovals.aspx",
                "FinancialAnalysis.aspx",
                "PaymentRequisitionDetail.aspx",
                "PaymentRequisitions.aspx",
                "PurchaseInvoiceDetail.aspx",
                "PurchaseInvoices.aspx",
                "PurchaseOrderDetail.aspx",
                "PurchaseOrders.aspx",
                "PurchaseReceipt.aspx",
                "PurchaseRequisitionDetail.aspx",
                "PurchaseRequisitions.aspx"
        };

        public static void Main(string[] args)
        {
            try {
                // The @ prefix says this is a verbatim string, so we don't have to escape the \ character 
                // string[] lines = System.IO.File.ReadAllLines(@"D:\projects\csharp\finance\PaymentRequisitionDetail.aspx");
                // var srcfile = new XDocument(lines);
                foreach(string fileName in finance_files) {
                    Console.WriteLine(" >>> Parsing {0}.", fileName);

//                    FileStream fs = File.OpenRead(@"D:\projects\csharp\finance\PaymentRequisitionDetail.aspx");
                    FileStream fs = File.OpenRead(@"D:\projects\csharp\finance\"+fileName);

                    var html = new HtmlDocument();
                    html.Load(fs);

                    // Pull out some known sections with strings
                    parse_ngElements(html, "//h1", "data-ng-bind");
                    parse_ngElements(html, "//h2", "data-ng-bind");
                    parse_ngElements(html, "//h3", "data-ng-bind");
                    parse_ngElements(html, "//h4", "data-ng-bind");

                    parse_ngElements(html, "//span", "data-ng-bind");
//                    parse_ngElements(html, "//span", "class=\"collapsepanel_head_title\"");

                    parse_ngContents(html, "//div", "ng-message");
                    
                    parse_htmlElements(html, "//label");
                    parse_htmlElements(html, "//th");
                    parse_htmlElements(html, "//p");
                }
                
            } 
            catch (FileNotFoundException) {
                Console.WriteLine("Hey! Something went wrong. Is the source file present in this directory?");   
            } catch (IOException) {
                Console.WriteLine("Hey! Something went wrong with file IO. I don't know what.");   
            }
        }
    }
}

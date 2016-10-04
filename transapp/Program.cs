using System;
using System.IO;
using System.Text.RegularExpressions;
using HtmlAgilityPack; 

namespace ConsoleApplication
{
    public class Program
    {

        public static string reportStaticString(string result)
        {
            string constantID = null;

            // Empty strings or one-character strings are not useful 
            if (result.Length > 1)
            {
                // &nbsp by itself is trash
                if ((result.Length == 5) && result.Contains("&nbsp")) {
                    return null;
                } 
                // If we've translated it already, we can return
                if (result.Contains("span translate=")) {
                    return null;
                }

                // Replace embedded &nbsp; with real spaces
                if(result.Contains("&nbsp;")) {
                    Console.WriteLine("*** WARNING: Following String had non-breaking spaces");
                    result = result.Replace("&nbsp;", " ");
                }

                // This is supposed to eliminate code blocks. 
                // TODO: It seems not to work.
                MatchCollection matches = Regex.Matches(result, "\\{\\{([^\"]*)}}");
                if (matches.Count == 0) {
                    // To make the constant, trim  spaces and trailing periods. 
                    constantID = result.Trim().TrimEnd('.').ToUpper();
                    // Then replace embedded spaces with underscores
                    if (constantID.Contains(" ")) {
                        constantID = constantID.Replace(" ", "_");
                    }
                    // Write the entry in the EN-US.lang style
                    Console.WriteLine("  \"{0}\" : \"{1}\",", constantID, result);
                }
            }
            return constantID;
        }

        public static void rewrite_ngBlock(HtmlNode item, string constantID) {
            // Stub for eventual replacement code. This needs to create span elements and so on.
        }

        public static void rewrite_basicTag(HtmlNode item, string constantID) {
            // Stub for eventual replacement code. Simple translate="" stub
        }

        /* This method finds strings that are hiding inside Angular elements like data-ng-bind.
           Replacement of the strings will require <span> tags and other nonsense.
           */
        public static void parse_ngElements(HtmlDocument html, string tagName, string attribute)
        {
            // Get all of the nodes matching the selected HTML tag type
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
                            // find elements that have the desired attribute
                            if (attrib.Name.Contains(attribute))
                            {
                                // The strings will occur inside single quotes 'like this'
                                // As written the outermost single quotes are matched, this can fail
                                // if there are single quotes embedded within the string. A more
                                // powerful regex could likely handle this but it's not worth the effort.
                                foreach (Match match in Regex.Matches(attrib.Value, "\'([^\"]*)\'"))
                                {
                                    // Strip off the singel quotes before passing down
                                    string constant = match.ToString();
                                    constant = constant.Substring(1,constant.Length-2);
                                    // Report the string
                                    string constantID = reportStaticString(constant.Trim());
                                    // TODO: item may be the wrong thing to pass here, or may need more context?
                                    rewrite_ngBlock(item, constantID); 
                                }
                            }
                        }
                    }
                }
            }
        }

        /* This method finds strings that are hiding inside Angular elements like ng-message.
           Replacement of the strings should be straightforward.
           */
        public static void parse_ngContents(HtmlDocument html, string tagName, string attribute)
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
                                MatchCollection matches = Regex.Matches(item.InnerText, "{{([^\"]*)}}");
                                if (matches.Count == 0) {
                                    // Trim the CR and LF too
                                    string constantID = reportStaticString(item.InnerText.Trim().Trim('\r', '\n'));
                                    // TODO: item may be the wrong thing to pass here, or may need more context?
                                    rewrite_basicTag(item, constantID); 
                                }
                            }
                        }
                    }
                }
            }
        }

        /* This method finds strings that are hiding inside bsaic HTML elements.
           Replacement of the strings should be straightforward.
           */
        public static void parse_htmlElements(HtmlDocument html, string tagName, Boolean mustBeLeaf = false, string classType = null)
        {
            var nodes = html.DocumentNode.SelectNodes(tagName);
            if (nodes != null)
            {
                foreach (HtmlNode item in nodes)
                {
                    // Dump from the child nodes in case of embedded SPAN tags
                    foreach (HtmlNode child in item.ChildNodes) {
                        
                        // If requested, skip elements with children
                        if (mustBeLeaf) {
                            if (child.ChildNodes.Count > 1) {
                                continue;
                            }
                        }
                        // If it's commented out, bail
                        if( child.InnerText.Contains("<!--") || child.InnerText.Contains("<%--")) {
                            continue;
                        }

                        // Skip the ones with code inside them. 
                        MatchCollection matches = Regex.Matches(child.InnerText, "{{([^\"]*)}}");
                        if (matches.Count == 0) {
                            string constantID = reportStaticString(child.InnerText.Trim().TrimEnd(':'));
                            // TODO: item may be the wrong thing to pass here, or may need more context?
                            rewrite_basicTag(item, constantID); 
                        }
                    }
                }
            }
        }

        public static string[] finance_files = new string[] {
/*            
                "ExpenseClaims.aspx",
                "FinanceApprovals.aspx",
                "FinancialAnalysis.aspx",
*/                
                "PaymentRequisitionDetail.aspx",
                "PaymentRequisitions.aspx",
/*                
                "PurchaseInvoiceDetail.aspx",
                "PurchaseInvoices.aspx",
                "PurchaseOrderDetail.aspx",
                "PurchaseOrders.aspx",
                "PurchaseReceipt.aspx",
                "PurchaseRequisitionDetail.aspx",
*/                
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

                    parse_ngContents(html, "//div", "ng-message");
                    
                    parse_htmlElements(html, "//label");
                    parse_htmlElements(html, "//th");
                    parse_htmlElements(html, "//p");
                    parse_htmlElements(html, "//footer");
                    parse_htmlElements(html, "//a");
                    parse_htmlElements(html, "//h1");
                    parse_htmlElements(html, "//h2");
                    parse_htmlElements(html, "//h3");
                    parse_htmlElements(html, "//button");
                    parse_htmlElements(html, "//li", true);
                    parse_htmlElements(html, "//td", true);
                  
                    // These two made a lot of false positives.
//                    parse_htmlElements(html, "//h4");
//                    parse_htmlElements(html, "//div", true);
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

using System;
using System.IO;
using System.Text;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 1 ) {
                Console.WriteLine("Usage: Single argument (required) is base of LANG file name.");
                Environment.Exit(0);      
            }
            Console.WriteLine("Transforming LANG file " + args[0] + " to CSV.");

            try {
                string[] lines = System.IO.File.ReadAllLines(args[0] + ".lang");

                StringBuilder builder = new StringBuilder();

                // Column headers help the translator know what to do.
                builder.AppendLine("Key,EN_US String,FR_CA String");

                foreach (string line in lines)
                {
                    var temp = line.Split(':');

    // TODO: Handle colons in the English input string
                    if (temp.Length > 2) {
                        Console.WriteLine("Colon in translation string, possible error in CSV file.");
                    }

                    if (temp.Length > 1) {
                        if( temp[1].Trim().Equals("{")) {
                            // Skip for now. Possible to comment or group?
                            // See https://github.com/angular-translate/angular-translate/blob/master/demo/l10n/en.json 
                        }
                        else {
                            // Export this line to the CSV file for the translator.
                            // Question: Where did the "s go? Did Join() eat them?
                            // Console.WriteLine("Dumping line for "+temp[0].Trim());
                            builder.AppendLine(string.Join(",", temp[0].Trim(), temp[1].Trim(), " "));
                        }
                    }
                }
                // Dump all collected lines to the CSV file. 
                try {
                    System.IO.File.WriteAllText(args[0]+ ".csv", builder.ToString());
                } catch (IOException) {
                    // Probably file in use. close it!
                    Console.WriteLine("Hey! Something went wrong. If Excel has the file "+args[0]+ ".csv open, please close it.");
                }
            } catch (FileNotFoundException) {
                Console.WriteLine("Hey! Something went wrong. Is "+args[0] + ".lang present in this directory?");   
            } catch (IOException) {
                Console.WriteLine("Hey! Something went wrong with file IO. I don't know what.");   
            }
            
        }
    }
}

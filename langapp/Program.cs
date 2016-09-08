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
                Console.WriteLine("Usage: Single argument (required) is base of CSV file name.");
                Environment.Exit(0);      
            }
            Console.WriteLine("Transforming CSV file " + args[0] + " to fr_CA.lang.");

            try {
                string[] lines = System.IO.File.ReadAllLines(args[0] + ".csv");

                StringBuilder builder = new StringBuilder();

                // Start with open parenthesis
                builder.AppendLine("{");

                foreach (string line in lines)
                {
                    var temp = line.Split(','); // CSV file, duh

    // TODO: Handle commas in the English or French columns
                    if (temp.Length > 4) {
                        Console.WriteLine("Commas in translation string, possible error in CSV file.");
                        // Skip this entry
                        continue;
                    }

                    if (temp.Length > 2) {
                        if( temp[0].Trim().Equals("Key")) {
                            // Skip the header row. Is there a cleaner way to skip the first line?
                            continue;
                        }
                        else {
                            // Export the key and the french text to the CSV file.
                            // Console.WriteLine("Dumping line for "+temp[0].Trim()+);
                            builder.AppendLine("    \""+temp[0].Trim()+"\" , \""+temp[2].Trim()+"\",");
                        }
                    }
                }
                // End  with close parenthesis
                builder.AppendLine("}");

                // Dump all collected lines to the LANG file. 
                try {
                    System.IO.File.WriteAllText("fr_CA.lang", builder.ToString());
                } catch (IOException) {
                    // Probably file in use. close it!
                    Console.WriteLine("Hey! Something went wrong. If your editor has the file fr_CA.lang open, please close it.");
                }
            } catch (FileNotFoundException) {
                Console.WriteLine("Hey! Something went wrong. Is "+args[0] + ".csv present in this directory?");   
            } catch (IOException) {
                Console.WriteLine("Hey! Something went wrong with file IO. I don't know what.");   
            }
        }
    }
}

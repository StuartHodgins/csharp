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
            Console.WriteLine("Transforming CSV file {0} to fr_CA.lang.", args[0]);

            try {
                string[] lines = System.IO.File.ReadAllLines(args[0] + ".csv");

                StringBuilder builderEN = new StringBuilder();
                StringBuilder builderFR = new StringBuilder();

                // Start with open parenthesis
                builderEN.AppendLine("{");
                builderFR.AppendLine("{");

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
                            builderEN.AppendLine("    \""+temp[0].Trim()+"\" : \""+temp[1].Trim()+"\",");
                            builderFR.AppendLine("    \""+temp[0].Trim()+"\" : \""+temp[2].Trim()+"\",");
                        }
                    }
                }
                // End with close parenthesis
                builderEN.AppendLine("}");
                builderFR.AppendLine("}");

                // Dump all collected lines to the LANG files. 
                try {
                    // Lang 1 (English)
                    System.IO.File.WriteAllText("en_US1.lang", builderEN.ToString(), Encoding.Unicode);
                } catch (IOException) {
                    // Probably file in use. close it!
                    Console.WriteLine("Hey! Something went wrong. If your editor has the file en_US1.lang open, please close it.");
                }
                try {
                    // Lang 1 (Cdn French)
                    System.IO.File.WriteAllText("fr_CA1.lang", builderFR.ToString(), Encoding.Unicode);
                } catch (IOException) {
                    // Probably file in use. close it!
                    Console.WriteLine("Hey! Something went wrong. If your editor has the file fr_CA1.lang open, please close it.");
                }
            } 
            catch (FileNotFoundException) {
                Console.WriteLine("Hey! Something went wrong. Is {0}.csv present in this directory?", args[0]);   
            } catch (IOException) {
                Console.WriteLine("Hey! Something went wrong with file IO. I don't know what.");   
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Xml;

namespace ConsoleApplication
{
    // DayOneEntry: Simple object for adding to a sortable List
    // Holds the entry date and the text 
    public class DayOneEntry : IComparable<DayOneEntry> {
        public String date {get; set;}
        public String text {get; set;} 

        public DayOneEntry(String d, String t)
        {
            date = d;
            text = t;
        }

        public override string ToString()
        {
            return "Date: " + date + "   Text: " + text;
        }

        //  Comparator for DayOneEntry type.
        public int CompareTo(DayOneEntry doe)
        {
                // We only care about the date for sorting
                return date.CompareTo(doe.date);
        }
    }
        
    public class Program
    {
        // Where did I stash the DayOne backups?
        static string dayonedir = @"D:\kelly_iphone_media\com.dayonelog.dayoneiphone\Library\Application Support\Journal.dayone\entries\";
        // We need to pass XmlReader a settings object so it will let us parse the files
        static XmlReaderSettings mySettings = new XmlReaderSettings();
        
        public static void Main(string[] args)
        {
            // Make a list so we can sort them before we output them
            List<DayOneEntry> entries = new List<DayOneEntry>();

            // Do the file handling in a try jsut in case of file problems
            try
            {
                // Settings object tells XmlReader that it's OK to parse the files
                mySettings.DtdProcessing = DtdProcessing.Ignore;

                foreach (string file in Directory.EnumerateFiles(dayonedir))
                {
                    using (FileStream fs = File.OpenRead(file))
                    {
                        using (XmlReader entry = XmlReader.Create(fs, mySettings))
                        {
                            try {
                                entries.Add(readEntry(entry));
                            }
                            catch(Exception ex) {
                                Console.WriteLine("Error in file " + file);
                                Console.WriteLine(ex);
                            }
                        }
                    }
                }
            }
            // These don't seem to be needed with my test data
            catch (ArgumentNullException)
            {
                Console.WriteLine("Hey! Something went wrong.");
            }
            catch (SecurityException)
            {
                Console.WriteLine("Hey! Something went wrong.");
            }

            // We're done, output the results
            produceList(entries);
        }

        // Parsing method for a DayOne entry file
        private static DayOneEntry readEntry(XmlReader entry) {
            // Find the date element, should always be first
            entry.ReadToFollowing("date");
            String date = entry.ReadElementContentAsString();

            // Sometimes the first key element is the one we want, sometimes it is the second one
            // The single-entry versions seem to be only those  from 2013.
            entry.ReadToNextSibling("key");
            if (entry.ReadElementContentAsString() != "Entry Text")
            {
                // It wasn't the first key, try again
                entry.ReadToNextSibling("key");
                if (entry.ReadElementContentAsString() != "Entry Text")
                {
                    // It wasn't the second key either, give up
                    throw new Exception("ERROR: Could not find 'Entry text'!");
                }
            }
            // If we got here then we found the Entry Text key, the next element is the actual text
            entry.ReadToFollowing("string");

            // Return a new DayOneEntry
            return new DayOneEntry(date, entry.ReadElementContentAsString());
        }

        // Sort the list and then output the entries to a text file
        private static void produceList(List<DayOneEntry> entries)
        {
            // A separator line to chunk up the output
            string sep_line = "------------------------------------";

            entries.Sort();

            using (var recoveryFile = System.IO.File.Create("d1_recover.txt"))
            {
                using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(recoveryFile))
                {
                    foreach (DayOneEntry doe in entries)
                    {
                        file.WriteLine(doe.date);
                        file.WriteLine(doe.text);
                        file.WriteLine(sep_line);
                    }
                }
            }
        }
    }
}

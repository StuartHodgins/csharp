using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Xml;

namespace ConsoleApplication
{
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

        // Default comparer for DayOneEntry type.
        public int CompareTo(DayOneEntry doe)
        {
                // TODO write the comparator
                return date.CompareTo(doe.date);
        }
    }
        
    public class Program
    {
        static string dayonedir = @"D:\kelly_iphone_media\com.dayonelog.dayoneiphone\Library\Application Support\Journal.dayone\entries\";
        static XmlReaderSettings mySettings = new XmlReaderSettings();
        static string sep_line = "------------------------------------";
        
        public static void Main(string[] args)
        {
            List<DayOneEntry> entries = new List<DayOneEntry>();
            try {
                // Tell XmlReader that it's OK to parse the files
                mySettings.DtdProcessing = DtdProcessing.Ignore;

                foreach (string file in Directory.EnumerateFiles(dayonedir))
                {
                   FileStream fs = File.OpenRead(file);
                   using (XmlReader entry = XmlReader.Create(fs, mySettings))
                   {
                        entry.ReadToFollowing("date");
                        String date = entry.ReadElementContentAsString();
//                        Console.WriteLine("Date: " + date);
                        // Sometimes the first key is the one we want, sometimes it is the second one
                        entry.ReadToNextSibling("key");
                        if( entry.ReadElementContentAsString() != "Entry Text" ) {
                            // It wasn't the first key
                            entry.ReadToNextSibling("key");
                            if( entry.ReadElementContentAsString() != "Entry Text" ) {
                                // It wasn't the second key either
                                Console.WriteLine("ERROR: Unexpected data!");
                                continue;
                            }
                        }
                        // If we got here we found the Entry Text, the next element is the entry text
                        entry.ReadToFollowing("string");
                        String text = entry.ReadElementContentAsString();
//                        Console.WriteLine("Data: " + text);
                        entries.Add(new DayOneEntry(date,text));
                   }
                }
           }
           catch (ArgumentNullException) {
                Console.WriteLine("Hey! Something went wrong.");   
           } catch (SecurityException) {
                Console.WriteLine("Hey! Something went wrong.");   
           }

           // Output the sorted List
           entries.Sort();
           
           // Output file

            var  recoveryFile = System.IO.File.Create("d1_recover.txt");
            using (System.IO.StreamWriter file = 
                new System.IO.StreamWriter(recoveryFile))
                {
                    foreach( DayOneEntry doe in entries) {
                        Console.WriteLine("Date: " + doe.date);
                        file.WriteLine("Date: " + doe.date);
                        Console.WriteLine("Text: " + doe.text);
                        file.WriteLine("Text: " + doe.text);
                        Console.WriteLine(sep_line);
                        file.WriteLine(sep_line);
                    }
                }
        }
    }
}

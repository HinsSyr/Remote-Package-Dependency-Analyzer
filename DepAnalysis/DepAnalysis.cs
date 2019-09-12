///////////////////////////////////////////////////////////////////////////
// DepAnalysis.cs       -- Analysis the dependency between files         //                               
// version              -- 1.0                                           // 
// Language             -- C#, .Net Framework 4.6.1                      // 
// Platform             -- Dell XPS2018, WIN10, VS2017 Community         // 
// Application          -- CSE681 Project#3 Homework                     // 
// Author               -- BO QIU , Master in Electrical Engineering,    //
//                         Syracuse University                           //
//                         (315) 278-2362, bqiu03@syr.edu                //
///////////////////////////////////////////////////////////////////////////
/*
 * Module Operations
 * ======================
 * By using the Typetable, this program analyzes the dependency between
 * files, and output a relationship of files
 * 
 * Public Interface
 * ======================
 * show_gra_depent()            // A method for showing the dependency
 * show_rela_result()           // The method by recalling container to output the result of dependency
 * match()                     // Scan the all files and extract tokens, and compare to the Typetable
 * 
 * 
 */

/* Build Process
 * ======================
 * Required Files:
 *   FileMg.cs   Toker.cs    TypeAnalysis.cs  TypeTable.cs
 *   
 * Compiler Command:
 *   csc /target:exe DepAnalysis.cs   FileMg.cs   Toker.cs    TypeAnalysis.cs  TypeTable.cs
 *   
 * Maintenance History
 * ======================
 * ver1.0 : 31 October 2018
 * - first release
 * 
 * Planned Modifications:
 * ----------------------
 * - 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CStoker;
using CodeAnalysis;
using TypeTableNS;
using FileMgNS;

namespace DepAnalysisNS
{
    //Define the DepAnalysis class for upcoming using
     public class DepAnalysis
    {
        CToker toker;
        TestParser testfile = new TestParser();
        TypeTable tt;

        // This is a Dictionary type for sacving the dependency of files
       public Dictionary<String, List<String>> depentable { set; get; }

        // show the dependency in a graph
        public void show_gra_depent()
        {
            Console.WriteLine("----------------------------------------------------------");
            Console.WriteLine("This is the Dependency Graph of all processing files  ");
              
            foreach (var ele in depentable)
            {
                Console.Write("\n  {0}", ele.Key);
                foreach (var item in ele.Value)
                    Console.Write("\n    [{0}]", item);
            }
            Console.WriteLine("\n---------------------------------------------------------");

        }

        // output the result of analyzing the dependency
        public void show_rela_result()
        {
            Console.WriteLine("\n\n");
            Console.WriteLine("\n This is the Dependency Relationship between all processing files");
            Console.WriteLine("--------------------------------------------------------------------");
            if (depentable.Keys.Count != 0)
            {
                foreach (var ele in depentable)
                    foreach (var item in ele.Value)
                        Console.WriteLine("{0,20}       depend on      {1,20}", ele.Key, item);
            }
            else
                Console.WriteLine("this file doesnt depend on any");
            Console.WriteLine("\n---------------------------------------------------------");

        }
        // a method to add a pair of file names into the container
        public void add(String key, String value)
        {
            if (depentable.ContainsKey(key) )
            {
                if(!depentable[key].Contains(value))
                depentable[key].Add(value);
            }
            else
            {
                List<String> temp = new List<String>();
                temp.Add(value);
                depentable.Add(key, temp);
            }
        }

        // match the toker and Typetable 
        public void match(string[] args)
        {
            tt = new TypeTable();
            tt = tt.getTypeTable(args);
            depentable = new Dictionary<String, List<String>>();
            foreach (string file in args)
            {
                toker = new CToker();
                if (!toker.openFile(file as string))
                {
                    Console.Write("\n  Can't open {0}\n\n", args[0]);
                    return;
                }
                String tok = "";
                while ( (tok=toker.getTok()) != "")
                {
                    if (tok != "\n")
                    {
                        if (tt.table.ContainsKey(tok))
                        {
                            foreach (var ele in tt.table)
                            {
                                if (ele.Key == tok)
                                {
                                    foreach (var item in ele.Value)
                                    {
                                        if (item.file == Path.GetFileName(file) )
                                        {
                                            add(item.file,item.file);
                  
                                        }
                                        else
                                            add(Path.GetFileName(file), item.file);
                                    }
                                }
                            }
                        }
                    }
                }
                toker.close();
            }
        }

        public string dep_tostring()        {            string dep_table = "";            foreach (var item in depentable)            {                dep_table = dep_table + " \n ====================";                dep_table = dep_table + " \n" + item.Key;                dep_table = dep_table + " Dependency: ";                dep_table = dep_table + "\n ";                foreach (var elem in item.Value)                {                    dep_table = dep_table + "\n" + item.Key + " depends on " + elem;                }                dep_table = dep_table + " \n ====================";            }            return dep_table;        }


    }
    // this is the test sub 
    class DepAnalysisTest
    {
#if (Test_DepAnalysis)
        static void Main(string[] args)
        {
            FileMg get_cs = new FileMg();
            string[] all_files = get_cs.find_solu_all_cs(get_cs.get_solu_path());

            DepAnalysis depana = new DepAnalysis();
            depana.match(all_files);
            depana.show_gra_depent();
            depana.show_rela_result();
        }

        }
#endif
 }

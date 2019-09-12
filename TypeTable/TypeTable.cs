///////////////////////////////////////////////////////////////////////////
// TypeTable.cs         -- save the type in a table of all files         //                               
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
 * By using TypeAnalysis to make a TypeTable for later using
 * 
 * Public Interface
 * ======================
 * show()                        // show the TypeTable
 * TypeTable getTypeTable()      // get back TypeTable for later using
 * 
 * 
 */

/* Build Process
 * ======================
 * Required Files:
 *   FileMg.cs   Semi.cs    TypeAnalysis.cs  Display.cs
 *   
 * Compiler Command:
 *   csc /target:exe TypeTable.cs   Display.cs   FileMg.cs    TypeAnalysis.cs  Semi.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CodeAnalysis;
using CSsemi;
using FileMgNS;

namespace TypeTableNS
{
    /////////////////////////////////////////////////////////
    // Typetable class 
    using File = String;
    using Type = String;
    using Namespace = String;

    public struct TypeItem
    {
        public File file;
        public Namespace namesp;
    }

    public class TypeTable
    {
        public  Dictionary<File, List<TypeItem>> table { get; set; } =
          new Dictionary<File, List<TypeItem>>();
        
        // add an element to the Typetable
        public void add(Type type, TypeItem ti)
        {
            if (table.ContainsKey(type))
                table[type].Add(ti);
            else
            {
                List<TypeItem> temp = new List<TypeItem>();
                temp.Add(ti);
                table.Add(type, temp);
            }
        }
        public void add(Type type, File file, Namespace ns)
        {
            TypeItem temp;
            temp.file = file;
            temp.namesp = ns;
            add(type, temp);
        }

        // output the typetable
        public void show()
        {
            Console.WriteLine(" This is the collection of TypeTable of Above files ");
            Console.WriteLine(" ---------------------------------------------------");
            foreach (var elem in table)
            {
                Console.Write("\n  {0}", elem.Key);
                foreach (var item in elem.Value)
                {
                    Console.Write("\n    [{0}, {1}]", item.file, item.namesp);
                }
            }
            Console.Write("\n");
        }
        // get back the Tyeptable for other program using
        public TypeTable getTypeTable(string[] args)
        {
            TestParser tp = new TestParser();
            TypeTable tt = new TypeTable();
            string ns = "";
            foreach (string file in args)
            {
                CSsemi.CSemiExp semi = new CSsemi.CSemiExp();
                semi.displayNewLines = false;
                if (!semi.open(file as string))
                {
                    Console.Write("\n  Can't open {0}\n\n", args[0]);
                }

            
                BuildCodeAnalyzer builder = new BuildCodeAnalyzer(semi);
                Parser parser = builder.build();

                try
                {
                    while (semi.getSemi())
                        parser.parse(semi);
                }
                catch (Exception ex)
                {
                    Console.Write("\n\n  {0}\n", ex.Message);
                }
                Repository rep = Repository.getInstance();
                List<Elem> table = rep.locations;
              
                foreach (Elem e in table)
                {
                 
                    if (e.type == "namespace")
                        ns = e.name;
                    if (e.type == "interface" || e.type == "class" || e.type == "struct" || e.type == "enum" || e.type == "delegate")
                        tt.add(e.name, Path.GetFileName(file), ns);
                }

                semi.close();
            }
            return tt;
        }
        public string tt_to_string()
        {
            string result = "";
          
            result = result + "\n\n";
            foreach (var elem in table)
            {
            
                result = result + elem.Key;
                result = result + "\n";
                foreach (var value in elem.Value)
                {
                  
                    result = result + "\n [ " + value.file + " , " + value.namesp  + " ] ";
                }
                result = result + "\n====================================\n\n";
               
            }
            
            return result;
        }
    }

    


    public class Test_table
    { 

        //----< Test Stub >--------------------------------------------------

#if (TEST_TypeTable)

       static void Main(string[] args)
    {
            FileMg get_cs = new FileMg();
            string[] all_files = get_cs.find_solu_all_cs(get_cs.get_solu_path());

            TestParser tp = new TestParser();
            TypeTable tb = new TypeTable();
            tb=tb.getTypeTable(all_files);
            tb.show();
        }
#endif
    }
}   


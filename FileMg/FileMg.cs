///////////////////////////////////////////////////////////////////////////
// FileMg.cs            -- manage the input files for convenient using   //                               
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
 * In order to not using command line and for convenient using in the 
 * later recalling
 * 
 * Public Interface
 * ======================
 * get_solu_path()            // method for getting the path of solution
 * find_solu_direct()           // method to get all directory in the solution
 * find_solu_all_cs()                   // method to get all .cs format files in the solution
 * 
 * 
 */

/* Build Process
 * ======================
 * Required Files:
 *   
 *   
 * Compiler Command:
 *   csc /target:exe FileMg.cs  
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

namespace FileMgNS
{
    public class FileMg
    {
        public string solution_path { get; set; }  //store the solution path

        // find the solution root path  ../../project3
        public string get_solu_path()
        {
            solution_path = Path.GetFullPath("../../../");
            return solution_path;
        }
      
        // find the all .cs files in the solution
        public string[] find_solu_all_cs(string path)
        {
            List<string> all_cs_list = new List<string>();

            string[] all_entries = Directory.GetFileSystemEntries(path);
            foreach (var file in all_entries)
            {
                string extesion = Path.GetExtension(file);
                if (extesion == ".cs")
                    all_cs_list.Add(file);
            }

            string[] solu_dics = Directory.GetDirectories(path);
            foreach (var dics in solu_dics)
            {
                string[] files = Directory.GetFileSystemEntries(dics);
                foreach (var file in files)
                {
                    string extension = Path.GetExtension(file);
                    if (extension == ".cs")
                        all_cs_list.Add(file);
                }

            }
            return all_cs_list.ToArray();
        }
    }
    // Test sub
    class test
#if (Test_Filemg)
    {
        static void Main(string[] args)
        {
            FileMg testfg = new FileMg();
            string path = testfg.get_solu_path();
            string[] all_cs_file = testfg.find_solu_all_cs(path);
            Console.WriteLine("\n\n");
            Console.WriteLine(" ------------------Below is the whole .cs files in the solution-----------------");
           Console.WriteLine("\n\n");

         

            foreach (var test in all_cs_file)
                Console.WriteLine(test);
        }
    }
#endif
}

///////////////////////////////////////////////////////////////////////////
// AutoTestUnit.cs      -- Test the requirements of project 3            //                               
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
 * Test all requirements.
 * 
 * Public Interface
 * ======================
 * FileMg()                    // Test FileMg()   function
 * typeanalysier()             // Test typeanalysier() function
 * TypeTable()                 // Test TypeTable() function
 * DepAnalysis()               // Test DepAnalysis() function
 * CsGraph<string, string>()   // Test CsGraph<string, string>() function
 * 
 */

/* Build Process
 * ======================
 * Required Files:
 *   DepAnalysis.cs   FileMg.cs   StrongComponent.cs   TypeAnalysis.cs  TypeTable.cs
 *   
 * Compiler Command:
 *   csc /target:exe AutoTestUnit.cs DepAnalysis.cs   FileMg.cs   StrongComponent.cs   TypeAnalysis.cs  TypeTable.cs
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
using TypeTableNS;
using CodeAnalysis;
using DepAnalysisNS;
using FileMgNS;
using StrongComponent;

namespace AutoTestUnitNS
{
    //  This is an AutoTestUnit class for testing the requirement of project 3
    class AutoTestUnit
    {
        // This is a test method for testing the requirement 1
        public void requirement1()
        {
            Console.WriteLine();
            Console.WriteLine("This is the requirement 1 test");
            Console.WriteLine("Requirement: List all .cs format files in the whole solution");
            Console.WriteLine("-----------------------------------------------------------------------");
            FileMg get_cs = new FileMg();
            string[] all_files = get_cs.find_solu_all_cs(get_cs.get_solu_path());
            foreach (var file in all_files)
                Console.WriteLine(file);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Conclusion:  Meet the requirement 1 !!!!!");
            Console.WriteLine("-----------------------------------------------------------------------");

        }
        // This is a test method for testing the requirement 2

        public void requirement2()
        {
            Console.WriteLine();
            Console.WriteLine("This is the requirement 2 test");
            Console.WriteLine("Requirement: Analysis  and show any type defined by user");
            Console.WriteLine("-----------------------------------------------------------------------");
            FileMg get_cs = new FileMg();
            string[] all_files = get_cs.find_solu_all_cs(get_cs.get_solu_path());
            typeanalysier test = new typeanalysier();
            test.show_result(all_files);

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Conclusion:  Meet the requirement 2 !!!!");
            Console.WriteLine("-----------------------------------------------------------------------");
        }
        // This is a test method for testing the requirement 3

        public void requirement3()
        {

            Console.WriteLine();
            Console.WriteLine("This is the requirement 3 test");
            Console.WriteLine("Requirement: Make the typetable in the collection of all files");
            Console.WriteLine("-----------------------------------------------------------------------");
            FileMg get_cs = new FileMg();
            string[] all_files = get_cs.find_solu_all_cs(get_cs.get_solu_path());

            TypeTable tb = new TypeTable();
            tb = tb.getTypeTable(all_files);
            tb.show();

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Conclusion:  Meet the requirement 3 !!!!");
            Console.WriteLine("-----------------------------------------------------------------------");
        }
        // This is a test method for testing the requirement 4

        public void requirement4()
        {
            Console.WriteLine();
            Console.WriteLine("This is the requirement 4 test");
            Console.WriteLine("Requirement: Analysis the dependency between all files");
            Console.WriteLine("-----------------------------------------------------------------------");
            FileMg get_cs = new FileMg();
            string[] all_files = get_cs.find_solu_all_cs(get_cs.get_solu_path());

            DepAnalysis depana = new DepAnalysis();
            depana.match(all_files);
            depana.show_gra_depent();
            depana.show_rela_result();

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Conclusion:  Meet the requirement 4 !!!!");
            Console.WriteLine("-----------------------------------------------------------------------");
        }
        // This is a test method for testing the requirement 5

        public void requirement5()
        {
            Console.WriteLine();
            Console.WriteLine("This is the requirement 5 test");
            Console.WriteLine("Requirement: Find the strong connected component between all files");
            Console.WriteLine("-----------------------------------------------------------------------");
            FileMg get_cs = new FileMg();
            string[] all_files = get_cs.find_solu_all_cs(get_cs.get_solu_path());

            CsGraph<string, string> test = new CsGraph<string, string>("test");
            test.show_strong(all_files);

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Conclusion:  Meet the requirement 5 !!!!");
            Console.WriteLine("-----------------------------------------------------------------------");
        }
        

        // this is the entrance of this test program
        static void Main(string[] args)
        {
            AutoTestUnit test = new AutoTestUnit();
            test.requirement1();
            test.requirement2();
            test.requirement3();
            test.requirement4();
            test.requirement5();
            Console.ReadKey();
        }
    }
}

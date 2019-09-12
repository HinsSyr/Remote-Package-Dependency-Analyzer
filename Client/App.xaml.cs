///////////////////////////////////////////////////////////////////////////
// App.xaml.cs                                                           //                               
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
 * Execute before creating main window
 * 
 * Public Interface
 * ======================
 *

/* Build Process
 * ======================
 * Required Files:
 *  
 *   
 * Compiler Command:
 *   
 *   
 * Maintenance History
 * ======================
 * ver1.0 : 5 December 2018
 * - first release
 * 
 * Planned Modifications:
 * ----------------------
 * - 
 */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Navigator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        // This method will execute before creating main window
        private void Instruction(object sender, StartupEventArgs e)
        {
            MessageBox.Show("1.First select the beginning path\n2.Then connect the server\n3.Then you can analysis all files in selected folder");
            

        }

    }
}

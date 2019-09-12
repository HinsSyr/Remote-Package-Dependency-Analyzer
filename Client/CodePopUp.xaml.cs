///////////////////////////////////////////////////////////////////////////////
// CodePopUp.xaml.cs - Displays text file source in response to double-click //
// ver 1.0                                                                   //
// source:Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2017    //
// author:BO QIU , Master in Electrical Engineering,                         //
//                         Syracuse University                               //
//                         (315) 278-2362, bqiu03@syr.edu                    //
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Navigator
{
    public partial class CodePopUp : Window
    {

        // this method will execute before creating a popup window
        public CodePopUp()
        {
            InitializeComponent();
        }
    }
}

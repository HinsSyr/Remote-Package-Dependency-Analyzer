////////////////////////////////////////////////////////////////////////////
// NavigatorClient.xaml.cs - Demonstrates Directory Navigation in WPF App //
// ver 1.0                                                                //
// source:Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2017 // 
// author:Bo Qiu, Master in Electrical Engineering,                       //
//                         Syracuse University                            //
//                         (315) 278-2362, bqiu03@syr.edu                 //
////////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package defines WPF application processing by the client.  The client
 * displays a local FileFolder view, and a remote FileFolder view.  It supports
 * navigating into subdirectories, both locally and in the remote Server.
 * 
 * It also supports viewing local files.
 * 
 * Maintenance History:
 * --------------------
 * ver 1.0 : 5 Dec. 2018
 * - first release
 */
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Threading;
using MessagePassingComm;
using Environment;
using Files;
using System.Windows.Forms;

namespace Navigator
{
    // This is the Mainwindow creating class.
    public partial class MainWindow : Window
    {
        private IFileMgr fileMgr { get; set; } = null;  // note: Navigator just uses interface declarations
        Comm comm { get; set; } = null;
        Dictionary<string, Action<CommMessage>> messageDispatcher = new Dictionary<string, Action<CommMessage>>();
        Thread rcvThread = null;
        bool Client_Con_Server { get; set; } = false;

        // The main window will be created in this method
        public MainWindow()
        {
            InitializeComponent();
            initializeEnvironment();
            fileMgr = FileMgrFactory.create(FileMgrType.Local); // uses Environment
            comm = new Comm(ClientEnvironment.address, ClientEnvironment.port);
            initializeMessageDispatcher();
            rcvThread = new Thread(rcvThreadProc);
            rcvThread.Start();
            // creat a message for first responce so that the later message could successfully sent and being responed.
            CommMessage try_con = new CommMessage(CommMessage.MessageType.connect);
            try_con.from = ClientEnvironment.endPoint;
            try_con.to = ServerEnvironment.endPoint;
            comm.postMessage(try_con);
        }
        //----< make Environment equivalent to ClientEnvironment >-------

        void initializeEnvironment()
        {
            Environment.Environment.root = ClientEnvironment.root;
            Environment.Environment.address = ClientEnvironment.address;
            Environment.Environment.port = ClientEnvironment.port;
            Environment.Environment.endPoint = ClientEnvironment.endPoint;
        }
        //----< define how to process each message command >-------------

        void initializeMessageDispatcher()
        {
            //define the browse operation and refresh the listbox of files
            messageDispatcher["Start_Browse_Files"] = (CommMessage msg) =>
              {
                  remoteFiles.Items.Clear();
                  foreach (string file in msg.arguments)
                  {
                      remoteFiles.Items.Add(file);
                  }
              };
            //define the browse operation and refresh the listbox of dirs
            messageDispatcher["Start_Browse_Dirs"] = (CommMessage msg) =>
              {
                  remoteDirs.Items.Clear();
                  foreach (string dir in msg.arguments)
                  {
                      remoteDirs.Items.Add(dir);
                  }

              };
            // load remotefiles listbox with dirs from root
            messageDispatcher["getTopFiles"] = (CommMessage msg) =>
            {
                remoteFiles.Items.Clear();
                foreach (string file in msg.arguments)
                {
                    remoteFiles.Items.Add(file);
                }
            };
            // load remoteDirs listbox with dirs from root

            messageDispatcher["getTopDirs"] = (CommMessage msg) =>
            {
                remoteDirs.Items.Clear();
                foreach (string dir in msg.arguments)
                {
                    remoteDirs.Items.Add(dir);
                }
                Navigator_Status.Text = "Go to Top Directory";
            };
            // load remoteFiles listbox with files from folder

            messageDispatcher["moveIntoFolderFiles"] = (CommMessage msg) =>
            {
                
                remoteFiles.Items.Clear();
                foreach (string file in msg.arguments)
                {
                    remoteFiles.Items.Add(file);
                }
                
            };
            // load remoteDirs listbox with dirs from folder

            messageDispatcher["moveIntoFolderDirs"] = (CommMessage msg) =>
            {
                
                remoteDirs.Items.Clear();
                foreach (string dir in msg.arguments)
                {
                    remoteDirs.Items.Add(dir);
                }
                Navigator_Status.Text = "Go to Next Directory";
            };

            // receive message of after the doubleclick files
            messageDispatcher["DoubleClickFiles"] = (CommMessage msg) =>
             {
                 string fileName = remoteFiles.SelectedValue as string;
                 try
                 {
                     string path = System.IO.Path.Combine(ServerEnvironment.root, fileName);
                     string contents = File.ReadAllText(path);
                     CodePopUp popup = new CodePopUp();
                     popup.codeView.Text = contents;
                     popup.Show();
                 }
                 catch (Exception ex)
                 {
                     string msg1 = ex.Message;
                 }
                 
             };
            // go back to upper dirs
            messageDispatcher["GoToUpDir"] = (CommMessage msg) =>
              {
                 
                  remoteDirs.Items.Clear();
                  foreach (string dir in msg.arguments)
                  {
                      remoteDirs.Items.Add(dir);
                  }
                  Navigator_Status.Text = "Back to Up Directory";
              };
            // go to top files
            messageDispatcher["GoToUpFiles"] = (CommMessage msg) =>
              {
                 
                  remoteFiles.Items.Clear();
                  foreach (string file in msg.arguments)
                  {
                      remoteFiles.Items.Add(file);
                  }
                 
              };
            // refresh textbox after typetable analysis
            messageDispatcher["Rq_Analysis_Ttable"] = (CommMessage msg) =>
              {
                
                  StringBuilder storage = new StringBuilder();
                  foreach (var argument in msg.arguments)
                      storage.Append(argument);
                  Results.Text = storage.ToString();
                  Analysis_Status.Text = "Analysis of Tyeptable Completed";
              };
            //refresh textbox after dependency analysis
            messageDispatcher["Rq_Analysis_Depend"] = (CommMessage msg) =>
              {
                  StringBuilder storage = new StringBuilder();
                  foreach (var argument in msg.arguments)
                      storage.Append(argument);
                  Results.Text = storage.ToString();
                  Analysis_Status.Text = "Analysis of Dependency Completed";
              };
            // refresh textbox after strong component analysis
            messageDispatcher["Rq_Analysis_SCC"] = (CommMessage msg) =>
              {
                  StringBuilder storage = new StringBuilder();
                  foreach (var argument in msg.arguments)
                      storage.Append(argument);
                  Results.Text = storage.ToString();
                  Analysis_Status.Text = "Analysis of Strong Component Completed";
              };
            // connect the server first to get files
            messageDispatcher["ConnectToServerFile"] = (CommMessage msg) =>
              {
                  
                  remoteFiles.Items.Clear();
                  foreach (string file in msg.arguments)
                  {
                      remoteFiles.Items.Add(file);
                  }
                  Client_Con_Server = true;
                  Navigator_Status.Text = "Connected";
              };
            //connect the server first to get dirs
            messageDispatcher["ConnectToServerDir"] = (CommMessage msg) =>
            {
                foreach (string dir in msg.arguments)
                {
                    remoteDirs.Items.Add(dir);
                }
                Client_Con_Server = true;
                Navigator_Status.Text = "Connected";
            };
        }
        //----< define processing for GUI's receive thread >-------------

        void rcvThreadProc()
        {
            Console.Write("\n  starting client's receive thread");
            while (true)
            {
                CommMessage msg = comm.getMessage();
                msg.show();
                if (msg.command == null)
                    continue;

                // pass the Dispatcher's action value to the main thread for execution

                Dispatcher.Invoke(messageDispatcher[msg.command], new object[] { msg });
            }
        }
        //----< shut down comm when the main window closes >-------------

        private void Window_Closed(object sender, EventArgs e)
        {
            comm.close();

            // The step below should not be nessary, but I've apparently caused a closing event to 
            // hang by manually renaming packages instead of getting Visual Studio to rename them.

            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        //----< not currently being used >-------------------------------

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }
       
       
        //----< move to root of remote directories >---------------------
        /*
         * - sends a message to server to get files from root
         * - recv thread will create an Action<CommMessage> for the UI thread
         *   to invoke to load the remoteFiles listbox
         */

            //click remotetop button and the trigger of envent
        private void RemoteTop_Click(object sender, RoutedEventArgs e)
        {
            if (Client_Con_Server == true)
            {
                CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
                msg1.from = ClientEnvironment.endPoint;
                msg1.to = ServerEnvironment.endPoint;
                msg1.author = "Jim Fawcett";
                msg1.command = "getTopFiles";
                msg1.arguments.Add("");
                comm.postMessage(msg1);
                CommMessage msg2 = msg1.clone();
                msg2.command = "getTopDirs"; 
                comm.postMessage(msg2);
            }
            else Navigator_Status.Text = "Please connect to the Server first";

        }
        //----< download file and display source in popup window >-------

        private void remoteFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Client_Con_Server == true)
            {
                CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
                msg1.from = ClientEnvironment.endPoint;
                msg1.to = ServerEnvironment.endPoint;
                msg1.author = "Bo Qiu";
                msg1.command = "DoubleClickFiles";
                msg1.arguments.Add(remoteFiles.SelectedValue as string);
                comm.postMessage(msg1);
            }
            else Navigator_Status.Text = "Please connect to the Server first";
        }
        //----< move to parent directory of current remote path >--------

        private void RemoteUp_Click(object sender, RoutedEventArgs e)
        {
            if (Client_Con_Server == true)
            {
                CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
                msg1.from = ClientEnvironment.endPoint;
                msg1.to = ServerEnvironment.endPoint;
                msg1.author = "Bo Qiu";
                msg1.command = "GoToUpDir";
                msg1.arguments.Add(remoteFiles.SelectedValue as string);
                comm.postMessage(msg1);
                CommMessage msg2 = msg1.clone();
                msg2.command = "GoToUpFiles";
                comm.postMessage(msg2);
            }
            else Navigator_Status.Text = "Please connect to the Server first";
        }
        //----< move into remote subdir and display files and subdirs >--
        /*
         * - sends messages to server to get files and dirs from folder
         * - recv thread will create Action<CommMessage>s for the UI thread
         *   to invoke to load the remoteFiles and remoteDirs listboxs
         */

        private void remoteDirs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Client_Con_Server == true)
            {
                CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
                msg1.from = ClientEnvironment.endPoint;
                msg1.to = ServerEnvironment.endPoint;
                msg1.command = "moveIntoFolderDirs";
                msg1.arguments.Add(remoteDirs.SelectedValue as string );
                comm.postMessage(msg1);
                CommMessage msg2 = msg1.clone();
                msg2.command = "moveIntoFolderFiles";
                comm.postMessage(msg2);
            }
            else Navigator_Status.Text = "Please connect to the Server first";
        }

        // connect to server button entrance and the trigger of event
        private void Connect(object sender, RoutedEventArgs e)
        {

            if (Client_Con_Server == false)
            {
                CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
                msg1.from = ClientEnvironment.endPoint;
                msg1.to = ServerEnvironment.endPoint;
                msg1.author = "Bo Qiu";
                msg1.command = "ConnectToServerFile";
                msg1.arguments.Add("");
                comm.postMessage(msg1);
                CommMessage msg2 = msg1.clone();
                msg2.command = "ConnectToServerDir";
                comm.postMessage(msg2);
            }
            else
            {
                Navigator_Status.Text = "The client already connect to the server";
                return;
            }
        }
        //  Typetable analysis button and the trigger of event
        private void Typetable_func(object sender, RoutedEventArgs e)
        {
            if (Client_Con_Server == true)
            {
                Analysis_Status.Text = "Beginning Analysis of Tyeptable";
                CommMessage msg = new CommMessage(CommMessage.MessageType.request);
                msg.from = ClientEnvironment.endPoint;
                msg.to = ServerEnvironment.endPoint;
                msg.author = "Bo Qiu";
                msg.command = "Rq_Analysis_Ttable";
                comm.postMessage(msg);
            }
            else Analysis_Status.Text = "Please connect to the Server first";
        }
        //  Dependency analysis button and the trigger of event
        private void Dependency_func(object sender, RoutedEventArgs e)
        {
            if (Client_Con_Server == true)
            {
                Analysis_Status.Text = "Beginning Analysis of Dependency";
                CommMessage msg = new CommMessage(CommMessage.MessageType.request);
                msg.from = ClientEnvironment.endPoint;
                msg.to = ServerEnvironment.endPoint;
                msg.author = "Bo Qiu";
                msg.command = "Rq_Analysis_Depend";
                comm.postMessage(msg);
            }
            else Analysis_Status.Text = "Please connect to the Server first";

        }
        //  StrongComponent analysis button and the trigger of event
        private void StrongComponent_func(object sender, RoutedEventArgs e)
        {
            if (Client_Con_Server == true)
            {
                Analysis_Status.Text = "Beginning Analysis of StrongComponent";
                CommMessage msg = new CommMessage(CommMessage.MessageType.request);
                msg.from = ClientEnvironment.endPoint;
                msg.to = ServerEnvironment.endPoint;
                msg.author = "Bo Qiu";
                msg.command = "Rq_Analysis_SCC";
                comm.postMessage(msg);
            }
            else Analysis_Status.Text = "Please connect to the Server first";
        }
        //  Browse path  button and the trigger of event
        private void Browse_Path(object sender, RoutedEventArgs e)
        {
            string Browse_Path = "";
            FolderBrowserDialog fddialog = new FolderBrowserDialog();
            fddialog.Description = "Select Path";
            fddialog.ShowDialog();

            if (fddialog.SelectedPath != "")
            {
               Browse_Path = fddialog.SelectedPath;
            }
            if (Client_Con_Server == true && Browse_Path!= "")
            {
                CommMessage msg = new CommMessage(CommMessage.MessageType.request);
                msg.from = ClientEnvironment.endPoint;
                msg.to = ServerEnvironment.endPoint;
                msg.author = "Bo Qiu";
                msg.command = "Start_Browse_Files";
                msg.arguments.Add(Browse_Path);
                comm.postMessage(msg);

                CommMessage msg2 = msg.clone();
                msg2.command = "Start_Browse_Dirs";
                comm.postMessage(msg2);

            }
            else Analysis_Status.Text = "Please connect to the Server first";

        }
    }
}

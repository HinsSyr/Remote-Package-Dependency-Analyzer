////////////////////////////////////////////////////////////////////////////
// NavigatorServer.cs - File Server for WPF NavigatorClient Application   //
// ver 2.0                                                                //
// source:Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2017 //
// Author               -- BO QIU , Master in Electrical Engineering,     //
//                         Syracuse University                            //
//                         (315) 278-2362, bqiu03@syr.edu                 //
////////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package defines a single NavigatorServer class that returns file
 * and directory information about its rootDirectory subtree.  It uses
 * a message dispatcher that handles processing of all incoming and outgoing
 * messages.
 * 
 * Maintanence History:
 * --------------------
 * ver 2.0 - 24 Oct 2017
 * - added message dispatcher which works very well - see below
 * - added these comments
 * ver 1.0 - 22 Oct 2017
 * - first release
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePassingComm;
using Environment;
using Files;
using System.IO;
using TypeTableNS;
using DepAnalysisNS;
using StrongComponent;

namespace Navigator
{
    public class NavigatorServer
    {
        IFileMgr localFileMgr { get; set; } = null;
        Comm comm { get; set; } = null;

        Dictionary<string, Func<CommMessage, CommMessage>> messageDispatcher =
          new Dictionary<string, Func<CommMessage, CommMessage>>();

        /*----< initialize server processing >-------------------------*/

        

        public NavigatorServer()
        {
            initializeEnvironment();
            Console.Title = "Navigator Server";
            localFileMgr = FileMgrFactory.create(FileMgrType.Local);
        }
        /*----< set Environment properties needed by server >----------*/

        void initializeEnvironment()
        {
            Environment.Environment.root = ServerEnvironment.root;
            Environment.Environment.address = ServerEnvironment.address;
            Environment.Environment.port = ServerEnvironment.port;
            Environment.Environment.endPoint = ServerEnvironment.endPoint;
        }
        /*----< define how each message will be processed >------------*/

        void initializeDispatcher()
        {
            // process the message and reply the new files address  
            Func<CommMessage, CommMessage> Start_Browse_Files = (CommMessage msg) =>
               {
                   Environment.Environment.root = msg.arguments[0];
                   Console.WriteLine(Environment.Environment.root);
                   localFileMgr.pathStack.Clear();
                   msg.arguments.Clear();
                   localFileMgr.currentPath = "";
                   CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                   reply.to = msg.from;
                   reply.from = msg.to;
                   reply.command = "Start_Browse_Files";
                   reply.arguments = localFileMgr.getFiles().ToList<string>();
                   return reply;
               };
            messageDispatcher["Start_Browse_Files"] = Start_Browse_Files;

            // process the message and reply the new dirs address
            Func<CommMessage, CommMessage> Start_Browse_Dirs = (CommMessage msg) =>
               {
                   Environment.Environment.root = msg.arguments[0];
                   localFileMgr.pathStack.Clear();
                   msg.arguments.Clear();
                   localFileMgr.currentPath = "";
                   CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                   reply.to = msg.from;
                   reply.from = msg.to;
                   reply.command = "Start_Browse_Dirs";
                   reply.arguments = localFileMgr.getDirs().ToList<string>();
                   return reply;
               };
            messageDispatcher["Start_Browse_Dirs"] = Start_Browse_Dirs;
            // process the message and reply the new message to get top files
            Func<CommMessage, CommMessage> getTopFiles = (CommMessage msg) =>
            {
                localFileMgr.pathStack.Clear();
                msg.arguments.Clear();
                localFileMgr.currentPath = "";
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "getTopFiles";
                reply.arguments = localFileMgr.getFiles().ToList<string>();
                return reply;
            };
            messageDispatcher["getTopFiles"] = getTopFiles;
            // process the message and reply the new message to get top dirs
            Func<CommMessage, CommMessage> getTopDirs = (CommMessage msg) =>
            {
                localFileMgr.pathStack.Clear();
                msg.arguments.Clear();
                localFileMgr.currentPath = "";
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "getTopDirs";
                reply.arguments = localFileMgr.getDirs().ToList<string>();
                return reply;
            };
            messageDispatcher["getTopDirs"] = getTopDirs;

            // process the message and reply the new message to move into next files
            Func<CommMessage, CommMessage> moveIntoFolderFiles = (CommMessage msg) =>
            {
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "moveIntoFolderFiles";
                reply.arguments = localFileMgr.getFiles().ToList<string>();
                return reply;
            };
            messageDispatcher["moveIntoFolderFiles"] = moveIntoFolderFiles;

            // process the message and reply the new message to move into next dirs
            Func<CommMessage, CommMessage> moveIntoFolderDirs = (CommMessage msg) =>
            {
               
                    string dirName = msg.arguments.Last().ToString();
                    localFileMgr.pathStack.Push(localFileMgr.currentPath);
                    localFileMgr.currentPath = dirName;
                Console.WriteLine("now add ress : --------------{0}", dirName);
                
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "moveIntoFolderDirs";
                reply.arguments = localFileMgr.getDirs().ToList<string>();
                return reply;
            };
            messageDispatcher["moveIntoFolderDirs"] = moveIntoFolderDirs;

            // process the message and reply the new message to double click files
            Func<CommMessage, CommMessage> DoubleClickFiles = (CommMessage msg) =>
              {
                 
                  CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                  reply.to = msg.from;
                  reply.from = msg.to;
                  reply.command = "DoubleClickFiles";
                  reply.arguments = msg.arguments;
                  return reply;
              };
            messageDispatcher["DoubleClickFiles"] = DoubleClickFiles;

            // process the message and reply the new message to go back upper dirs
            Func<CommMessage, CommMessage> GoToUpDir = (CommMessage msg) =>
               {
                   if (localFileMgr.currentPath != "")
                   {
                       localFileMgr.currentPath = localFileMgr.pathStack.Peek();
                       localFileMgr.pathStack.Pop();
                   }
                   else
                       localFileMgr.currentPath = "";
                  
                   CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                   reply.to = msg.from;
                   reply.from = msg.to;
                   reply.command = "GoToUpDir";
                   reply.arguments= localFileMgr.getDirs().ToList<string>();
                   return reply;
               };
            messageDispatcher["GoToUpDir"] = GoToUpDir;

            // process the message and reply the new message to go back upper files
            Func<CommMessage, CommMessage> GoToUpFiles = (CommMessage msg) =>
              {
                  CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                  reply.to = msg.from;
                  reply.from = msg.to;
                  reply.command = "GoToUpFiles";
                  reply.arguments= localFileMgr.getFiles().ToList<string>();
                  return reply;
              };
            messageDispatcher["GoToUpFiles"] = GoToUpFiles;

            // process the message and reply the new message to typetable analysis
            Func<CommMessage, CommMessage> Rq_Analysis_Ttable = (CommMessage msg) =>
               {
                   List<string> files = new List<string>();
                   string path = Path.Combine(Environment.Environment.root, localFileMgr.currentPath);
                   Console.WriteLine(" string path = Path.Combine(Environment.Environment.root,localFileMgr.currentPath)  --- list:  {0}", path);
                   string absPath = Path.GetFullPath(path);
                   Console.WriteLine(" string absPath = Path.GetFullPath(path);  --- list:  {0}", absPath);
                   localFileMgr.FindFile(absPath);
                   files = localFileMgr.all_files;
                   string[] xx = files.ToArray();
                   TypeTable t_table = new TypeTable();
                   foreach(var aa in xx)
                   Console.WriteLine(aa);
                   t_table = t_table.getTypeTable(files.ToArray());
                   string result = t_table.tt_to_string();
                   Console.WriteLine("----------------------------------- {0}", result.Length); 
                   CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                   reply.to = msg.from;
                   reply.from = msg.to;
                   reply.command = "Rq_Analysis_Ttable";

                   reply.arguments.Clear();
                   if (result.Length > 1024)
                   {
                       string pp = "";
                       for (int i = 0; i < result.Length; i++)
                       {
                           pp += result[i];
                           if (pp.Length >= 1024)
                           {
                               reply.arguments.Add(pp);
                               pp = "";
                           }

                       }
                       reply.arguments.Add(pp);
                       pp = "";
                   }
                   else
                       reply.arguments.Add(result);

                   localFileMgr.all_files.Clear();
                   return reply;
               };
            messageDispatcher["Rq_Analysis_Ttable"] = Rq_Analysis_Ttable;

            // process the message and reply the new message to Dependency analysis
            Func<CommMessage, CommMessage> Rq_Analysis_Depend = (CommMessage msg) =>
              {
                  List<string> files = new List<string>();
                  string path = Path.Combine(Environment.Environment.root, localFileMgr.currentPath);
                  Console.WriteLine(" string path = Path.Combine(Environment.Environment.root,localFileMgr.currentPath)  --- list:  {0}", path);
                  string absPath = Path.GetFullPath(path);
                  Console.WriteLine(" string absPath = Path.GetFullPath(path);  --- list:  {0}", absPath);
                  localFileMgr.FindFile(absPath);
                  files = localFileMgr.all_files;
                  DepAnalysis depend_analysis = new DepAnalysis();
                  depend_analysis.match(files.ToArray());
                  string result = depend_analysis.dep_tostring();
                  CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                  reply.to = msg.from;
                  reply.from = msg.to;
                  reply.command = "Rq_Analysis_Depend";
                  reply.arguments.Clear();
                  if (result.Length > 1024)
                  {
                      string pp = "";
                      for (int i = 0; i < result.Length; i++)
                      {
                          pp += result[i];
                          if (pp.Length >= 1024)
                          {
                              reply.arguments.Add(pp);
                              pp = "";
                          }

                      }
                      reply.arguments.Add(pp);
                      pp = "";
                  }
                  else
                      reply.arguments.Add(result);


                  localFileMgr.all_files.Clear();
                  return reply;
              };
            messageDispatcher["Rq_Analysis_Depend"] = Rq_Analysis_Depend;

            // process the message and reply the new message to Strong componenet analysis
            Func<CommMessage, CommMessage> Rq_Analysis_SCC = (CommMessage msg) =>
               {
                   List<string> files = new List<string>();
                   string path = Path.Combine(Environment.Environment.root, localFileMgr.currentPath);
                   Console.WriteLine(" string path = Path.Combine(Environment.Environment.root,localFileMgr.currentPath)  --- list:  {0}", path);
                   string absPath = Path.GetFullPath(path);
                   Console.WriteLine(" string absPath = Path.GetFullPath(path);  --- list:  {0}", absPath);
                   localFileMgr.FindFile(absPath);
                   files = localFileMgr.all_files;
                   CsGraph<string, string> scc = new CsGraph<string, string>("scc");
                   string result = scc.show_strong(files.ToArray()).SC_tostring();
                   CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                   reply.to = msg.from;
                   reply.from = msg.to;
                   reply.command = "Rq_Analysis_SCC";
                   reply.arguments.Clear();
                   if (result.Length > 1024)
                   {
                       string pp = "";
                       for (int i = 0; i < result.Length; i++)
                       {
                           pp += result[i];
                           if (pp.Length >= 1024)
                           {
                               reply.arguments.Add(pp);
                               pp = "";
                           }

                       }
                       reply.arguments.Add(pp);
                       pp = "";
                   }
                   else
                       reply.arguments.Add(result);


                   localFileMgr.all_files.Clear();
                   return reply;
               };
            messageDispatcher["Rq_Analysis_SCC"] = Rq_Analysis_SCC;

            // process the message and reply the new message to connect to server and obtain files
            Func<CommMessage, CommMessage> ConnectToServerFile = (CommMessage msg) =>
             {

                
                 CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                 reply.to = msg.from;
                 reply.from = msg.to;
                 reply.command = "ConnectToServerFile";
                 reply.arguments = localFileMgr.getFiles().ToList<string>();
                 return reply;

             };
            messageDispatcher["ConnectToServerFile"] = ConnectToServerFile;

            // process the message and reply the new message to connect to server and obtain dirs
            Func<CommMessage, CommMessage> ConnectToServerDir = (CommMessage msg) =>
            {

                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "ConnectToServerDir";
                reply.arguments = localFileMgr.getDirs().ToList<string>();
                return reply;

            };
            messageDispatcher["ConnectToServerDir"] = ConnectToServerDir;
        }
        /*----< Server processing >------------------------------------*/
        /*
         * - all server processing is implemented with the simple loop, below,
         *   and the message dispatcher lambdas defined above.
         */
        static void Main(string[] args)
        {
            TestUtilities.title("Starting Navigation Server", '=');
            try
            {
                NavigatorServer server = new NavigatorServer();
                server.initializeDispatcher();
                server.comm = new MessagePassingComm.Comm(ServerEnvironment.address, ServerEnvironment.port);

                while (true)
                {
                    CommMessage msg = server.comm.getMessage();
                    if (msg.type == CommMessage.MessageType.closeReceiver)
                        break;
                    msg.show();
                    if (msg.command == null)
                        continue;
                    
                    CommMessage reply = server.messageDispatcher[msg.command](msg);
                    reply.show();
                    server.comm.postMessage(reply);
                    Console.WriteLine("already post");
                }
            }
            catch (Exception ex)
            {
                Console.Write("\n  exception thrown:\n{0}\n\n", ex.Message);
            }
        }
    }
}

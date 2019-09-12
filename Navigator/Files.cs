/////////////////////////////////////////////////////////////////////
// FileMgr - provides file and directory handling for navigation   //
// ver 1.0                                                         //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2017 //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package defines IFileMgr interface, FileMgrFactory, and LocalFileMgr
 * classes.  Clients use the FileMgrFactory to create an instance bound to
 * an interface reference.
 * 
 * The FileManager finds files and folders at the root path and in any
 * subdirectory in the tree rooted at that path.
 * 
 * Maintenance History:
 * --------------------
 * ver 1.1 : 23 Oct 2017
 * - moved all Environment definitions into an Environment project
 * ver 1.0 : 22 Oct 2017
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Environment;

namespace Files
{
    public enum FileMgrType { Local, Remote }

    ///////////////////////////////////////////////////////////////////
    // NavigatorClient uses only this interface and factory

    public interface IFileMgr
    {
        IEnumerable<string> getFiles();
        IEnumerable<string> getDirs();
        bool setDir(string dir);
        Stack<string> pathStack { get; set; }
        string currentPath { get; set; }

        void FindFile(string dirPath);

        List<string> all_files { get; set; }

    }

    public class FileMgrFactory
    {
        static public IFileMgr create(FileMgrType type)
        {
            if (type == FileMgrType.Local)
                return new LocalFileMgr();
            else
                return null;  // eventually will have remote file Mgr
        }
    }

    ///////////////////////////////////////////////////////////////////
    // Concrete class for managing local files

    public class LocalFileMgr : IFileMgr
    {
        public string currentPath { get; set; } = "";
        public List<string> all_files { get; set; } = new List<string>();
        public Stack<string> pathStack { get; set; } = new Stack<string>();

        public LocalFileMgr()
        {
            pathStack.Push(currentPath);  // stack is used to move to parent directory
        }
        //----< get names of all files in current directory >------------

        public IEnumerable<string> getFiles()
        {
            List<string> files = new List<string>();
            string path = Path.Combine(Environment.Environment.root, currentPath);
            string absPath = Path.GetFullPath(path);
            files = Directory.GetFiles(path).ToList<string>();
            for (int i = 0; i < files.Count(); ++i)
            {
                files[i] = Path.Combine(currentPath, Path.GetFileName(files[i]));
            }
            return files;
        }
        //----< get names of all subdirectories in current directory >---
        public void FindFile(string dirPath) //参数dirPath为指定的目录
        {
            //在指定目录及子目录下查找文件,在listBox1中列出子目录及文件
            DirectoryInfo Dir = new DirectoryInfo(dirPath);
            
            try
            {
                foreach (DirectoryInfo d in Dir.GetDirectories())//查找子目录 
                {
                    if (d.GetFiles() != null)
                    {
                        FindFile(Path.Combine(Dir.ToString() , d.ToString()));
                        // listBox1.Items.Add(Dir + d.ToString() + "\\");  // listBox1中填加目录名
                    }
                }
                foreach (FileInfo f in Dir.GetFiles("*.cs")) //查找文件
                {
                    //if (f.Length!=0)

                    all_files.Add(Path.Combine(Dir.ToString() , f.ToString())); //listBox1中填加文件名
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public IEnumerable<string> getDirs()
        {
            List<string> dirs = new List<string>();
            string path = Path.Combine(Environment.Environment.root, currentPath);
            dirs = Directory.GetDirectories(path).ToList<string>();
            for (int i = 0; i < dirs.Count(); ++i)
            {
                string dirName = new DirectoryInfo(dirs[i]).Name;
                dirs[i] = Path.Combine(currentPath, dirName);
            }
            return dirs;
        }
        //----< sets value of current directory - not used >-------------

        public bool setDir(string dir)
        {
            if (!Directory.Exists(dir))
                return false;
            currentPath = dir;
            return true;
        }
    }

    class TestFileMgr
    {
        static void Main(string[] args)
        {
        }
    }
}

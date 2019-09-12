﻿///////////////////////////////////////////////////////////////////////
// RulesAndActions.cs - Parser rules specific to an application      //
// ver 2.4                                                           //
// Language:    C#, 2008, .Net Framework 4.6.1                       //
// Platform:    Dell Precision T7400, Win7, SP1                      //
// Application: Demonstration for CSE681, Project #2, Fall 2011      //
// source:      Jim Fawcett, CST 4-187, Syracuse University          //
//              (315) 443-3948, jfawcett@twcny.rr.com                //
// Author       BO QIU , Master in Electrical Engineering,           //
//              Syracuse University                                  //
//             (315) 278-2362, bqiu03@syr.edu                        //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * RulesAndActions package contains all of the Application specific
 * code required for most analysis tools.
 *
 * It defines the following Four rules which each have a
 * grammar construct detector and also a collection of IActions:
 *   - DetectNameSpace rule
 *   - DetectClass rule
 *   - DetectFunction rule
 *   - DetectScopeChange
 *   
 *   Three actions - some are specific to a parent rule:
 *   - Print
 *   - PrintFunction
 *   - PrintScope
 * 
 * The package also defines a Repository class for passing data between
 * actions and uses the services of a ScopeStack, defined in a package
 * of that name.
 *
 * Note:
 * This package does not have a test stub since it cannot execute
 * without requests from Parser.
 *  
 */
/* Required Files:
 *   IRuleAndAction.cs, RulesAndActions.cs, Parser.cs, ScopeStack.cs,
 *   Semi.cs, Toker.cs
 *   
 * Build command:
 *   csc /D:TEST_PARSER Parser.cs IRuleAndAction.cs RulesAndActions.cs \
 *                      ScopeStack.cs Semi.cs Toker.cs
 *   
 * Maintenance History:
 * --------------------
 * ver 2.4 : 31 Oct.2018
 * - added the rules to detect the alias, delegate,enum .
 * ver 2.3 : 30 Sep 2014
 * - added scope-based complexity analysis
 *   Note: doesn't detect braceless scopes yet
 * ver 2.2 : 24 Sep 2011
 * - modified Semi package to extract compile directives (statements with #)
 *   as semiExpressions
 * - strengthened and simplified DetectFunction
 * - the previous changes fixed a bug, reported by Yu-Chi Jen, resulting in
 * - failure to properly handle a couple of special cases in DetectFunction
 * - fixed bug in PopStack, reported by Weimin Huang, that resulted in
 *   overloaded functions all being reported as ending on the same line
 * - fixed bug in isSpecialToken, in the DetectFunction class, found and
 *   solved by Zuowei Yuan, by adding "using" to the special tokens list.
 * - There is a remaining bug in Toker caused by using the @ just before
 *   quotes to allow using \ as characters so they are not interpreted as
 *   escape sequences.  You will have to avoid using this construct, e.g.,
 *   use "\\xyz" instead of @"\xyz".  Too many changes and subsequent testing
 *   are required to fix this immediately.
 * ver 2.1 : 13 Sep 2011
 * - made BuildCodeAnalyzer a public class
 * ver 2.0 : 05 Sep 2011
 * - removed old stack and added scope stack
 * - added Repository class that allows actions to save and 
 *   retrieve application specific data
 * - added rules and actions specific to Project #2, Fall 2010
 * ver 1.1 : 05 Sep 11
 * - added Repository and references to ScopeStack
 * - revised actions
 * - thought about added folding rules
 * ver 1.0 : 28 Aug 2011
 * - first release
 *
 * Planned Modifications (not needed for Project #2):
 * --------------------------------------------------
 * - add folding rules:
 *   - CSemiExp returns for(int i=0; i<len; ++i) { as three semi-expressions, e.g.:
 *       for(int i=0;
 *       i<len;
 *       ++i) {
 *     The first folding rule folds these three semi-expression into one,
 *     passed to parser. 
 *   - CToker returns operator[]( as four distinct tokens, e.g.: operator, [, ], (.
 *     The second folding rule coalesces the first three into one token so we get:
 *     operator[], ( 
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using CSsemi;
using FileMgNS;

namespace CodeAnalysis
{
  public class Elem  // holds scope information
  {
    public string type { get; set; }
    public string name { get; set; }
    public int beginLine { get; set; }
    public int endLine { get; set; }
    public int beginScopeCount { get; set; }
    public int endScopeCount { get; set; }

    public override string ToString()
    {
      StringBuilder temp = new StringBuilder();
      temp.Append("{");
      temp.Append(String.Format("{0,-10}", type)).Append(" : ");
      temp.Append(String.Format("{0,-10}", name)).Append(" : ");
      temp.Append(String.Format("{0,-5}", beginLine.ToString()));  // line of scope start
      temp.Append(String.Format("{0,-5}", endLine.ToString()));    // line of scope end
      temp.Append("}");
      return temp.ToString();
    }
  }

  public class Repository
  {
    ScopeStack<Elem> stack_ = new ScopeStack<Elem>();
    List<Elem> locations_ = new List<Elem>();
    Dictionary<string, List<Elem>> locationsTable_ = new Dictionary<string, List<Elem>>();

    static Repository instance;

    public Repository()
    {
      instance = this;
    }

    //----< provides all code access to Repository >-------------------
    public static Repository getInstance()
    {
      return instance;
    }

    //----< provides all actions access to current semiExp >-----------

    public CSsemi.CSemiExp semi
    {
      get;
      set;
    }

    // semi gets line count from toker who counts lines
    // while reading from its source

    public int lineCount  // saved by newline rule's action
    {
      get { return semi.lineCount; }
    }
    public int prevLineCount  // not used in this demo
    {
      get;
      set;
    }

    //----< enables recursively tracking entry and exit from scopes >--

    public int scopeCount
    {
      get;
      set;
    }

    public ScopeStack<Elem> stack  // pushed and popped by scope rule's action
    {
      get { return stack_; } 
    }
 
    // the locations table is the result returned by parser's actions
    // in this demo

    public List<Elem> locations
    {
      get { return locations_; }
      set { locations_ = value; }
    }

    public Dictionary<string, List<Elem>> LocationsTable 
    {
      get { return locationsTable_; }
      set { locationsTable_ = value; } 
    }

  }
  /////////////////////////////////////////////////////////
  // pushes scope info on stack when entering new scope

  public class PushStack : AAction
  {
    public PushStack(Repository repo)
    {
      repo_ = repo;
    }
    public override void doAction(CSsemi.CSemiExp semi)
    {
      Display.displayActions(actionDelegate, "action PushStack");
      ++repo_.scopeCount;
      Elem elem = new Elem();
      elem.type = semi[0];  // expects type
      elem.name = semi[1];  // expects name
      elem.beginLine = repo_.semi.lineCount - 1;
      elem.endLine = 0;
      elem.beginScopeCount = repo_.scopeCount;
      elem.endScopeCount = 0;
      repo_.stack.push(elem);
      if (AAction.displayStack)
        repo_.stack.display();
      if (AAction.displaySemi)
      {
        Console.Write("\n  line# {0,-5}", repo_.semi.lineCount - 1);
        Console.Write("entering ");
        string indent = new string(' ', 2 * repo_.stack.count);
        Console.Write("{0}", indent);
        this.display(semi); // defined in abstract action
      }
      if (elem.type == "control" || elem.name == "anonymous")
        return;
      repo_.locations.Add(elem);
    }
  }
  /////////////////////////////////////////////////////////
  // pops scope info from stack when leaving scope

  public class PopStack : AAction
  {
    public PopStack(Repository repo)
    {
      repo_ = repo;
    }
    public override void doAction(CSsemi.CSemiExp semi)
    {
      Display.displayActions(actionDelegate, "action SaveDeclar");
      Elem elem;
      try
      {
        elem = repo_.stack.pop();
        for (int i = 0; i < repo_.locations.Count; ++i )
        {
          Elem temp = repo_.locations[i];
          if (elem.type == temp.type)
          {
            if (elem.name == temp.name)
            {
              if ((repo_.locations[i]).endLine == 0)
              {
                (repo_.locations[i]).endLine = repo_.semi.lineCount;
                (repo_.locations[i]).endScopeCount = repo_.scopeCount;
                break;
              }
            }
          }
        }
      }
      catch
      {
        return;
      }
      CSsemi.CSemiExp local = new CSsemi.CSemiExp();
      local.Add(elem.type).Add(elem.name);
      if(local[0] == "control")
        return;

      if (AAction.displaySemi)
      {
        Console.Write("\n  line# {0,-5}", repo_.semi.lineCount);
        Console.Write("leaving  ");
        string indent = new string(' ', 2 * (repo_.stack.count + 1));
        Console.Write("{0}", indent);
        this.display(local); // defined in abstract action
      }
    }
  }
  ///////////////////////////////////////////////////////////
  // action to print function signatures - not used in demo

  public class PrintFunction : AAction
  {
    public PrintFunction(Repository repo)
    {
      repo_ = repo;
    }
    public override void display(CSsemi.CSemiExp semi)
    {
      Console.Write("\n    line# {0}", repo_.semi.lineCount - 1);
      Console.Write("\n    ");
      for (int i = 0; i < semi.count; ++i)
        if (semi[i] != "\n" && !semi.isComment(semi[i]))
          Console.Write("{0} ", semi[i]);
    }
    public override void doAction(CSsemi.CSemiExp semi)
    {
      this.display(semi);
    }
  }
  /////////////////////////////////////////////////////////
  // concrete printing action, useful for debugging

  public class Print : AAction
  {
    public Print(Repository repo)
    {
      repo_ = repo;
    }
    public override void doAction(CSsemi.CSemiExp semi)
    {
      Console.Write("\n  line# {0}", repo_.semi.lineCount - 1);
      this.display(semi);
    }
  }
  /////////////////////////////////////////////////////////
  // display public declaration

  public class SaveDeclar : AAction
  {
    public SaveDeclar(Repository repo)
    {
      repo_ = repo;
    }
    public override void doAction(CSsemi.CSemiExp semi)
    {
      Display.displayActions(actionDelegate, "action SaveDeclar");
      Elem elem = new Elem();
      elem.type = semi[0];  // expects type
      elem.name = semi[1];  // expects name
      elem.beginLine = repo_.semi.lineCount;
      elem.endLine = elem.beginLine;
      elem.beginScopeCount = repo_.scopeCount;
      elem.endScopeCount = elem.beginScopeCount;
      if (AAction.displaySemi)
      {
        Console.Write("\n  line# {0,-5}", repo_.semi.lineCount - 1);
        Console.Write("entering ");
        string indent = new string(' ', 2 * repo_.stack.count);
        Console.Write("{0}", indent);
        this.display(semi); // defined in abstract action
      }
      repo_.locations.Add(elem);
    }
  }
  /////////////////////////////////////////////////////////
  // rule to detect namespace declarations

  public class DetectNamespace : ARule
  {
    public override bool test(CSsemi.CSemiExp semi)
    {
      Display.displayRules(actionDelegate, "rule   DetectNamespace");
      int index = semi.Contains("namespace");
      if (index != -1)
      {
        CSsemi.CSemiExp local = new CSsemi.CSemiExp();
        // create local semiExp with tokens for type and name
        local.displayNewLines = false;
        local.Add(semi[index]).Add(semi[index + 1]);
        doActions(local);
        return true;
      }
      return false;
    }
  }
  /////////////////////////////////////////////////////////
  // rule to dectect class,struct,enum and interface definitions

  public class DetectClass : ARule
  {
    public override bool test(CSsemi.CSemiExp semi)
    {
      Display.displayRules(actionDelegate, "rule   DetectClass");
      int indexCL = semi.Contains("class");
      int indexIF = semi.Contains("interface");
      int indexST = semi.Contains("struct");
      int indexEN = semi.Contains("enum");
      

            int index = Math.Max(indexCL, indexIF);
            index = Math.Max(index, indexST);
            index = Math.Max(index, indexEN);
           
      if (index != -1)
      {
        CSsemi.CSemiExp local = new CSsemi.CSemiExp();
        // local semiExp with tokens for type and name
        local.displayNewLines = false;
                if(semi[index] =="class" || semi[index] == "interface" || semi[index]== "struct" || semi[index]=="enum")
        local.Add(semi[index]).Add(semi[index + 1]);
                else
                    local.Add(semi[index]).Add(semi[index + 2]);
        doActions(local);
        return true;
      }
      return false;
    }
  }

    /////////////////////////////////////////////////////////
    // rule to dectect Delegate definitions

    public class DetectDelegate : ARule
    {
        public override bool test(CSsemi.CSemiExp semi)
        {
            Display.displayRules(actionDelegate, "rule   DetectDelegate");
            int indexDE = semi.Contains("delegate");

     
            if (indexDE != -1)
            {
                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                // local semiExp with tokens for type and name
                local.displayNewLines = false;
               
                    local.Add(semi[indexDE]).Add(semi[indexDE + 2]);
                doActions(local);
                return true;
            }
            return false;
        }
    }
    /////////////////////////////////////////////////////////
    // rule to dectect function definitions

    public class DetectFunction : ARule
  {
    public static bool isSpecialToken(string token)
    {
      string[] SpecialToken = { "if", "for", "foreach", "while", "catch", "using" };
      foreach (string stoken in SpecialToken)
        if (stoken == token)
          return true;
      return false;
    }
    public override bool test(CSsemi.CSemiExp semi)
    {
      Display.displayRules(actionDelegate, "rule   DetectFunction");
      if (semi[semi.count - 1] != "{")
        return false;

      int index = semi.FindFirst("(");
      if (index > 0 && !isSpecialToken(semi[index - 1]))
      {
        CSsemi.CSemiExp local = new CSsemi.CSemiExp();
        local.Add("function").Add(semi[index - 1]);
        doActions(local);
        return true;
      }
      return false;
    }
  }
  /////////////////////////////////////////////////////////
  // detect entering anonymous scope
  // - expects namespace, class, and function scopes
  //   already handled, so put this rule after those
  public class DetectAnonymousScope : ARule
  {
    public override bool test(CSsemi.CSemiExp semi)
    {
      Display.displayRules(actionDelegate, "rule   DetectAnonymousScope");
      int index = semi.Contains("{");
      if (index != -1)
      {
        CSsemi.CSemiExp local = new CSsemi.CSemiExp();
        // create local semiExp with tokens for type and name
        local.displayNewLines = false;
        local.Add("control").Add("anonymous");
        doActions(local);
        return true;
      }
      return false;
    }
  }


    /////////////////////////////////////////////////////////
    // rule to dectect Alias definitions

    public class DetectAlias : ARule
    {
        public override bool test(CSsemi.CSemiExp semi)
        {
            Display.displayRules(actionDelegate, "rule   DetecAlias");
            int indexF= semi.Contains("using");
            int indexS = semi.Contains("=");
           
            if (indexF != -1 && indexS!=-1 && (indexS-indexF)==2)
            {
                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                local.displayNewLines = false;
                local.Add("Alias").Add(semi[indexF+1]);
                doActions(local);
                return true;
            }
            return false;

        }
    }
  /////////////////////////////////////////////////////////
  // detect public declaration

  public class DetectPublicDeclar : ARule
  {
    public override bool test(CSsemi.CSemiExp semi)
    {
      Display.displayRules(actionDelegate, "rule   DetectPublicDeclar");
      int index = semi.Contains(";");
      if (index != -1)
      {
        index = semi.Contains("public");
        if (index == -1)
          return true;
        CSsemi.CSemiExp local = new CSsemi.CSemiExp();
        // create local semiExp with tokens for type and name
        local.displayNewLines = false;
        local.Add("public "+semi[index+1]).Add(semi[index+2]);

        index = semi.Contains("=");
        if (index != -1)
        {
          doActions(local);
          return true;
        }
        index = semi.Contains("(");
        if(index == -1)
        {
          doActions(local);
          return true;
        }
      }
      return false;
    }
  }
  /////////////////////////////////////////////////////////
  // detect leaving scope

  public class DetectLeavingScope : ARule
  {
    public override bool test(CSsemi.CSemiExp semi)
    {
      Display.displayRules(actionDelegate, "rule   DetectLeavingScope");
      int index = semi.Contains("}");
      if (index != -1)
      {
        doActions(semi);
        return true;
      }
      return false;
    }
  }
    public class TestParser
    {
        //----< process commandline to get file references >-----------------

        public List<string> ProcessCommandline(string[] args)
        {
            List<string> files = new List<string>();
            if (args.Length == 0)
            {
                Console.Write("\n  Please enter file(s) to analyze\n\n");
                return files;
            }
            string path = args[0];
            path = Path.GetFullPath(path);
            for (int i = 1; i < args.Length; ++i)
            {
                string filename = Path.GetFileName(args[i]);
                files.AddRange(Directory.GetFiles(path, filename));
            }
            return files;
        }

        public void ShowCommandLine(string[] args)
        {
            Console.Write("\n  Commandline args are:\n  ");
            foreach (string arg in args)
            {
                Console.Write("  {0}", arg);
            }
            Console.Write("\n  current directory: {0}", System.IO.Directory.GetCurrentDirectory());
            Console.Write("\n");
        }
    }


    public class Parser
    {
        private List<IRule> Rules;

        public Parser()
        {
            Rules = new List<IRule>();
        }
        public void add(IRule rule)
        {
            Rules.Add(rule);
        }
        public void parse(CSsemi.CSemiExp semi)
        {
            // Note: rule returns true to tell parser to stop
            //       processing the current semiExp

            Display.displaySemiString(semi.displayStr());

            foreach (IRule rule in Rules)
            {
                if (rule.test(semi))
                    break;
            }
        }
    }
    public class BuildCodeAnalyzer
    {
        Repository repo = new Repository();

        public BuildCodeAnalyzer(CSsemi.CSemiExp semi)
        {
            repo.semi = semi;
        }
        public virtual Parser build()
        {
            Parser parser = new Parser();

            // decide what to show
            AAction.displaySemi = false;
            AAction.displayStack = false;  // false is default

            // action used for namespaces, classes, and functions
            PushStack push = new PushStack(repo);

            // capture namespace info
            DetectNamespace detectNS = new DetectNamespace();
            detectNS.add(push);
            parser.add(detectNS);

            // capture class info
            DetectClass detectCl = new DetectClass();
            detectCl.add(push);
            parser.add(detectCl);

            // capture delegate info
            DetectDelegate detectDG = new DetectDelegate();
            SaveDeclar dtg = new SaveDeclar(repo);
            detectDG.add(dtg);
            parser.add(detectDG);

            // capture alias info
            DetectAlias detectAs = new DetectAlias();
            SaveDeclar als = new SaveDeclar(repo);
            detectAs.add(als);
            parser.add(detectAs);

            // capture function info
            DetectFunction detectFN = new DetectFunction();
            detectFN.add(push);
            parser.add(detectFN);

            // handle entering anonymous scopes, e.g., if, while, etc.
            DetectAnonymousScope anon = new DetectAnonymousScope();
            anon.add(push);
            parser.add(anon);


            // show public declarations
            DetectPublicDeclar pubDec = new DetectPublicDeclar();
            SaveDeclar print = new SaveDeclar(repo);
            pubDec.add(print);
            parser.add(pubDec);

            // handle leaving scopes
            DetectLeavingScope leave = new DetectLeavingScope();
            PopStack pop = new PopStack(repo);
            leave.add(pop);
            parser.add(leave);

            // parser configured
            return parser;
        }
    }
    // Test class
    public class typeanalysier
    {
        public void show_result(string[] args)
        {
            Console.WriteLine("below is the all processing files : ");
            Console.WriteLine("\n");
            foreach (var csfile in args)
            {
                Console.WriteLine(csfile);
            }
            Console.Write("\n ======================\n");
            TestParser tp = new TestParser();
            foreach (string file in args)
            {
                Console.Write("\n  Processing file {0}\n", System.IO.Path.GetFileName(file));

                CSsemi.CSemiExp semi = new CSsemi.CSemiExp();
                semi.displayNewLines = false;
                if (!semi.open(file as string))
                {
                    Console.Write("\n  Can't open {0}\n\n", args[0]);
                }
                Console.Write("\n  Type and Function Analysis");
                Console.Write("\n ----------------------------");
                BuildCodeAnalyzer builder = new BuildCodeAnalyzer(semi);
                Parser parser = builder.build();
                try
                {
                    while (semi.getSemi())
                        parser.parse(semi);
                    Console.Write("\n  locations table contains:");
                }
                catch (Exception ex)
                {
                    Console.Write("\n\n  {0}\n", ex.Message);
                }
                Repository rep = Repository.getInstance();
                List<Elem> table = rep.locations;
                Console.Write(
                    "\n  {0,10}, {1,25}, {2,5}, {3,5}, {4,5}, {5,5}, {6,5}, {7,5}",
                    "category", "name", "bLine", "eLine", "bScop", "eScop", "size", "cmplx"
                );
                Console.Write(
                    "\n  {0,10}, {1,25}, {2,5}, {3,5}, {4,5}, {5,5}, {6,5}, {7,5}",
                    "--------", "----", "-----", "-----", "-----", "-----", "----", "-----"
                );
                foreach (Elem e in table)
                {
                    if (e.type == "class" || e.type == "struct")
                        Console.Write("\n");
                    Console.Write(
                      "\n  {0,10}, {1,25}, {2,5}, {3,5}, {4,5}, {5,5}, {6,5}, {7,5}",
                      e.type, e.name, e.beginLine, e.endLine, e.beginScopeCount, e.endScopeCount + 1,
                      e.endLine - e.beginLine + 1, e.endScopeCount - e.beginScopeCount + 1
                    );
                }
                Console.Write("\n");
                semi.close();
            }
        }
    }


    // package test sub 
    class TestAnalysis
    {
#if (TEST_ANALYSIS)
        static void Main(string[] args)
        {
            FileMg get_cs = new FileMg();
            string[] all_files = get_cs.find_solu_all_cs(get_cs.get_solu_path());
            typeanalysier test = new typeanalysier();
            test.show_result(all_files);
        }
#endif
    }

}


///////////////////////////////////////////////////////////////////////////
// StrongComponent.cs   -- Find the strong connected component in a graph//                               
// version              -- 1.0                                           // 
// Language             -- C#, .Net Framework 4.6.1                      // 
// Platform             -- Dell XPS2018, WIN10, VS2017 Community         // 
// Application          -- CSE681 Project#3 Homework   
// Source               -- Jim Fawcett, CST 2-187, Syracuse University   //
//                         (315) 443-3948, jfawcett@twcny.rr.com// 
// Author               -- BO QIU , Master in Electrical Engineering,    //
//                         Syracuse University                           //
//                         (315) 278-2362, bqiu03@syr.edu                //
///////////////////////////////////////////////////////////////////////////
/*
 * Module Operations
 * ======================
 * Find the strong connected component in a graph
 * 
 * Public Interface
 * ======================
 * CsEdge<V, E>            //define the edge
 * CsNode<V, E>           // define the node
 * CsGraph<V, E>                    // define the graph
 * tarjan()                      // the algorithm for finding the SCC
 * 
 * 
 */

/* Build Process
 * ======================
 * Required Files:
 *   DepAnalysis.cs   FileMg.cs
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
using DepAnalysisNS;
using FileMgNS;
using System.IO;

namespace StrongComponent
{
    public class CsEdge<V, E> // holds child node and instance of edge type E
    {
        public CsNode<V, E> targetNode { get; set; } = null;
        public E edgeValue { get; set; }

        public CsEdge(CsNode<V, E> node, E value)
        {
            targetNode = node;
            edgeValue = value;
        }
    };

    public class CsNode<V, E>
    {
        public V nodeValue { get; set; }
        public string name { get; set; }
        public List<CsEdge<V, E>> children { get; set; }
        public bool visited { get; set; }
        public int dfn_nodenumber { get; set; } = -1;
        public int low_nodenumber { get; set; } = -1;

        //----< construct a named node >---------------------------------------

        public CsNode(string nodeName)
        {
            name = nodeName;
            children = new List<CsEdge<V, E>>();
            visited = false;
        }
        //----< add child vertex and its associated edge value to vertex >-----

        public void addChild(CsNode<V, E> childNode, E edgeVal)
        {
            children.Add(new CsEdge<V, E>(childNode, edgeVal));
        }
        //----< find the next unvisited child >--------------------------------

        public CsEdge<V, E> getNextUnmarkedChild()
        {
            foreach (CsEdge<V, E> child in children)
            {
                if (!child.targetNode.visited)
                {
                    child.targetNode.visited = true;
                    return child;
                }
            }
            return null;
        }
        //----< has unvisited child? >-----------------------------------

        public bool hasUnmarkedChild()
        {
            foreach (CsEdge<V, E> child in children)
            {
                if (!child.targetNode.visited)
                {
                    return true;
                }
            }
            return false;
        }
        public void unmark()
        {
            visited = false;
        }
        public override string ToString()
        {
            return name;
        }
    }
    /////////////////////////////////////////////////////////////////////////
    // Operation<V,E> class

    class Operation<V, E>
    {
        //----< graph.walk() calls this on every node >------------------------

        virtual public bool doNodeOp(CsNode<V, E> node)
        {
            Console.Write("\n  {0}", node.ToString());
            return true;
        }
        //----< graph calls this on every child visitation >-------------------

        virtual public bool doEdgeOp(E edgeVal)
        {
            Console.Write(" {0}", edgeVal.ToString());
            return true;
        }
    }
    /////////////////////////////////////////////////////////////////////////
    // CsGraph<V,E> class

    public class CsGraph<V, E>
    {
        public CsNode<V, E> startNode { get; set; }
        public string name { get; set; }
        public bool showBackTrack { get; set; } = false;

        public List<string> scc_list = new List<string>();

        private List<CsNode<V, E>> adjList { get; set; }  // node adjacency list
        private Operation<V, E> gop = null;
        Stack<CsNode<V, E>> st = new Stack<CsNode<V, E>>();

        public static int dfn_index = 0;
        

  

        //----< construct a named graph >--------------------------------------

        public CsGraph(string graphName)
        {
            name = graphName;
            adjList = new List<CsNode<V, E>>();
            gop = new Operation<V, E>();
            startNode = null;
        }

        //----< add vertex to graph adjacency list >---------------------------

        public void addNode(CsNode<V, E> node)
        {
            adjList.Add(node);
        }
        //----< clear visitation marks to prepare for next walk >--------------

        public void clearMarks()
        {
            foreach (CsNode<V, E> node in adjList)
                node.unmark();
        }
        //----< depth first search from startNode >----------------------------

        public void walk()
        {
            if (adjList.Count == 0)
            {
                Console.Write("\n  no nodes in graph");
                return;
            }
            if (startNode == null)
            {
                Console.Write("\n  no starting node defined");
                return;
            }
            if (gop == null)
            {
                Console.Write("\n  no node or edge operation defined");
                return;
            }
            this.tarjan(startNode);
            foreach (CsNode<V, E> node in adjList)
                if (!node.visited)
                    tarjan(node);
            foreach (CsNode<V, E> node in adjList)
                node.unmark();
            return;
        }
        //----< depth first search from specific node >------------------------

        public void walk(CsNode<V, E> node)
        {
            // process this node

            gop.doNodeOp(node);
            node.visited = true;

            // visit children
            do
            {
                CsEdge<V, E> childEdge = node.getNextUnmarkedChild();
                if (childEdge == null)
                {
                    return;
                }
                else
                {
                    gop.doEdgeOp(childEdge.edgeValue);
                    walk(childEdge.targetNode);
                    if (node.hasUnmarkedChild() || showBackTrack)
                    {                         // popped back to predecessor node
                        gop.doNodeOp(node);     // more edges to visit so announce
                    }                         // location and next edge
                }
            } while (true);
        }
        // the method for finding SCC
        public void tarjan(CsNode<V, E> node)
        {
            node.dfn_nodenumber = node.low_nodenumber = ++dfn_index;                      
            st.Push(node);                          
            foreach (var a in node.children)
            {
                if ( a.targetNode.dfn_nodenumber < 0 )
                {
                    tarjan(a.targetNode);                 
                   node.low_nodenumber = Math.Min(a.targetNode.low_nodenumber, node.low_nodenumber);
                }
                else if (st.Contains(a.targetNode))                  
                    node.low_nodenumber = Math.Min(node.low_nodenumber,a.targetNode.dfn_nodenumber);
            }
            if (node.dfn_nodenumber == node.low_nodenumber)
            {
                scc_list.Add("\n SCC: ");
           //   Console.Write("\n SCC: ");
                CsNode<V, E> w;
                do
                {
                    w = st.Pop();
                  // Console.Write(w.name + " ");
                    scc_list.Add(w.name + " ");
                } while (w != node);
            }
        }
        // by recalling many times tarjan() to search all the nodes in the graph
        public void tarjan()
        {
            foreach (var v in adjList)
            {
                if(v.dfn_nodenumber<0)
                tarjan(v);

            }
        }

        // show the result of SCC
        public CsGraph<string,string> show_strong(string[] args)
        {
            DepAnalysis test = new DepAnalysis();
            test.match(args);
            CsGraph<string, string> dep_graph = new CsGraph<string, string>("dep_name");
            CsNode<string, string> graph_start_node = new CsNode<string, string>("start_graph");
            List<CsNode<string, string>> allnode = new List<CsNode<string, string>>();
            for (int i = 0; i < args.Length; ++i)
            {
                CsNode<string, string> nodes = new CsNode<string, string>(Path.GetFileName(args[i]));
                graph_start_node = nodes;
                allnode.Add(nodes);
            }

            foreach (var ele in test.depentable)
            {
                foreach (var ls in allnode)
                {
                    if (ls.name==ele.Key )
                    {
                        foreach (var item in ele.Value)
                        {
                            foreach (var ls2 in allnode)
                            {
                                if (ls2.name==item)
                                {
                                    ls.addChild(ls2, "XXX");
                                }
                            }
                        }
                    }
                }

            }
            foreach (var ls in allnode)
                dep_graph.addNode(ls);
        //    dep_graph.showDependencies();
            dep_graph.startNode = graph_start_node;

         //   Console.WriteLine("\n\n");
            Console.WriteLine("\n----------------------------show strong component -----------------------");

            dep_graph.tarjan();
            return dep_graph;
        }

        //show the graph dependencies

        public void showDependencies()
        {
            Console.Write("\n  Dependency Table:");
            Console.Write("\n -------------------");
            foreach (var node in adjList)
            {
                Console.Write("\n  {0}", node.name);
                for (int i = 0; i < node.children.Count; ++i)
                {
                    Console.Write("\n    {0}", node.children[i].targetNode.name);
                }
            }
        }
        public string SC_tostring()
        {
            string result = "";
            foreach (var item in scc_list)
            {
                result = result + item;
            }
            return result;
        }

        public void show()
        {
            foreach (var item in scc_list)
            {
                Console.WriteLine(item);
            }
        }

    }

    
    /////////////////////////////////////////////////////////////////////////
    // Test class

    class demoOperation : Operation<string, string>
    {
        override public bool doNodeOp(CsNode<string, string> node)
        {
            Console.Write("\n -- {0}", node.name);
            return true;
        }
    }

   // Test sub
    class Test_StrongComponent
    {
#if (Test_StrongC)
        static void Main(string[] args)
        {
            FileMg get_cs = new FileMg();
            string[] all_files = get_cs.find_solu_all_cs(get_cs.get_solu_path());

            CsGraph<string, string> test = new CsGraph<string, string>("test");
            test.show_strong(all_files).show();
          //  test.show();
            Console.WriteLine("\n\n");
            Console.WriteLine("\n----------------------------------------end of test--------------------------------");

        }
#endif
    }
}

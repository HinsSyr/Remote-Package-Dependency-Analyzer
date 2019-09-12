///////////////////////////////////////////////////////////////////////////
// ITokenCollection.cs  -- Builds interface ITokenCollection             //                               
// version              -- 1.0                                           // 
// Language             -- C#, .Net Framework 4.0                        // 
// Platform             -- Dell XPS2018, WIN10, VS2017 Community         // 
// Application          -- CSE681 Project#2 Homework                     // 
// Source               -- Jim Fawcett, CST 2-187, Syracuse University   //
//                         (315) 443-3948, jfawcett@twcny.rr.com         //
// Author               -- BO QIU , Master in Electrical Engineering,    //
//                         Syracuse University                           //
//                         (315) 278-2362, bqiu03@syr.edu                //
///////////////////////////////////////////////////////////////////////////
/*
 * Module Operations
 * ======================
 * The ITokenCollection interface is implemented by ClsSemiExp.
 * 
 * Public Interface
 * ======================
 * StringBuilder get();          // is implemented by SemiExp class.
 * 
 */

/* Build Process
 * ======================
 * Required Files:
 *   ClsTokens.cs   ClsSemiExp.cs
 *   
 * Compiler Command:
 *   csc /target:exe /define:TEST_SEMIEXPRESSION ClsTokens.cs ClsSemiExp.cs
 *   
 * Maintenance History
 * ======================
 * ver1.0 : 31 October 2018
 * - first release
 * 
 * Planned Modifications:
 * ----------------------
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer
{
    using Token = String;
    using TokColl = List<String>;

    public interface ITokenCollection : IEnumerable<Token>
    {
        bool open(string source);                 // attach toker to source
        void close();                             // close toker's source
        TokColl get();                            // collect semi
        int size();                               // number of tokens
        Token this[int i] { get; set; }           // index semi
        ITokenCollection add(Token token);        // add a token to collection
        bool insert(int n, Token tok);            // insert tok at index n
        void clear();                             // clear all tokens
        bool contains(Token token);               // has token?
        bool find(Token tok, out int index);      // find tok if in semi
        Token predecessor(Token tok);             // find token before tok
        bool hasSequence(params Token[] tokSeq);  // does semi have this sequence of tokens?
        bool hasTerminator();                     // does semi have a valid terminator
        bool isDone();                            // at end of tokenSource?
        int lineCount();                          // get number of lines processed
        string ToString();                        // concatenate tokens with intervening spaces
        void show();                              // display semi on console
    }
    class ItokenCollection_test
    {
        static void Main(string[] args)
        {
        }
    }
}

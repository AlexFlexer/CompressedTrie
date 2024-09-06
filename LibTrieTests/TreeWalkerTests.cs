using LibTrie;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibTrieTests
{
    [TestClass()]
    public class TreeWalkerTests
    {
        private ITreeWalker<TrieNodeInfo> mWalker;

        public TreeWalkerTests()
        {
            var trie = new Trie("1", "12", "13", "14", "125", "126", "127", "138", "139", "130", "141", "142", "143");
            mWalker = trie.Walker;
        }

        [TestMethod()]
        public void TestDefaultDepthFirstTraversalImplementation()
        {
            var rightAnswer = new string[] { "1", "2", "5", "6", "7", "3", "8", "9", "0", "4", "1", "2", "3" };
            int counter = -1;
            mWalker.WalkDepthFirst((node) =>
            {
                if (counter >= 0)
                {
                    Console.WriteLine("Depth-first for counter = " + counter);
                    Assert.AreEqual(rightAnswer[counter], node.Value.Value!.ToString());
                }
                ++counter;
                return true;
            });
        }

        [TestMethod()]
        public void TestDefaultRowsTraversalImplementation()
        {
            var rightAnswer = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "1", "2", "3" };
            int counter = -1;
            mWalker.WalkRowOrder((node) =>
            {
                if (counter >= 0)
                {
                    Console.WriteLine("Row-order for counter = " + counter);
                    Assert.AreEqual(rightAnswer[counter], node.Value.Value!.ToString());
                }
                ++counter;
                return true;
            });
        }
    }
}

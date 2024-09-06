using LibTrie.Printers;
using LibTrie;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibTrieTests
{
    [TestClass]
    public class ConsistencyTests
    {
        [TestMethod]
        [Timeout(60_000)]
        public void TestTrieDrivenStringsRestoration()
        {
            var lines = File.ReadAllLines(Constants.FILE_SAMPLE);
            var numeratedLines = new Dictionary<long, string>();
            foreach (var line in lines.Select((str, i) => new { i, str }))
                if (!string.IsNullOrEmpty(line.str))
                    numeratedLines.Add(line.i + 1, line.str);

            var trie = new Trie(lines);

            var lvl = -1;
            var stack = new Stack<string>();
            trie.Walker.WalkDepthFirst((node) =>
            {
                var diff = lvl - node.Value.Level;
                while (diff-- >= 0)
                    stack.Pop();
                stack.Push(node.Value.Value!);
                lvl = node.Value.Level;
                if (node.Value.Indexes.Any())
                    foreach (var ind in node.Value.Indexes)
                    {
                        Assert.IsTrue(numeratedLines.ContainsKey(ind));
                        Assert.AreEqual(numeratedLines[ind], StringifyStack(stack));
                        numeratedLines.Remove(ind);
                    }
                return true;
            });

            Assert.IsTrue(!numeratedLines.Any());
        }

        private string StringifyStack(Stack<string> stack)
        {
            var sb = new StringBuilder();
            foreach (var item in stack.Reverse())
                sb.Append(item);
            return sb.ToString();
        }
    }
}

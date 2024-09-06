using LibTrie;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibTrieTests
{
    [TestClass()]
    public class TrieWalkerTests
    {
        [TestMethod()]
        public void TestFirstCurrNodeCallGetsRoot()
        {
            var trie = new Trie();
            var root = trie.Root;
            var testRoot = trie.Walker.GetCurrentNode();
            CheckEqualityOfNodes(root, testRoot!);
        }

        [TestMethod()]
        public void TestGoToNodeOnNodeWithChildren()
        {
            var trie = new Trie("1", "12");
            var walker = trie.Walker;
            walker.GoToNode(walker.GetCurrentNode()!.Descendants[0]);
            var currNode = walker.GetCurrentNode();
            var node = walker.GoToNode(currNode!.Descendants[0]);
            var expectedNode = new TrieNodeInfo("2", 2, new SortedSet<long>() { 2 }, "12");
            CheckNodeInfos(node!.Value, expectedNode);
        }

        [TestMethod()]
        public void TestGoToPredecessorOnNonRoot()
        {
            var trie = new Trie("1", "12");
            var walker = trie.Walker;
            walker.GoToNode(walker.GetCurrentNode()!.Descendants[0]);
            var currNode = walker.GetCurrentNode();
            walker.GoToNode(currNode!.Descendants[0]);
            walker.GoToPredecessor();
            var expectedNode = new TrieNodeInfo("1", 1, new SortedSet<long>() { 1 }, "1");
            CheckNodeInfos(walker.GetCurrentNode()!.Value, expectedNode);
        }

        [TestMethod()]
        public void TestGoToPredecessorOnRoot()
        {
            var trie = new Trie("1", "12");
            var walker = trie.Walker;
            var node = walker.GoToPredecessor();
            Assert.IsNull(node);
            CheckEqualityOfNodes(walker.GetCurrentNode()!, trie.Root);
        }

        private void CheckNodeInfos(TrieNodeInfo n1, TrieNodeInfo n2)
        {
            Assert.AreEqual(n1.Value, n2.Value);
            Assert.AreEqual(n1.Level, n2.Level);
            Assert.IsTrue(Enumerable.SequenceEqual(n1.Indexes, n2.Indexes));
            Assert.AreEqual(n1.Descriptor, n2.Descriptor);
        }

        private void CheckEqualityOfNodes(Node<TrieNodeInfo> n1, Node<TrieNodeInfo> n2)
        {
            CheckNodeInfos(n1.Value, n2.Value);
            Assert.AreEqual(n1.Descendants.Count, n2.Descendants.Count);
            for (int i = 0; i < n1.Descendants.Count; i++)
                CheckNodeInfos(n1.Descendants[i], n2.Descendants[i]);
        }
    }
}

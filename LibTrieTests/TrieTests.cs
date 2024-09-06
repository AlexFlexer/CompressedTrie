using LibTrie;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibTrieTests
{
    /**
     * Быстрая заметка, на случай, если ты пришел(-ла) сюда, чтоб увидеть пример
     * тест-кейсов для древовидных структур данных: если узлы твоей структуры данных
     * указывают на своих "предков", не забудь написать тест-кейс для проверки, что
     * при изменениях структуры информация о предках у всех узлов правильно настроена.
     */
    [TestClass()]
    public class TrieTests
    {
        private readonly List<(string, long, TrieNodeInfo[])> mTestCases = new()
        {
            ("abcd", 1, new TrieNodeInfo[] {
                new TrieNodeInfo("", 0, new SortedSet<long> {}, ""),
                new TrieNodeInfo("abcd", 1, new SortedSet<long> { 1 }, "a")
            }),
            ("abcde", 2, new TrieNodeInfo[]
            {
                new TrieNodeInfo("", 0, new SortedSet<long> {}, ""),
                new TrieNodeInfo("abcd", 1, new SortedSet<long> { 1 }, "a"),
                new TrieNodeInfo("e", 2, new SortedSet<long> { 2 }, "ae")
            }),
            ("abcdf", 3, new TrieNodeInfo[]
            {
                new TrieNodeInfo("", 0, new SortedSet<long> {}, ""),
                new TrieNodeInfo("abcd", 1, new SortedSet<long> { 1 }, "a"),
                new TrieNodeInfo("e", 2, new SortedSet<long> { 2 }, "ae"),
                new TrieNodeInfo("f", 2, new SortedSet<long> { 3 }, "af")
            }),
            ("st", 4, new TrieNodeInfo[]
            {
                new TrieNodeInfo("", 0, new SortedSet<long> {}, ""),
                new TrieNodeInfo("abcd", 1, new SortedSet<long> { 1 }, "a"),
                new TrieNodeInfo("st", 1, new SortedSet<long> { 4 }, "s"),
                new TrieNodeInfo("e", 2, new SortedSet<long> { 2 }, "ae"),
                new TrieNodeInfo("f", 2, new SortedSet<long> { 3 }, "af")
            }),
            ("sf", 5, new TrieNodeInfo[]
            {
                new TrieNodeInfo("", 0, new SortedSet<long> {}, ""),
                new TrieNodeInfo("abcd", 1, new SortedSet<long> { 1 }, "a"),
                new TrieNodeInfo("s", 1, new SortedSet<long> { }, "s"),
                new TrieNodeInfo("e", 2, new SortedSet<long> { 2 }, "ae"),
                new TrieNodeInfo("f", 2, new SortedSet<long> { 3 }, "af"),
                new TrieNodeInfo("t", 2, new SortedSet<long> { 4 }, "st"),
                new TrieNodeInfo("f", 2, new SortedSet<long> { 5 }, "sf")
            }),
            ("s", 6, new TrieNodeInfo[]
            {
                new TrieNodeInfo("", 0, new SortedSet<long> {}, ""),
                new TrieNodeInfo("abcd", 1, new SortedSet<long> { 1 }, "a"),
                new TrieNodeInfo("s", 1, new SortedSet<long> { 6 }, "s"),
                new TrieNodeInfo("e", 2, new SortedSet<long> { 2 }, "ae"),
                new TrieNodeInfo("f", 2, new SortedSet<long> { 3 }, "af"),
                new TrieNodeInfo("t", 2, new SortedSet<long> { 4 }, "st"),
                new TrieNodeInfo("f", 2, new SortedSet<long> { 5 }, "sf")
            })
        };

        [TestMethod()]
        [Timeout(5_000)]
        public void TestTrieCommonAndEdgeCases()
        {
            var printer = new DebugTriePrinter();
            var trie = new Trie();
            foreach (var testCase in mTestCases)
            {
                Console.WriteLine("Testing case for index " + testCase.Item2);
                trie.AddString(testCase.Item1, testCase.Item2);
                AssertNodeInfosAreEqual(printer.GetLevelsList(trie), testCase.Item3);
            }
        }

        private string StringifyNodeInfos(IList<TrieNodeInfo> nodes)
        {
            var result = new StringBuilder();
            foreach (var info in nodes)
            {
                result.AppendLine(info.ToString());
                result.AppendLine("Indices: " + string.Join(",", info.Indexes));
            }
            return result.ToString();
        }

        private void AssertNodeInfosAreEqual(IList<TrieNodeInfo> result, IList<TrieNodeInfo> expected)
        {
            var sResult = StringifyNodeInfos(result);
            var sExpected = StringifyNodeInfos(expected);
            try
            {
                Assert.AreEqual(sExpected, sResult);
            }
            catch
            {
                Console.WriteLine("Result:");
                Console.WriteLine(sResult);
                Console.WriteLine("\nExpected:");
                Console.WriteLine(sExpected);
                throw;
            }
        }
    }
}
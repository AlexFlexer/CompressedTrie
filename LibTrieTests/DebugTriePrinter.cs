using LibTrie.Printers;
using LibTrie;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibTrieTests
{
    // специальный класс, который помимо печати префискного дерева умеет выводить
    // его уровни как список списков строк
    internal class DebugTriePrinter : BreadthTriePrinter
    {
        public IList<TrieNodeInfo> GetLevelsList(Trie t)
        {
            var walker = t.Walker;
            var result = new List<TrieNodeInfo>();
            if (walker.GetCurrentNode() != null)
            {
                walker.WalkRowOrder((nodeInfo) =>
                {
                    result.Add(nodeInfo.Value);
                    return true;
                });
            }
            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibTrie.Printers
{
    internal class DepthTriePrinter : ITriePrinter
    {
        private const char CHAR_DEPTH_INDICATOR = '.';

        public string Print(Trie trie)
        {
            var walker = trie.Walker;
            if (walker.GetCurrentNode() == null)
                return string.Empty;
            var result = new StringBuilder();
            walker.WalkDepthFirst((node) =>
            {
                for (int i = 0; i < node.Value.Level; ++i)
                    result.Append(CHAR_DEPTH_INDICATOR);
                result.Append(node.Value).AppendLine();
                return true;
            });
            return result.ToString();
        }
    }
}

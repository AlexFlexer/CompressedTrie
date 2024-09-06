using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibTrie
{
    /// <summary>
    /// Интерфейс компонента, который позволяет представлять дерево в виде строки.
    /// </summary>
    public interface ITriePrinter
    {
        /// <summary>
        /// Выводит префиксное дерево как строку.
        /// </summary>
        /// <param name="trie">префиксное дерево</param>
        /// <returns>строка, созданная из префиксного дерева</returns>
        public string Print(Trie trie);
    }
}

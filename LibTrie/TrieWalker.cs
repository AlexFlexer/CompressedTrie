using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibTrie
{
    /// <summary>
    /// Базовая информация узла префиксного дерева, доступная внешнему миру.
    /// </summary>
    /// <param name="Value">строка-значение узла</param>
    /// <param name="Level">текущий ряд в дереве (глубина) узла</param>
    /// <param name="Indexes">все индексы данного узла</param>
    /// <param name="Descriptor">уникальный идентификатор узла внутри дерева, по нему выполняются
    /// все операции поиска узлов в префиксном дереве</param>
    public class TrieNodeInfo
    {
        public readonly string? Value;
        public readonly int Level;
        public readonly IReadOnlySet<long> Indexes;
        public readonly string Descriptor;

        public TrieNodeInfo(string? value, int level, IReadOnlySet<long> indexes, string descriptor)
        {
            Value = value;
            Level = level;
            Indexes = indexes;
            Descriptor = descriptor;
        }

        public override bool Equals(object? obj)
        {
            return obj is TrieNodeInfo info &&
                   Value == info.Value &&
                   Level == info.Level &&
                   Enumerable.SequenceEqual(Indexes, info.Indexes) &&
                   Descriptor == info.Descriptor;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value, Level, Indexes, Descriptor);
        }
    }

    /// <summary>
    /// Реализация обходчика для префиксного дерева.
    /// </summary>
    internal class TrieWalker : ITreeWalker<TrieNodeInfo>
    {
        private readonly ITrieNodeInfoProvider mNodeInfoProvider;
        private Node<TrieNodeInfo>? mCurrentNode;

        public TrieWalker(ITrieNodeInfoProvider infoProvider)
        {
            mNodeInfoProvider = infoProvider;
            mCurrentNode = mNodeInfoProvider.ProvidePredeccor(null);
        }

        public Node<TrieNodeInfo>? GetCurrentNode() => mCurrentNode;

        public Node<TrieNodeInfo>? GoToNode(TrieNodeInfo descendantValue)
        {
            Node<TrieNodeInfo>? candidate = null;
            if (mCurrentNode != null)
                candidate = mNodeInfoProvider.ProvideDescendant(mCurrentNode.Value, descendantValue);
            if (candidate != null)
                mCurrentNode = candidate;
            return candidate;
        }

        public Node<TrieNodeInfo>? GoToPredecessor()
        {
            Node<TrieNodeInfo>? candidate = null;
            if (mCurrentNode != null)
                candidate = mNodeInfoProvider.ProvidePredeccor(mCurrentNode.Value);
            if (candidate != null)
                mCurrentNode = candidate;
            return candidate;
        }
    }

    /// <summary>
    /// Интерфейс класса, который способен предоставить информацию об узлах дерева/графа.
    /// </summary>
    internal interface ITrieNodeInfoProvider
    {
        /// <summary>
        /// Предоставляет информацию об узле-потомке текущего узла.
        /// </summary>
        /// <param name="currNodeInfo">информация о текущем узле</param>
        /// <param name="descendantInfo">информация об узле-потомке</param>
        /// <returns>абстрактный узел с узлами-потомками искомого по информации descendantInfo</returns>
        public Node<TrieNodeInfo> ProvideDescendant(TrieNodeInfo currNodeInfo, TrieNodeInfo descendantInfo);

        /// <summary>
        /// Предоставляет информацию об узле-родителе данного узла.
        /// </summary>
        /// <param name="info">информация узла, родителя которого надо найти, или null, если
        /// нужно получить корневой узел</param>
        /// <returns>узел-родитель узла, найденного по данной информации</returns>
        public Node<TrieNodeInfo>? ProvidePredeccor(TrieNodeInfo? info);
    }
}

using LibTrie.Printers;
using System.Text;

namespace LibTrie
{
    /// <summary>
    /// <para>
    /// Класс, реализующий сжатое префиксное дерево. Чтобы узнать о принципе работы
    /// префиксного дерево, пройди по ссылке: <a href="https://en.wikipedia.org/wiki/Trie">Trie</a>
    /// </para>
    /// 
    /// Здесь же я напишу о том, почему оно сжатое. Само по себе префиксное дерево
    /// (ПД) может быть использовано, например, в системах автодополнения, как, например,
    /// адресная строка браузера. Оно работает по принципу выделения отдельного узла для
    /// каждого символа, а строки, а у которых есть общий префикс, также используют одну и
    /// ту же ветку для хранения одинаковых префиксов.
    /// 
    /// <para>
    /// <b>Как происходит добавление элемента в дерево?</b>
    /// Допустим, мы хотим добавить несколько строк: 123, 124, 135. Про индексы пока не думаем, позже
    /// объясню, зачем они.
    /// Сначала мы добавляем строку 123. После добавления структура дерева будет такой: корень -> 123.
    /// Как видим, 123 - это один узел.
    /// Добавляем 124, структура меняется: корень -> 12 -> 3, корень -> 12 -> 4. Так как новая строка 
    /// отличается на один символ, мы отделяем последний символ от узла 123, так остается узел 12, и от него
    /// идут узлы 3 и 4.
    /// Длбавляем 135. Новая структура: корень -> 1 -> 2 -> 3, корень -> 1 -> 2 -> 4, корень -> 1 -> 35.
    /// Если же мы попытаемся добавить строку 12, то у узла 2 просто добавится новый индекс.
    /// </para>
    /// 
    /// <para>
    /// <b>Что такое индекс?</b>
    /// Его наличие показывает, что на данный узел (а именно, на данные символы узла)
    /// заканчивается одна или несколько строк.
    /// Индексом мог быть любой объект, но в данной реализации был выбран тип long.
    /// </para>
    /// </summary>
    public class Trie : ITrieNodeInfoProvider
    {
        private readonly NodeInternal mNodeMain = new NodeInternal();

        /// <summary>
        /// Обходчик дерева. Внимание: при каждом вызове свойства создается новый экземпляр
        /// обходчика.
        /// </summary>
        public ITreeWalker<TrieNodeInfo> Walker { get => new TrieWalker(this); }

        /// <summary>
        /// Корневой узел дерева. При каждом вызове свойства создается новый экземпляр.
        /// </summary>
        public Node<TrieNodeInfo> Root { get => CreatePublicNode(mNodeMain); }

        /// <summary>
        /// Объект, с помощью которого дерево выводится на печать при вызове Trie.ToString().
        /// Может быть null, тогда будет вызван base.ToString().
        /// </summary>
        public ITriePrinter? Printer { get; set; } = new BreadthTriePrinter();

        /// <summary>
        /// Создает пустое дерево.
        /// </summary>
        public Trie() { }

        /// <summary>
        /// Добавляет данные строки в дерево, при этом индексируя их начиная с 1 по
        /// data.Count.
        /// </summary>
        /// <param name="strings">Строки, которые нужно добавить в дерево.</param>
        public Trie(params string[] strings) : this(strings.AsEnumerable()) { }

        /// <summary>
        /// Добавляет данные пары строка-индекс в дерево.
        /// </summary>
        /// <param name="strIndexPairs">пары строка-индекс</param>
        public Trie(params (string, long)[] strIndexPairs) : this(strIndexPairs.AsEnumerable()) { }

        /// <summary>
        /// Добавляет данные строки в дерево, при этом индексируя их начиная с 1 по
        /// data.Count.
        /// </summary>
        /// <param name="data">Строки, которые нужно добавить в дерево.</param>
        public Trie(IEnumerable<string> data)
        {
            int autoIncrementIndex = 1;
            foreach (var item in data)
                AddString(item, autoIncrementIndex++);
        }

        /// <summary>
        /// Добавляет данные пары строка-индекс в дерево.
        /// </summary>
        /// <param name="data">пары строка-индекс</param>
        public Trie(IEnumerable<(string, long)> data)
        {
            foreach (var item in data)
                AddString(item.Item1, item.Item2);
        }

        /// <summary>
        /// Добавляет данную строку и данный индекс в дерево. Если данные индекс,строка уже существуют,
        /// структура дерева не меняется. Если строка пустая, также ничего не происходит.
        /// </summary>
        /// <param name="str">строка, подлежащая добавлению в дерево</param>
        /// <param name="uniqueIndex">индекс</param>
        public void AddString(string str, long uniqueIndex)
        {
            if (str == null || str.Length == 0)
                return;
            int indexStr = 0;
            int indexNode = 0;
            var node = mNodeMain;
            while (node != null)
            {
                if (indexNode >= node.mValue.Length)
                {
                    // самое время либо переходить на другой узел, либо вставлять остаток
                    // проверяемой строки как новый узел дерева
                    if (indexStr >= str.Length)
                    {
                        node.mIndexes.Add(uniqueIndex);
                        return;
                    }
                    var candidate = node.mNodes.FirstOrDefault((node) => node.mValue![0] == str[indexStr]);
                    if (candidate == null)
                    {
                        // нет подходящего узла, вставляем новый
                        InsertNewNodeForStringRemainder(str, uniqueIndex, node, indexStr);
                        return;
                    }
                    node = candidate;
                    indexNode = 0;
                }
                else
                {
                    // сравниваем символы, если равны, идем дальше, иначе
                    // разделяем узел на два
                    if (indexStr >= str.Length || str[indexStr] != node.mValue[indexNode])
                    {
                        // символы не равны, либо строка закончилась раньше,
                        // чем значение узла, разделяем узел на два
                        ForkNode(str, uniqueIndex, node, indexStr, indexNode);
                        return;
                    }
                    ++indexNode;
                    ++indexStr;
                }
            }
            throw new Exception("Control flow should never reach here!");
        }

        public Node<TrieNodeInfo> ProvideDescendant(TrieNodeInfo currNodeInfo, TrieNodeInfo descendantInfo)
        {
            var node = GetNodeByDescriptor(descendantInfo.Descriptor);
            if (node == null)
                throwBadNodeDescriptor(descendantInfo);
            return CreatePublicNode(node!);
        }

        public Node<TrieNodeInfo>? ProvidePredeccor(TrieNodeInfo? info)
        {
            if (info == null)
                return CreatePublicNode(mNodeMain);
            var nodeInternal = GetNodeByDescriptor(info.Descriptor);
            if (nodeInternal == null)
                throwBadNodeDescriptor(info);
            return nodeInternal == mNodeMain ? null : CreatePublicNode(nodeInternal!.mPredecessor!);
        }

        public override string? ToString() => Printer?.Print(this) ?? base.ToString();

        private void InsertNewNodeForStringRemainder(string str, long uniqueIndex, NodeInternal node, int indexStr)
        {
            var newNode = new NodeInternal();
            newNode.mValue.Append(str.AsSpan(indexStr));
            newNode.mCurrentLevel = node.mCurrentLevel + 1;
            newNode.mPredecessor = node;
            newNode.mIndexes.Add(uniqueIndex);
            node.mNodes.Add(newNode);
        }

        private void ForkNode(string str, long uniqueIndex, NodeInternal node, int indexStr, int indexNode)
        {
            // разделяем значения для форка
            var valueFromString = indexStr >= str.Length ? null : str.Substring(indexStr);
            var valueFromBuilder = GetChunk(node.mValue, indexNode, true);
            // создаем соответствующие узлы и связываем их с текущим, здесь:
            //  nodeFromString - узел, значение которого равно подстроке, с которой начинаются
            //      различия между значением текущего узла и данной строки, будь то окончание
            //      самой строки или отличие в символе;
            var nodeFromString = CreateAndLinkNodeFromStringValue(valueFromString, node);
            //  nodeFromBuilder - узел с подзначением текущего узла, т.е. в результате форка возможны
            //      два сценария: узел делится на два - различие в символе; часть узла становится
            //      новым узлом - строка закончилась раньше, чем значение текущего узла
            var nodeFromBuilder = CreateAndLinkNodeFromBuilderValue(valueFromBuilder, node);
            // передаем все индексы из старого узла в новый, который содержит часть старого значения
            // текущего узла
            foreach (long index in node.mIndexes)
                nodeFromBuilder.mIndexes.Add(index);
            node.mIndexes.Clear();
            // передаем узлы-потомки новому узлу (образованному от старого)
            nodeFromBuilder.mNodes.AddRange(node.mNodes);
            // также не забываем "сообщить" потомкам, что у них сменился предок
            foreach (var child in node.mNodes)
                child.mPredecessor = nodeFromBuilder;
            // очищаем список потомков текущего узла
            node.mNodes.Clear();
            // теперь у старого узла только один потомок: новый узел с подзначением
            // старого узла
            node.mNodes.Add(nodeFromBuilder);
            // новому поддереву нужно заново задать значение глубины (уровня)
            IncrementDepthLevel(nodeFromBuilder);
            // решаем, кому "подарить" индекс:
            if (nodeFromString != null)
            {
                // не забываем добавить новый узел в потомки текущему!
                node.mNodes.Add(nodeFromString);
                // если форк произошел из-за различий символов, то индекс, без сомнений,
                // должен быть подарен новому узлу
                nodeFromString.mIndexes.Add(uniqueIndex);
            }
            else
            {
                // если же форк произошел, потому что строка закончилась раньше, чем значение узла,
                // дарим индекс текущему узлу
                node.mIndexes.Add(uniqueIndex);
            }
        }

        private NodeInternal? CreateAndLinkNodeFromStringValue(string? valueFromString, NodeInternal node)
        {
            NodeInternal? nodeFromString = null;
            if (valueFromString != null)
            {
                nodeFromString = new NodeInternal();
                nodeFromString.mValue.Append(valueFromString);
                nodeFromString.mCurrentLevel = node.mCurrentLevel + 1;
                nodeFromString.mPredecessor = node;
            }
            return nodeFromString;
        }

        private NodeInternal CreateAndLinkNodeFromBuilderValue(StringBuilder valueFromBuilder, NodeInternal node)
        {
            var nodeFromBuilder = new NodeInternal();
            nodeFromBuilder.mValue.Append(valueFromBuilder);
            nodeFromBuilder.mPredecessor = node;
            nodeFromBuilder.mCurrentLevel = node.mCurrentLevel;
            return nodeFromBuilder;
        }

        private NodeInternal? GetNodeByDescriptor(string descriptor)
        {
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));
            if (descriptor.Length == 0)
                return mNodeMain;
            int indexDesc = 0;
            var node = mNodeMain;
            while (node != null && indexDesc < descriptor.Length)
            {
                var candidate = node.mNodes.FirstOrDefault((n) => descriptor[indexDesc] == n.mValue[0]);
                node = candidate;
                ++indexDesc;
            }
            return node;
        }

        private Node<TrieNodeInfo> CreatePublicNode(NodeInternal source)
        {
            var sourceInfo = new TrieNodeInfo(
                source.mValue.ToString(), source.mCurrentLevel, source.mIndexes, CreateDescriptor(source)
            );
            IReadOnlyList<TrieNodeInfo> children = source!.mNodes
                .Select(x => new TrieNodeInfo(x.mValue.ToString(), x.mCurrentLevel, x.mIndexes, CreateDescriptor(x)))
                .ToList();
            return new Node<TrieNodeInfo>(sourceInfo, children);
        }

        private string CreateDescriptor(NodeInternal source)
        {
            var node = source;
            var result = new StringBuilder(source.mCurrentLevel);
            while (node != null)
            {
                if (node != mNodeMain)
                    result.Insert(0, node.mValue[0]);
                node = node.mPredecessor;
            }
            return result.ToString();
        }

        private void IncrementDepthLevel(NodeInternal root)
        {
            var queue = new Queue<NodeInternal>();
            queue.Enqueue(root);
            while (queue.Any())
            {
                var node = queue.Dequeue();
                node.mCurrentLevel = node.mPredecessor == null
                    ? 0 : node.mPredecessor.mCurrentLevel + 1;
                foreach (var child in node.mNodes)
                    queue.Enqueue(child);
            }
        }

        private StringBuilder GetChunk(StringBuilder source, int start, bool cut) =>
            GetChunk(source, start, source.Length - start, cut);

        private StringBuilder GetChunk(StringBuilder source, int start, int count, bool cut)
        {
            if (start + count > source.Length)
                throw new ArgumentOutOfRangeException(nameof(count));
            var result = new StringBuilder(count);
            for (int i = start; i < start + count; ++i)
                result.Append(source[i]);
            if (cut)
                source.Remove(start, count);
            return result;
        }

        private void throwBadNodeDescriptor(TrieNodeInfo n)
        {
            throw new ArgumentException("Can't find node with descriptor: " + n.Descriptor +
                ", maybe you have outdated node data?");
        }

        // класс для внутреннего использования в структуре данных
        private class NodeInternal
        {
            // значение данного узла
            public readonly StringBuilder mValue = new StringBuilder();
            // уровень глубины данного узла
            public int mCurrentLevel = 0;
            // узел-родитель
            public NodeInternal? mPredecessor = null;
            // узлы-потомки
            public readonly List<NodeInternal> mNodes = new List<NodeInternal>();
            // список индексов строки, которая заканчивается данным узлом;
            // индексом может быть, к примеру, номер строки
            public readonly SortedSet<long> mIndexes = new SortedSet<long>();
        }
    }
}

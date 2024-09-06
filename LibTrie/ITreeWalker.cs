using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibTrie
{
    /// <summary>
    /// Интерфейс объекта, который может идти по дереву/графу. Будем называть его "обходчик".
    /// Сделан для отделения внутренностей структуры графа/дерева от внешнего мира.
    /// </summary>
    /// <typeparam name="T">тип значений, которые хранит дерево/граф</typeparam>
    public interface ITreeWalker<T>
    {
        /// <summary>
        /// Метод для получения текущего узла.
        /// </summary>
        /// <returns>текущий узел, на котором сейчас находится обходчик</returns>
        public Node<T>? GetCurrentNode();

        /// <summary>
        /// Переходит на узел-потомок текущего узла по значению узла-потомка. Возвращенный
        /// узел становится текущим (если не достигнут лист дерева, иначе этот аспект без изменений).
        /// </summary>
        /// <param name="descendantValue">значение узла-потомка</param>
        /// <returns>узел-потомок текущего узла</returns>
        public Node<T>? GoToNode(T descendantValue);

        /// <summary>
        /// Переходит к узлу-предку данного узла. Возвращенный узел становится текущим
        /// (если не достигнут корень дерева, иначе этот аспект без изменений).
        /// </summary>
        /// <returns>узел-предок текущего узла</returns>
        public Node<T>? GoToPredecessor();

        /// <summary>
        /// Данный метод делает так, чтобы текущим узлом был корневой.
        /// </summary>
        public void GoToRoot()
        {
            Node<T>? node = GetCurrentNode();
            while (node != null) node = GoToPredecessor();
        }

        /// <summary>
        /// Обходит дерево в глубину. Например, для дерева
        ///         1
        ///    /    |    \ 
        ///   2     3     4
        ///  /|\   /|\   /|\
        /// 5 6 7 8 9 0 1 2 3
        /// вывод будет такой:
        /// 1,2,5,6,7,3,8,9,0,4,1,2,3
        /// Выполнение данного метода использует другие методы интерфейса, а потому
        /// может влиять на то, какой узел является текущим.
        /// </summary>
        /// <param name="func">функция, принимающая на вход узел, а на выходе выдающая true, если продолжать итерацию,
        /// иначе - false</param>
        /// <param name="returnToRoot">если true, то после окончания (или прекращения, если func вернула false) текущим узлом
        /// ставится корневой</param>
        public virtual void WalkDepthFirst(Func<Node<T>, bool> func, bool returnToRoot = true)
        {
            if (GetCurrentNode() == null) return;
            TraverseInternal(func, true);
            if (returnToRoot) GoToRoot();
        }

        /// <summary>
        /// Обходит дерево начиная с текущего узла в порядке по уровням (рядам). Например, для дерева
        ///         1
        ///    /    |    \ 
        ///   2     3     4
        ///  /|\   /|\   /|\
        /// 5 6 7 8 9 0 1 2 3
        /// вывод будет такой:
        /// 1,2,3,4,5,6,7,8,9,0,1,2,3
        /// Выполнение данного метода использует другие методы интерфейса, а потому
        /// может влиять на то, какой узел является текущим.
        /// </summary>
        /// <param name="func">функция, принимающая на вход узел, а на выходе выдающая true, если продолжать итерацию,
        /// иначе - false</param>
        /// <param name="returnToRoot">если true, то после окончания (или прекращения, если func вернула false) текущим узлом
        /// ставится корневой</param>
        public virtual void WalkRowOrder(Func<Node<T>, bool> func, bool returnToRoot = true)
        {
            if (GetCurrentNode() == null) return;
            TraverseInternal(func, false);
            if (returnToRoot) GoToRoot();
        }

        private void TraverseInternal(Func<Node<T>, bool> func, bool useAsStack)
        {
            if (GetCurrentNode() == null)
                return;
            var orderer = new LinkedList<Node<T>>();
            orderer.AddLast(GetCurrentNode()!);
            while (orderer.Any())
            {
                var node = useAsStack ? orderer.Last!.Value : orderer.First!.Value;
                if (useAsStack)
                    orderer.RemoveLast();
                else
                    orderer.RemoveFirst();
                if (!func(node)) break;
                if (useAsStack)
                    for (int i = node.Descendants.Count - 1; i >= 0; i--)
                    {
                        orderer.AddLast(GoToNode(node.Descendants[i])!);
                        GoToPredecessor();
                    }
                else
                    foreach (var child in node.Descendants)
                    {
                        orderer.AddLast(GoToNode(child)!);
                        GoToPredecessor();
                    }
            }
        }
    }

    /// <summary>
    /// Абстракция от узла дерева. Представляет собой узел со значением и узлами-потомками,
    /// к которым можно перейти по их значениям.
    /// </summary>
    /// <typeparam name="T">тип значений, которые хранит узел</typeparam>
    /// <param name="Value">значение текущего узла</param>
    /// <param name="Descendants">значения узлов-потомков</param>
    public class Node<T>
    {
        public readonly T Value;
        public readonly IReadOnlyList<T> Descendants;

        public Node(T value, IReadOnlyList<T> descendants)
        {
            Value = value;
            Descendants = descendants;
        }

        public override bool Equals(object? obj)
        {
            return obj is Node<T> node &&
                   Value?.Equals(node.Value) == true &&
                   Enumerable.SequenceEqual(Descendants, node.Descendants);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value, Descendants);
        }
    }
}

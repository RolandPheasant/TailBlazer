using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TailBlazer.Infrastucture
{
    public class DistinctPathTrie
    {
        private static readonly char[] DirectorySeparators =
        {
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar
        };

        public DistinctPathTrie(IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                Insert(path);
            }
        }

        private Node Root { get; } = new Node(string.Empty);

        public string GetDistinctPath(string path)
        {
            var result = GetDistinctPath(Root, new Stack<string>(path.Split(DirectorySeparators)));

            return string.Join(Path.DirectorySeparatorChar.ToString(), result);
        }

        private static Stack<string> GetDistinctPath(Node node, Stack<string> path)
        {
            return GetDistinctPath(node, path, result: new Stack<string>());
        }

        private static Stack<string> GetDistinctPath(Node node, Stack<string> path, Stack<string> result)
        {
            if (!path.Any())
            {
                return result;
            }

            if (result == null)
            {
                result = new Stack<string>();
            }

            var part = path.Pop();

            result.Push(part);

            Node childNode;
            if (node.Children.TryGetValue(part, out childNode) && childNode.Count > 1)
            {
                return GetDistinctPath(childNode, path, result);
            }

            return result;
        }

        public void Insert(string path)
        {
            Insert(Root, path: new Stack<string>(path.Split(DirectorySeparators)));
        }

        private static void Insert(Node node, Stack<string> path)
        {
            if (!path.Any())
            {
                return;
            }

            var part = path.Pop();

            Node childNode;
            if (!node.Children.TryGetValue(part, out childNode))
            {
                node.Children.Add(part, childNode = new Node(part));
            }

            childNode.Count++;

            Insert(childNode, path);
        }

        private class Node
        {
            public Node(string value)
            {
                Value = value;
                Children = new Dictionary<string, Node>(StringComparer.OrdinalIgnoreCase);
            }

            private string Value { get; }

            public int Count { get; set; }

            public IDictionary<string, Node> Children { get; }

            public override string ToString()
            {
                return $"Value: {Value}, Children: {Children.Count}";
            }
        }
    }
}

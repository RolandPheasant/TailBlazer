using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TailBlazer.Infrastructure;

public class FileNamer
{
    private static readonly char[] DirectorySeparators =
    {
        Path.DirectorySeparatorChar,
        Path.AltDirectorySeparatorChar
    };

    public FileNamer(IEnumerable<string> paths)
    {
        foreach (var path in paths)
        {
            Insert(path);
        }
    }

    private Node Root { get; } = new Node(string.Empty);

    public string GetName(string path)
    {
        return CombinePath(GetName(Root, path: new Stack<string>(path.Split(DirectorySeparators))));
    }

    private static string CombinePath(Stack<string> path)
    {
        var parts = new List<string>(3);

        parts.Add(path.First());

        if (path.Count > 1)
        {
            if (path.Count > 2)
            {
                parts.Add("..");
            }

            parts.Add(path.Last());
        }

        return string.Join(Path.DirectorySeparatorChar.ToString(), parts);
    }

    private static Stack<string> GetName(Node node, Stack<string> path)
    {
        return GetName(node, path, result: new Stack<string>());
    }

    private static Stack<string> GetName(Node node, Stack<string> path, Stack<string> result)
    {
        if (result == null)
        {
            result = new Stack<string>();
        }

        if (path.Count == 0)
        {
            return result;
        }

        var part = path.Pop();

        result.Push(part);

        Node childNode;
        if (node.Children.TryGetValue(part, out childNode) && childNode.Count > 1)
        {
            return GetName(childNode, path, result);
        }

        return result;
    }

    public void Insert(string path)
    {
        Insert(Root, path: new Stack<string>(path.Split(DirectorySeparators)));
    }

    private static void Insert(Node node, Stack<string> path)
    {
        if (path.Count == 0)
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
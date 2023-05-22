using System;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectTree : ScriptableObject, ISerializationCallbackReceiver
{
    public List<SerializableNode> serializedNodes = new();

    public Node Root { get; private set; } = new();

    public void OnBeforeSerialize()
    {
        serializedNodes.Clear();
        AddNodeToSerializedNodes(Root);
    }

    public void OnAfterDeserialize()
    {
        if (serializedNodes.Count > 0)
            Root = ReadNodeFromSerializedNodes(0);
        else
            Root = new Node();
    }

    private void AddNodeToSerializedNodes(Node n)
    {
        var serializedNode = new SerializableNode
        {
            target = n.Target,
            childCount = n.Children.Count,
            indexOfFirstChild = serializedNodes.Count + 1
        };
        serializedNodes.Add(serializedNode);
        foreach (var child in n.Children)
            AddNodeToSerializedNodes(child);
    }

    private Node ReadNodeFromSerializedNodes(int index)
    {
        var serializedNode = serializedNodes[index];
        var children = new List<Node>();

        var node = new Node
        {
            Target = serializedNode.target,
            Children = children
        };

        for (var i = 0; i != serializedNode.childCount; i++)
            node.AddChild(ReadNodeFromSerializedNodes(serializedNode.indexOfFirstChild + i));

        return node;
    }

    public class Node
    {
        public List<Node> Children = new();
        public Node Parent;
        public GameObject Target;

        public Node AddChild()
        {
            var child = new Node { Parent = this };
            Children.Add(child);
            return child;
        }

        public Node AddChild(Node child)
        {
            child.Parent = this;
            Children.Add(child);
            return child;
        }

        public void Delete()
        {
            Parent?.Children.Remove(this);
        }
    }

    [Serializable]
    public struct SerializableNode
    {
        public GameObject target;
        public int childCount;
        public int indexOfFirstChild;
    }
}
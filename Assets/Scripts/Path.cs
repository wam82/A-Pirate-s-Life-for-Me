using UnityEngine;

public class Path : MonoBehaviour
{
    private Node[] _nodes;

    public Node[] Nodes
    {
        get
        {
            if (_nodes == null || _nodes.Length == 0)
            {
                _nodes = GetComponentsInChildren<Node>();
            }
            return _nodes;
        }
        private set => _nodes = value;
    }

    public int NodeIndex { get; set; }
    public Node CurrentNode { get; set; }
    public Node LastNode { get; set; }
}

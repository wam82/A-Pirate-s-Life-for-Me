using UnityEngine;

public class Path : MonoBehaviour
{
    private void Start()
    {
        Nodes = GetComponentsInChildren<Node>();
    }
    
    public Node[] Nodes { get; private set; }

    public int NodeIndex { get; set; }

    public Node CurrentNode { get; set; }

    public Node LastNode { get; set; }
}

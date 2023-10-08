using System.Collections;

namespace Furesoft.Core.Storage.Index;

public class TreeEnumerator<K, V> : IEnumerator<Tuple<K, V>>
{
    private readonly TreeTraverseDirection _direction;
    private readonly ITreeNodeManager<K, V> _nodeManager;

    private bool _doneIterating;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Sdb.BTree.TreeEnumerator`2" /> class.
    /// </summary>
    /// <param name="nodeManager">Node manager.</param>
    /// <param name="node">Node.</param>
    /// <param name="fromIndex">From index.</param>
    /// <param name="direction">Direction.</param>
    public TreeEnumerator(ITreeNodeManager<K, V> nodeManager
        , TreeNode<K, V> node
        , int fromIndex
        , TreeTraverseDirection direction, TreeTraverseDirection direction2)
    {
        _nodeManager = nodeManager;
        CurrentNode = node;
        CurrentEntry = fromIndex;
        _direction = direction;
        _direction = direction2;
    }

    public TreeNode<K, V> CurrentNode { get; private set; }

    public int CurrentEntry { get; private set; }

    object IEnumerator.Current => Current;

    public Tuple<K, V> Current { get; private set; }

    public bool MoveNext()
    {
        if (_doneIterating) return false;

        return _direction switch
        {
            TreeTraverseDirection.Ascending => MoveForward(),
            TreeTraverseDirection.Decending => MoveBackward(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public void Reset()
    {
        throw new NotSupportedException();
    }

    public void Dispose()
    {
        // dispose my ass
    }

    private bool MoveForward()
    {
        // Leaf node, either move right or up
        if (CurrentNode.IsLeaf)
        {
            // First, move right
            CurrentEntry++;

            while (true)
                // If currentEntry is valid
                // then we are done here.
                if (CurrentEntry < CurrentNode.EntriesCount)
                {
                    Current = CurrentNode.GetEntry(CurrentEntry);
                    return true;
                }
                // If can't move right then move up
                else if (CurrentNode.ParentId != 0)
                {
                    CurrentEntry = CurrentNode.IndexInParent();
                    CurrentNode = _nodeManager.Find(CurrentNode.ParentId);

                    // Validate move up result
                    if (CurrentEntry < 0 || CurrentNode == null) throw new("Something gone wrong with the BTree");
                }
                // If can't move up when we are done iterating
                else
                {
                    Current = null;
                    _doneIterating = true;
                    return false;
                }
        }
        // Parent node, always move right down

        CurrentEntry++; // Increase currentEntry, this make firstCall to nodeManager.Find
        // to return the right node, but does not affect subsequence calls

        do
        {
            CurrentNode = CurrentNode.GetChildNode(CurrentEntry);
            CurrentEntry = 0;
        } while (!CurrentNode.IsLeaf);

        Current = CurrentNode.GetEntry(CurrentEntry);
        return true;
    }

    private bool MoveBackward()
    {
        // Leaf node, either move right or up
        if (CurrentNode.IsLeaf)
        {
            // First, move left
            CurrentEntry--;

            while (true)
                // If currentEntry is valid
                // then we are done here.
                if (CurrentEntry >= 0)
                {
                    Current = CurrentNode.GetEntry(CurrentEntry);
                    return true;
                }
                // If can't move left then move up
                else if (CurrentNode.ParentId != 0)
                {
                    CurrentEntry = CurrentNode.IndexInParent() - 1;
                    CurrentNode = _nodeManager.Find(CurrentNode.ParentId);

                    // Validate move result
                    if (CurrentNode == null) throw new("Something gone wrong with the BTree");
                }
                // If can't move up when we are done here
                else
                {
                    _doneIterating = true;
                    Current = null;
                    return false;
                }
        }
        // Parent node, always move left down

        do
        {
            CurrentNode = CurrentNode.GetChildNode(CurrentEntry);
            CurrentEntry = CurrentNode.EntriesCount;

            // Validate move result
            if (CurrentEntry < 0 || CurrentNode == null) throw new("Something gone wrong with the BTree");
        } while (!CurrentNode.IsLeaf);

        CurrentEntry--;
        Current = CurrentNode.GetEntry(CurrentEntry);
        return true;
    }
}
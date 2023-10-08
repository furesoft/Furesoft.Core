namespace Furesoft.Core.Storage.Index;

public sealed class TreeDiskNodeManager<K, V> : ITreeNodeManager<K, V>
{
    private readonly Dictionary<uint, TreeNode<K, V>> _dirtyNodes = new();
    private readonly Queue<TreeNode<K, V>> _nodeStrongRefs = new();
    private readonly Dictionary<uint, WeakReference<TreeNode<K, V>>> _nodeWeakRefs = new();
    private readonly TreeDiskNodeSerializer<K, V> _serializer;
    private readonly int maxStrongNodeRefs = 200;
    private readonly IRecordStorage recordStorage;
    private int cleanupCounter;

    /// <summary>
    ///     Construct a tree from given storage, using default comparer of key
    /// </summary>
    public TreeDiskNodeManager(ISerializer<K> keySerializer
        , ISerializer<V> valueSerializer
        , IRecordStorage nodeStorage)
        : this(keySerializer, valueSerializer, nodeStorage, Comparer<K>.Default)
    {
    }

    /// <summary>
    ///     Construct a tree from given storage, using the specified comparer of key
    /// </summary>
    /// <param name="keySerializer">Tool to serialize node keys.</param>
    /// <param name="valueSerializer">
    ///     Tool to serialize node values
    ///     <param>
    ///         <param name="recordStorage">Underlying tool for storage.</param>
    ///         <param name="keyComparer">Key comparer.</param>
    public TreeDiskNodeManager(ISerializer<K> keySerializer
        , ISerializer<V> valueSerializer
        , IRecordStorage recordStorage
        , IComparer<K> keyComparer)
    {
        if (recordStorage == null)
            throw new ArgumentNullException("nodeStorge");

        this.recordStorage = recordStorage;
        _serializer = new(this, keySerializer, valueSerializer);
        KeyComparer = keyComparer;
        EntryComparer = Comparer<Tuple<K, V>>.Create((a, b) => { return KeyComparer.Compare(a.Item1, b.Item1); });

        // The first record of nodeStorage stores id of root node,
        // if this record do not exist at the time this index instanitate,
        // then attempt to create it
        var firstBlockData = recordStorage.Find(1u);
        if (firstBlockData != null)
            RootNode = Find(BufferHelper.ReadBufferUInt32(firstBlockData, 0));
        else
            RootNode = CreateFirstRoot();
    }

    public ushort MinEntriesPerNode { get; } = 36;

    public IComparer<Tuple<K, V>> EntryComparer { get; }

    public IComparer<K> KeyComparer { get; }

    public TreeNode<K, V> RootNode { get; private set; }

    //
    // Public Methods
    //

    public TreeNode<K, V> Create(IEnumerable<Tuple<K, V>> entries, IEnumerable<uint> childrenIds)
    {
        // Create new record
        TreeNode<K, V> node = null;

        recordStorage.Create(nodeId =>
        {
            // Instantiate a new node
            node = new(this, nodeId, 0, entries, childrenIds);

            // Always keep reference to any node that we created
            OnNodeInitialized(node);

            // Return its deserialized value
            return _serializer.Serialize(node);
        });

        if (node == null) throw new("dataGenerator never called by nodeStorage");

        return node;
    }

    public TreeNode<K, V> Find(uint id)
    {
        // Check if the node is being held in memory,
        // if it does then return it
        if (_nodeWeakRefs.ContainsKey(id))
        {
            if (_nodeWeakRefs[id].TryGetTarget(out var node))
                return node;
            // node deallocated, remove weak reference
            _nodeWeakRefs.Remove(id);
        }

        // Not is not in memory, go get it
        var data = recordStorage.Find(id);
        if (data == null) return null;
        var dNode = _serializer.Deserialize(id, data);

        // Always keep reference to node we created
        OnNodeInitialized(dNode);
        return dNode;
    }

    public TreeNode<K, V> CreateNewRoot(K key, V value, uint leftNodeId, uint rightNodeId)
    {
        // Create new node as normal
        var node = Create(new Tuple<K, V>[]
        {
            new(key, value)
        }, new[]
        {
            leftNodeId,
            rightNodeId
        });

        // Make it the root node
        RootNode = node;
        recordStorage.Update(1u, LittleEndianByteOrder.GetBytes(node.Id));

        // Then return it
        return RootNode;
    }

    public void MakeRoot(TreeNode<K, V> node)
    {
        RootNode = node;
        recordStorage.Update(1u, LittleEndianByteOrder.GetBytes(node.Id));
    }

    public void Delete(TreeNode<K, V> node)
    {
        if (node == RootNode) RootNode = null;

        recordStorage.Delete(node.Id);

        if (_dirtyNodes.ContainsKey(node.Id)) _dirtyNodes.Remove(node.Id);
    }

    public void MarkAsChanged(TreeNode<K, V> node)
    {
        if (false == _dirtyNodes.ContainsKey(node.Id)) _dirtyNodes.Add(node.Id, node);
    }

    public void SaveChanges()
    {
        foreach (var kv in _dirtyNodes) recordStorage.Update(kv.Value.Id, _serializer.Serialize(kv.Value));

        _dirtyNodes.Clear();
    }

    //
    // Private Methods
    //

    private TreeNode<K, V> CreateFirstRoot()
    {
        // Write down the id of first node into the first block
        recordStorage.Create(LittleEndianByteOrder.GetBytes((uint) 2));

        // Return a new node, this node should has id of 2
        return Create(null, null);
    }

    private void OnNodeInitialized(TreeNode<K, V> node)
    {
        // Keep a weak reference to it
        _nodeWeakRefs.Add(node.Id, new(node));

        // Keep a strong reference to prevent weak refs from being dellocated
        _nodeStrongRefs.Enqueue(node);

        // Clean up strong refs if we been holding too many of them
        if (_nodeStrongRefs.Count >= maxStrongNodeRefs)
            while (_nodeStrongRefs.Count >= maxStrongNodeRefs / 2f)
                _nodeStrongRefs.Dequeue();

        // Clean up weak refs
        if (cleanupCounter++ >= 1000)
        {
            cleanupCounter = 0;
            var tobeDeleted = new List<uint>();
            foreach (var kv in _nodeWeakRefs)
                if (false == kv.Value.TryGetTarget(out var target))
                    tobeDeleted.Add(kv.Key);

            foreach (var key in tobeDeleted) _nodeWeakRefs.Remove(key);
        }
    }
}
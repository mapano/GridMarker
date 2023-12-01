using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TilePoolManager : MonoBehaviour
{
    [SerializeField]
    private Sprite sprite;
    [SerializeField]
    private int initialPoolSize = 10;
    [SerializeField]
    private int maxPoolSize = 50;
    private Queue<GameObject> _tilePool = new Queue<GameObject>();
    private float _pruneInterval = 60f; 
    private float _lastPruneTime;

    public void Init()
    {
        InitializePool();
        _lastPruneTime = Time.time;
    }

    void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject pooledTile = CreatePooledTile();
            pooledTile.SetActive(false);
            _tilePool.Enqueue(pooledTile);
        }
    }

    GameObject CreatePooledTile()
    {
        GameObject tempTile = new GameObject("TemporaryTile");
        SpriteRenderer renderer = tempTile.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        return tempTile;
    }

    public GameObject GetPooledTile()
    {
        if (_tilePool.Count > 0)
        {
            return _tilePool.Dequeue();
        }
        if (_tilePool.Count < maxPoolSize)
        {
            return CreatePooledTile();
        }

        return null;
    }

    public void ReturnPooledTile(GameObject tile)
    {
        if (_tilePool.Count < maxPoolSize)
        {
            tile.SetActive(false);
            _tilePool.Enqueue(tile);
        }
        else
        {
            Destroy(tile);
        }
    }

    void Update()
    {
        if (Time.time - _lastPruneTime > _pruneInterval)
        {
            PrunePool();
            _lastPruneTime = Time.time;
        }
    }

    void PrunePool()
    {
        while (_tilePool.Count > initialPoolSize)
        {
            GameObject objToDestroy = _tilePool.Dequeue();
            Destroy(objToDestroy);
        }
    }
}

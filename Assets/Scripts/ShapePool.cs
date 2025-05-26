// /*
// Created by Darsan
// */

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShapePool : MonoBehaviour
{
    [SerializeField] private Transform _content;
    [SerializeField] private TileHolder _tileHolderPrefab;
    [SerializeField] private float _maxTileHeight;
    [SerializeField] private float _space;



    private readonly List<TileHolder> _tiles  = new List<TileHolder>();

    public IEnumerable<Shape> Shapes => _tiles.Select(holder => holder.Shape);


    private void Update()
    {
        var lastPos = Vector3.zero;
        for (var i = 0; i < _tiles.Count; i++)
        {
            _tiles[i].transform.localPosition =
                Vector3.MoveTowards(_tiles[i].transform.localPosition, lastPos + (i > 0 ? Vector3.right * _space : Vector3.zero) + Vector3.right * _tiles[i].Size.x / 2, Time.deltaTime * 40);
            lastPos += (i > 0 ? Vector3.right * _space : Vector3.zero) + Vector3.right * _tiles[i].Size.x;
        }
    }

    public void ForceUpdatePositions()
    {
        var lastPos = Vector3.zero;
        for (var i = 0; i < _tiles.Count; i++)
        {
            _tiles[i].transform.localPosition =
                 lastPos + (i > 0 ? Vector3.right * _space : Vector3.zero) + Vector3.right * _tiles[i].Size.x / 2;
            lastPos += (i > 0 ? Vector3.right * _space : Vector3.zero) + Vector3.right * _tiles[i].Size.x;
        }
    }

    public void ReturnShape(Shape shape)
    {
        var tileHolder = _tiles.FirstOrDefault(holder => holder.Shape == shape);

        if (tileHolder == null)
        {
            tileHolder = Instantiate(_tileHolderPrefab,_content);
            tileHolder.MaxHeight = _maxTileHeight;
            _tiles.Insert(0,tileHolder);
            tileHolder.Shape = shape;
            tileHolder.transform.localPosition = Vector3.right * tileHolder.Size.x / 2;
        }

        tileHolder.ReturnShape();
    }


    public void AddShape(Shape shape)
    {
        var tileHolder = _tiles.FirstOrDefault(holder => holder.Shape == shape);
        if (tileHolder == null)
        {
            tileHolder = Instantiate(_tileHolderPrefab, _content);
            tileHolder.MaxHeight = _maxTileHeight;
            _tiles.Add(tileHolder);
            tileHolder.Shape = shape;
        }
        tileHolder.ReturnShape(false);
    }


    public void RemoveShape(Shape shape)
    {
        var tileHolder = _tiles.FirstOrDefault(holder => holder.Shape == shape);

        if (tileHolder != null)
        {
            if (shape.transform.parent == tileHolder.transform)
            {
                shape.transform.parent = null;
            }

            _tiles.Remove(tileHolder);
            Destroy(tileHolder.gameObject);
        }
    }

    public void Clear()
    {
        _tiles.ForEach(holder => Destroy(holder.gameObject));
        _tiles.Clear();
    }
}
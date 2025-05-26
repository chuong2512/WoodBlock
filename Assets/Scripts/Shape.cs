// /*
// Created by Darsan
// */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class Shape : MonoBehaviour, IEnumerable<Piece>, IInitializable<Shape.InitParams>
{
    public event Action<Shape> PointerDown;
    public event Action<Shape, Vector2> PointerDrag;
    public event Action<Shape> PointerUp;

    [SerializeField] private Piece _piecePrefab;

    private readonly List<Piece> _pieces = new List<Piece>();
    private bool _isFront;

    public BoxGrid BoxGrid { get; private set; }
    public Vector2Int Coordinate { get; set; }
    public string Id { get; set; } = "";

    public PieceColor PieceColor { get; set; }
    public bool Intractable { get; set; } = true;
    public bool Dragging { get; private set; }

    public bool IsFront
    {
        get => _isFront;
        set
        {
            _pieces.ForEach(piece => piece.IsFront = value);
            _isFront = value;
        }
    }

    public IEnumerator<Piece> GetEnumerator()
    {
        return _pieces.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool Initialized { get; private set; }

    public Vector2 CenterPoint
    {
        get => transform.TransformPoint(_localCenterOfGravity);
        set
        {
            
            transform.position += (Vector3)(value - CenterPoint);
        }
    }

    public Vector2 BoundingBox { get; private set; }

    private Vector2 _localCenterOfGravity;

    public void Init(InitParams data)
    {
        if (Initialized)
        {
            return;
        }

        PieceColor = data.ShapeData.color;
        BoxGrid = new BoxGrid(data.TileSize, data.Spacing);
        Id = data.Id;
        
        foreach (var dataPiece in data.ShapeData.pieces)
        {
            var localPosition = BoxGrid.GetRelativePositionForCoordinate(dataPiece.coordinate);
            var piece = Instantiate(_piecePrefab, transform);
            piece.Shape = this;
            var inverted = BoxGrid.IsInverted(data.ShapeData.baseInverted, dataPiece.coordinate);
//            Debug.Log("Coordinate:"+dataPiece.coordinate+" Inverted:"+inverted+" Base Inverted:"+data.ShapeData.baseInverted);
            piece.Size = data.TileSize;
            piece.PieceColor = data.ShapeData.color;
            piece.LocalCoordinate = dataPiece.coordinate;
            piece.transform.localPosition = localPosition;
            piece.Inverted = inverted;
            _pieces.Add(piece);
        }


        var minY = _pieces.Min(piece => piece.transform.localPosition.y);
        var maxY = _pieces.Max(piece => piece.transform.localPosition.y);
        var minX = _pieces.Min(piece => piece.transform.localPosition.x);
        var maxX = _pieces.Max(piece => piece.transform.localPosition.x);
        BoundingBox = new Vector2(maxX-minX + BoxGrid.TileSize , maxY-minY + BoxGrid.TileSize * Mathf.Cos(30 * Mathf.Deg2Rad));
        _localCenterOfGravity = new Vector2((maxX + minX)/2,
            (maxY + minY)/2 ) - (Vector2)transform.position;

        Initialized = true;
    }


    private void Update()
    {
        if (!Intractable)
        {
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            // ReSharper disable once Unity.PreferNonAllocApi
            // ReSharper disable once PossibleNullReferenceException
            if (!Dragging && Physics2D.OverlapPointAll(Camera.main.ScreenToWorldPoint(Input.mousePosition))
                .Any(collider => collider.attachedRigidbody?.gameObject == gameObject))
            {
                Dragging = true;
                MouseDown();
                Debug.Log("Dragging");
            }
        }

        if (Dragging&&Input.GetMouseButtonUp(0))
        {
            Dragging = false;
            MouseUp();
        }
    }

    private void MouseDown()
    {

        PointerDown?.Invoke(this);
        InputController.PointerDrag += InputControllerOnPointerDrag;
    }

    private void InputControllerOnPointerDrag(Vector2 delta)
    {
        PointerDrag?.Invoke(this, delta);
    }

    private void MouseUp()
    {
        InputController.PointerDrag -= InputControllerOnPointerDrag;
        PointerUp?.Invoke(this);
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(CenterPoint,new Vector3(BoundingBox.x,BoundingBox.y,0.1f));
    }

    public struct InitParams
    {
        public ShapeData ShapeData { get; set; }
        public float TileSize { get; set; }
        public float Spacing { get; set; }
        public string Id { get; set; }
    }
}

public partial class Shape
{
    public void RemovePiece(Piece piece, bool destroyPiece = true)
    {
        _pieces.Remove(piece);
        if (destroyPiece)
            Destroy(piece.gameObject);
    }

    public IEnumerable<Vector2Int> GetPieceInWorldCoordinates(Vector2Int? targetShapeCoordinate = null)
    {
        return _pieces.Select(piece => BoxGrid.RelativeCoordinate(targetShapeCoordinate ?? Coordinate
            , piece.LocalCoordinate, Vector2Int.zero));
    }

    public Vector2 GrapShapeCoordinate
    {
        get
        {
            var centerOfGravity = BoxGrid.GetCenterOfGravity(GetPieceInWorldCoordinates(Vector2Int.zero));
            var nearestPiece = _pieces.First();
            var nearestDistance = ((Vector2)nearestPiece.LocalCoordinate - centerOfGravity).sqrMagnitude;

            foreach (var piece in _pieces)
            {
                var distance = (piece.LocalCoordinate - centerOfGravity).sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestPiece = piece;
                }
            }
            Debug.Log(nearestPiece.LocalCoordinate);
            return nearestPiece.LocalCoordinate;
        }
    }
}


public static class ShapeExtensions
{
    public static ShapeData ToShapeData(this Shape shape, string id = null)
    {
        return new ShapeData
        {
            pieces = shape.Select(piece => new PieceData
            {
                coordinate = piece.LocalCoordinate
            }).ToList(),
            baseInverted = shape.First().Inverted,
            color = shape.PieceColor,
            id = string.IsNullOrEmpty(shape.Id)  ? UUID.GetId() : shape.Id,
            coordinate = shape.Coordinate
        };
    }

    public static bool IsEqualPieceData(this ShapeData shape, ShapeData data)
    {
        return shape.pieces.HasSameElementsSameNumber(data.pieces);
    }
}


[Serializable]
public struct ShapeData:IEnumerable<PieceData>
{
    public string id;
    public List<PieceData> pieces;
    public PieceColor color;
    public Vector2Int coordinate;
    public bool baseInverted;
    public IEnumerator<PieceData> GetEnumerator()
    {
        return pieces.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

[Serializable]
public struct ShapeCoordinate
{
    public string shapeId;
    public Vector2Int coordinate;
}
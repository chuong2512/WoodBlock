// /*
// Created by Darsan
// */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShapeManager : MonoBehaviour
{
    public static ShapeManager Instance { get; private set; }

    [SerializeField] private ShapePool _pool;
    [SerializeField] private GameBoard _board;
    [SerializeField] private Shape _shapePrefab;


    public bool Dragging { get; private set; }
    public bool LastOnBoardDragShape { get;private set; }
    public Vector2Int LastDragShapeCoordinate { get; private set; }
    public bool Active { get; set; }
    private readonly Dictionary<string,Vector2Int> _shapeVsCoordinatesDict=new Dictionary<string, Vector2Int>();

    private readonly List<Shape> _addedShapesOnBoardOrder = new List<Shape>();

    private void Awake()
    {
        Instance = this;
    }

    public void SetUp(ShapeData[] datas)
    {

        _addedShapesOnBoardOrder.Clear();
        _pool.Clear();
        _shapeVsCoordinatesDict.Clear();
        foreach (var data in datas)
        {
            var shape = Instantiate(_shapePrefab);
            shape.Init(new Shape.InitParams
            {
                ShapeData = data,
                Spacing = _board.Spacing,
                TileSize = _board.TileSize,
                Id = data.id
            });
            _shapeVsCoordinatesDict.Add(data.id,data.coordinate);
            shape.PointerDown +=ShapeOnPointerDown;
            _pool.AddShape(shape);
        }
        _pool.ForceUpdatePositions();
    }

    private void ShapeOnPointerDown(Shape shape)
    {
        if(!Active)
            return;
        if(Dragging)
            return;

        Dragging = true;

        if (_board.CurrentShapes.Any(s => s == shape))
        {
            LastDragShapeCoordinate = shape.Coordinate;
            LastOnBoardDragShape = true;
            _board.RemoveShape(shape);
            _addedShapesOnBoardOrder.Remove(shape);
        }
        else
        {
            LastOnBoardDragShape = false;
            
            shape.transform.localScale *= 1/ shape.transform.lossyScale.x;
        }

        shape.IsFront = true;
    
        shape.PointerDrag +=ShapeOnPointerDrag;
        shape.PointerUp+=ShapeOnPointerUp;
    }

    public void ReturnShapeToPool(Shape shape)
    {
        _pool.ReturnShape(shape);
    }

    public Shape GetShapeFromPool(Shape shape)
    {
        _pool.RemoveShape(shape);
        return shape;
    }

    [ContextMenu("Show Hint")]
    public void ShowHint()
    {
        var shape = _pool.Shapes.FirstOrDefault();
        if(!shape)
            return;

        _pool.RemoveShape(shape);

        var coordinate = _shapeVsCoordinatesDict[shape.Id];


        var list = shape.Select(piece => BoxGrid.RelativeCoordinate(coordinate, piece.LocalCoordinate, Vector2Int.zero)).ToList();

        var shapes = _board.BackgroundShapeTiles.Where(tile => list.Contains(tile.Holder.LocalCoordinate) && tile.Tile!=null && ((Piece)tile.Tile).Shape != null).Select(tile => ((Piece)tile.Tile).Shape).Distinct().ToList();


        foreach (var s in shapes)
        {
            _board.RemoveShape(s);
            _pool.ReturnShape(s);
            Debug.Log("Return Shape");
        }
        shape.Intractable = false;
        var targetPos = _board[coordinate].transform.position;
        var startPos = shape.transform.position;
        var startScale = shape.transform.localScale;
        StartCoroutine(SimpleCoroutine.MoveTowardsEnumerator(0, 1, n =>
        {
            shape.transform.position = Vector3.Lerp(startPos, targetPos, n);
            shape.transform.localScale = Vector3.Lerp(startScale, Vector3.one, n);
        }, () =>
        {
            _addedShapesOnBoardOrder.Add(shape);
            _board.AddShape(shape, coordinate);
        }, speed:7f));
    }

    public bool HasUndo() => _addedShapesOnBoardOrder.Any(shape => shape.Intractable);

    public void Undo()
    {
        if (!HasUndo())
        {
            return;
        }

        var s = _addedShapesOnBoardOrder.First(shape => shape.Intractable);
        _addedShapesOnBoardOrder.Remove(s);
        _board.RemoveShape(s);
        _pool.ReturnShape(s);
    }

    private void ShapeOnPointerUp(Shape shape)
    {
        _board.ClearHighlights();
        var matchCoordinateForShape = _board.GetNearestMatchCoordinateForShape(shape, out _);
        if (matchCoordinateForShape != null)
        {

            if (!LastOnBoardDragShape)
            {
                _pool.RemoveShape(shape);
            }

            _board.AddShape(shape, (Vector2Int) matchCoordinateForShape);
            _addedShapesOnBoardOrder.Add(shape);
          
        }
        else
        {
           
                _pool.ReturnShape(shape);
        }
        shape.IsFront = false;

        Dragging = false;
        shape.PointerDrag -= ShapeOnPointerDrag;
        shape.PointerUp -= ShapeOnPointerUp;
    }

    private void ShapeOnPointerDrag(Shape shape, Vector2 delta)
    { 
        shape.transform.position += (Vector3)delta;
        _board.HandleNearestMatchCoordinate(shape);
    }
}
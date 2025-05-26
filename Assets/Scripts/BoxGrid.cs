// /*
// Created by Darsan
// */

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoxGrid
{
    public float TileSize { get; }
    public float Spacing { get; set; }


    public BoxGrid(float tileSize, float spacing)
    {
        TileSize = tileSize;
        Spacing = spacing;
    }


    public Vector2Int InverseCoordinate(Vector2Int baseCoordinate, Vector2Int relCoordinate)
    {
        return relCoordinate * -1;
    }


    public static Vector2Int RelativeCoordinate(Vector2Int baseCoordinate, Vector2Int relCoordinate,
        Vector2Int newBaseCoordinate)
    {
        return baseCoordinate + relCoordinate - newBaseCoordinate;
    }

    public Vector2 GetRelativePositionForCoordinate(Vector2Int coordinate)
    {
        var x = coordinate.x * (TileSize / 2 + Spacing / Mathf.Sin(60 * Mathf.Deg2Rad));
        var y = coordinate.y * (TileSize * Mathf.Cos(30 * Mathf.Deg2Rad) + Spacing);
        return new Vector2(x,y);
    }


    public Vector2 GetRelativePositionForCoordinate(Vector2 coordinate)
    {
        var x = coordinate.x * (TileSize / 2 + Spacing / Mathf.Sin(60 * Mathf.Deg2Rad));
        var y = coordinate.y * (TileSize * Mathf.Cos(30 * Mathf.Deg2Rad) + Spacing);
        return new Vector2(x,y);
    }


    public Vector2Int GetNearestCoordinateForRelativePosition(Vector2 pos)
    {
        var x = pos.x/ (TileSize / 2 + Spacing / Mathf.Sin( 60 * Mathf.Deg2Rad));
        var y = pos.y/ (TileSize * Mathf.Cos(30 * Mathf.Deg2Rad) + Spacing);
        return new Vector2Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
    }


    public static bool IsInverted(bool baseInverted, Vector2Int relCoordinate)
    {
        var odd = Mathf.Abs(relCoordinate.x + relCoordinate.y) % 2 == 1;
//        Debug.Log(Mathf.Abs(relCoordinate.x + relCoordinate.y));
        return odd ? !baseInverted : baseInverted;
    }

    public static IEnumerable<Vector2Int> GetAdjacentCoordinates(IEnumerable<Vector2Int> coordinates)
    {
        var list = coordinates.ToList();
        return list.SelectMany(i => GetAdjacentCoordinates(i).Except(list)).Distinct();
    }

    public static IEnumerable<Vector2Int> GetAdjacentCoordinates(Vector2Int vec)
    {
        return new[]
        {
            vec + new Vector2Int(0, 1), vec + new Vector2Int(1, 0), vec + new Vector2Int(0, -1),
            vec + new Vector2Int(-1, 0)
        };
    }

    public static bool IsAdjacent(Vector2Int vec1, Vector2Int vec2)
    {
        return GetAdjacentCoordinates(vec1).Contains(vec2);
    }

    public bool IsAdjacentCoordinateGroup(IEnumerable<Vector2Int> coordinates1, IEnumerable<Vector2Int> coordinates2)
    {
        return GetAdjacentCoordinates(coordinates1).Intersect(coordinates2).Any();
    }


    public bool IsAdjacentShapes(ShapeData shape1, ShapeData shape2)
    {
       return IsAdjacentCoordinateGroup(
            shape1.Select(piece => RelativeCoordinate(shape1.coordinate, piece.coordinate, Vector2Int.zero)),
            shape2.Select(piece => RelativeCoordinate(shape2.coordinate, piece.coordinate, Vector2Int.zero)));
    }

    

    public static Vector2 GetCenterOfGravity(IEnumerable<Vector2Int> coordinates)
    {
        var list = coordinates.ToList();
        return list.Aggregate(Vector2.zero, (total, vec) => total + vec) / list.Count;
    }


    public IEnumerable<ShapeData> GetMaxAdjacentShapeGroup(IEnumerable<ShapeData> shapes, bool canBeSingle = false)
    {
        var list = GetAdjacentShapeGroup(shapes).Select(enumerable => enumerable.ToList()).ToList();
        var max = list.OrderByDescending(list1 => list1.Count).First();
        return canBeSingle || max.Count > 1 ? max : Enumerable.Empty<ShapeData>();
    }

    public IEnumerable<IEnumerable<ShapeData>> GetAdjacentShapeGroup(IEnumerable<ShapeData> shape)
    {
        var list = shape.ToList();
        var coordinateGroups = new List<List<ShapeData>>();

        while (list.Count>0)
        {
            coordinateGroups.Add(new List<ShapeData>
            {
                list.First()
            });
            list.RemoveAt(0);

            var currentCoordinateGroup = coordinateGroups.Last();
            for (var j = 0; j < currentCoordinateGroup.Count; j++)
            {
                for (var i = 0; i < list.Count; i++)
                {
                    if (IsAdjacentShapes(currentCoordinateGroup[j],list[i]))
                    {
                        currentCoordinateGroup.Add(list[i]);
                        list.RemoveAt(i);
                        --i;
                    }
                }
            }
        }

        return coordinateGroups;
    }

  
}
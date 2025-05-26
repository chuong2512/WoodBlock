// /*
// Created by Darsan
// */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct BoardData : IEnumerable<Vector2Int>
{
    public List<Vector2Int> tiles;

    public IEnumerator<Vector2Int> GetEnumerator()
    {
        return tiles.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct Level:ILevel
{
    [SerializeField]private List<ShapeData> _shapeDatas;
    [SerializeField]private int _levelNo;
    [SerializeField] private BoardData _boardData;

    public IEnumerable<ShapeData> Shapes => _shapeDatas;
    public BoardData Board => _boardData;

    public int LevelNo
    {
        get => _levelNo;
        set => _levelNo = value;
    }

    public bool Locked => throw new System.NotImplementedException();


    public Level(int levelNo,ShapeData[] shapeDatas,BoardData boardData) : this()
    {
        Init(levelNo, shapeDatas,boardData);
    }

    private void Init(int levelNo, ShapeData[] shapeDatas, BoardData boardData)
    {
        LevelNo = levelNo;
        _shapeDatas = shapeDatas.ToList();
        _boardData = boardData;

    }

    public Level(ILevel level) : this()
    {
        Init(level.LevelNo,
            level.Shapes.ToArray(),
  level.Board
            );
    }


}

public interface ILevel
{
    
    IEnumerable<ShapeData> Shapes{ get; }
    BoardData Board { get; }
    int LevelNo { get; }
    bool Locked { get; }
}
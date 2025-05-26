using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameBoard : Board
{
    public event Action GameResulted;

    [SerializeField] private AudioClip _successClip, _clickClip;
    [SerializeField] private GameObject _winEffect;

    private bool? _result;


    public bool Dirty { get; set; }

    public bool Completed
    {
        get => _completed;
        set
        {
            if(value == _completed)
                return;

            _completed = value;

            if(value)
            {
                GameResulted?.Invoke();
            }
        }
    }




    // ReSharper disable once NotAccessedField.Local
    private Vector2 _lastGravityCenterCoordinate;
    private bool _completed;

    public ILevel Level { get; set; }
    public float Spacing => _spacing;


    protected override void Awake()
    {
        base.Awake();
        Refresh();
    }






    public void AddHighlightCoordinates(IEnumerable<Vector2Int> coordinates)
    {
        foreach (var coordinate in coordinates)
        {
            this[coordinate].Highlight = true;
        }
    }

    public void ClearHighlights()
    {
        BoardTiles
            .Where(tile => tile.Highlight)
            .ForEach(tile => tile.Highlight = false);
    }


    protected override void OnAddShape(Shape shape)
    {
        base.OnAddShape(shape);

        if (backgroundShapeTiles.All(tile => tile.Tile))
            CompleteTheGame();
    }



    

    private void CompleteTheGame()
    {
        if (Completed)
            return;

        ShowCompleteEffects();
        Completed = true;
    }

    private void ShowCompleteEffects()
    {
        StartCoroutine(CompleteAnim());

    }

    private IEnumerator CompleteAnim()
    {
        yield return new WaitForSeconds(0.2f);

        Handheld.Vibrate();
        PlayClipIfCan(_successClip);

        var gravity = BoxGrid.GetCenterOfGravity(backgroundShapeTiles.Select(tile => tile.Holder.LocalCoordinate));

        var position = BoxGrid.GetRelativePositionForCoordinate(gravity);
        Instantiate(_winEffect, position, Quaternion.identity);


        yield return ScaleAnimMoveTowards(1.04f);
        yield return ScaleAnimLerb(1f, 10);
    }

    private IEnumerator ScaleAnimMoveTowards(float scale,float speed=1)
    {
        while (Mathf.Abs(transform.localScale.x-scale)>0.001f)
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.one * scale, speed * Time.deltaTime);
            yield return null;
        }
        transform.localScale = Vector3.one*scale;
    }

    private IEnumerator ScaleAnimLerb(float scale, float speed = 1)
    {
        var startScale = transform.localScale.x;
        var normalized = 0f;

        while (normalized<1f)
        {
            normalized = Mathf.Clamp(Mathf.Lerp(normalized, 1.3f, speed * Time.deltaTime),0,1);
            transform.localScale = Mathf.Lerp(startScale, scale, normalized) * Vector3.one;
            yield return null;
        }

        transform.localScale = Vector3.one * scale;
    }


    private void PlayClipIfCan(AudioClip clip, float vol = 0.35f)
    {
        if (AudioManager.IsSoundEnable && clip != null)
        {
            // ReSharper disable once PossibleNullReferenceException
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, vol);
        }
    }

    public void Setup(ILevel level)
    {
        ResetBoard();
        Level = level;
        

    }



    public override void ResetBoard()
    {
        base.ResetBoard();
        transform.localScale = Vector3.one;
        BoardTiles.ForEach(tile => tile.Highlight = false);
        Completed = false;
        Dirty = false;
    }
}


public enum Axis
{
    North,NorthEast,East,SouthEast
}

public static class AxisExtensions{
    public static Line ToLine(this Axis axis)
    {
        switch (axis)
        {
            case Axis.North:
                return new Line(Vector2.up);
            case Axis.NorthEast:
                return new Line(Quaternion.AngleAxis(45,-Vector3.forward)*Vector3.up);
            case Axis.East:
                return new Line(Quaternion.AngleAxis(90, -Vector3.forward) * Vector3.up);
            case Axis.SouthEast:
                return new Line(Quaternion.AngleAxis(135, -Vector3.forward) * Vector3.up);
            default:
                throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
        }
    }

    public static Vector2 ToVector(this Axis axis)
    {
        return axis.ToLine().NormalizedVec;
    }

    public static IEnumerable<Axis> All()
    {
        return Enum.GetValues(typeof(Axis)).Cast<Axis>();
    }
}

public struct ExecutionResult
{
    public Shape Shape{ get; set; }
    public Vector2Int LastCoordinate { get; set; }
}


[Serializable]
public struct PieceData
{
    public Vector2Int coordinate;

    public override bool Equals(object obj)
    {
        if (!(obj is PieceData))
        {
            return false;
        }

        var data = (PieceData)obj;

        return coordinate == data.coordinate;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
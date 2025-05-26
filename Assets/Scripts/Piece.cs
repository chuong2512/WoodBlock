using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Piece : Tile
{
    [SerializeField] protected SpriteRenderer spRenderer;

    [ConditionalField(nameof(_useSprites),false)] [SerializeField]private List<Color> _colors = new List<Color>();
    [SerializeField] private bool _useSprites;
    [ConditionalField(nameof(_useSprites), true)] [SerializeField]private List<Sprite> _sprites = new List<Sprite>();

    private PieceColor _pieceColor;
    private bool _isDecorator;

    public Shape Shape { get; set; }

    public bool IsFront
    {
        get => spRenderer.sortingLayerName == "Front";
        set => spRenderer.sortingLayerName = value ? "Front" : "Default";
    }

    public bool IsDecorator
    {
        get => _isDecorator;
        set
        {
            _isDecorator = value;
            spRenderer.color = spRenderer.color.WithAlpha(value ? 0.3f : 1f);
            GetComponents<Collider>().ForEach(c => c.enabled = !value);
        }
    }

    public PieceColor PieceColor
    {
        get => _pieceColor;
        set
        {
            _pieceColor = value;

            if (_useSprites)
            {
                var i = ((int)value) % _sprites.Count;
                spRenderer.sprite = _sprites[i];
            }
            else
            {
                var i = ((int)value) % _colors.Count;
                spRenderer.color = _colors[i];
            }
            spRenderer.color = spRenderer.color.WithAlpha(IsDecorator ? 0.3f : 1f);
        }
    }

    public override int Order => 2;

}


public enum PieceColor
{
    Green,Red,Blue,Magenta,Yellow,Cyan
}

public static class PieceColorExtensions
{


    public static IEnumerable<PieceColor> All()
    {
        return Enum.GetValues(typeof(PieceColor)).Cast<PieceColor>();
    }
}


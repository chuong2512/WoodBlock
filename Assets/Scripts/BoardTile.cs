using UnityEngine;

public class BoardTile : Tile
{
    public override int Order => _order;

    [SerializeField] private SpriteRenderer _spRenderer;
    [SerializeField] private GameObject _highlightEffect;
    [SerializeField] private int _order;
    [SerializeField]private Color[] _normalAndInvertedColors = new Color[2];

    public static bool DebugCoordinate
    {
        get;
        set;
    }

    public override bool Inverted
    {
        get => base.Inverted;
        set
        {
            _spRenderer.color = value ? _normalAndInvertedColors[0] : _normalAndInvertedColors[1];
            base.Inverted = value;
        }
    }

    private Tile _tile;
    private bool _highlight;

    public Color Color
    {
        get => _spRenderer.color;
        set => _spRenderer.color = value;
    }

    public bool Highlight
    {
        get => _highlight;
        set
        {
            _highlight = value;
            _highlightEffect.SetActive(value);
//            Color = value?Color.red : _startUpColor;
        }
    }

    public bool HideRenderer
    {
        get => !_spRenderer.enabled;
        set => _spRenderer.enabled = !value;
    }

    public Tile Tile
    {
        get => _tile;
        set
        {
            if (_tile != null)
                _tile.Holder = null;

            if (value != null)
            {
                value.Holder = this;
                value.Size = Size;
            }

            _tile = value;
        }
    }

  
    private void OnGUI()
    {
        if (!DebugCoordinate)
        {
            return;
        }
       
        var point = Camera.main.WorldToScreenPoint(transform.position);
        var guiStyle = new GUIStyle { fontSize = 25, normal = { textColor = Color.red }, };
        GUI.Label(new Rect(point.x - 20, Screen.height - point.y - 10, 40, 20), $"{LocalCoordinate.x},{LocalCoordinate.y}", guiStyle);
    }
}
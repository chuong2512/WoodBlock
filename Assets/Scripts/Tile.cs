using System;
using UnityEngine;

public abstract class Tile : MonoBehaviour
{
    [HideInInspector] [SerializeField] private Vector2Int _localCoordinate;
    private bool _inverted;

    public event Action<Tile> Clicked;

    public float Size
    {
        get => transform.localScale.x;
        set => transform.localScale = Vector3.one*value;
    }

    public virtual Vector2Int LocalCoordinate
    {
        get => _localCoordinate;
        set => _localCoordinate = value;
    }

    public Tile Holder { get; set; }

    public virtual bool Inverted
    {
        get => _inverted;
        set
        {
            transform.localEulerAngles = transform.localEulerAngles.WithZ(value ? 180 : 0);
            _inverted = value;
        }
    }

    public abstract int Order { get; }

    public virtual bool Interaction { get; set; } = true;

    public void OnClick()
    {
        Clicked?.Invoke(this);
    }
}
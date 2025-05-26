// /*
// Created by Darsan
// */

using UnityEngine;

public class TileHolder:MonoBehaviour
{
    private Shape _shape;

    public float MaxHeight { get; set; }
    public float MinWidth { get; set; } = 1.5f;

    public Shape Shape
    {
        get => _shape;
        set
        {
            value.transform.parent = transform;

            var height = value.BoundingBox.y;
            var h = Mathf.Min(height, MaxHeight);
            value.transform.localScale = Vector3.one*Mathf.Clamp01(h/height);
            Debug.Log(h / height);
            Size = new Vector2(Mathf.Clamp(value.BoundingBox.x * value.transform.localScale.x, MinWidth,Mathf.Infinity),h); ;
            _shape = value;
        }
    }

    public Vector2 Size { get; private set; }


    public void ReturnShape(bool animate=true)
    {
        var targetScale = Vector3.one * Mathf.Clamp01(Size.y / Shape.BoundingBox.y);
        if (animate)
        {
            var startScale = Shape.transform.localScale;
           
            var centerPoint = Shape.CenterPoint;


            StartCoroutine(SimpleCoroutine.MoveTowardsEnumerator(onCallOnFrame: n =>
            {
                Shape.transform.localScale = Vector3.Lerp(startScale, targetScale, n);
                Shape.CenterPoint = Vector2.Lerp(centerPoint, transform.position, n);
            },onFinished: () =>
            {
                Shape.CenterPoint = transform.position;
                Shape.transform.localScale = targetScale;
            }, speed: 20f));
        }
        else
        {
            Shape.CenterPoint = transform.position;
            Shape.transform.localScale = targetScale;
        }

    }
}
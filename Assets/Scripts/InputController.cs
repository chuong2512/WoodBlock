// /*
// Created by Darsan
// */

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputController : Singleton<InputController>
{
    public static event Action<Vector2> PointerDown;  
    public static event Action<Vector2> PointerUp;
    public static event Action<Vector2> PointerDrag;

    private Vector2 _lastMousePoint;


    public Camera Camera => Camera.main;

    public bool IgnoreUIs { get; set; } = false;

    public bool MousePress { get;private set; }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            


                if ( !IgnoreUIs &&
#if UNITY_EDITOR
                    EventSystem.current.IsPointerOverGameObject()
#else
                EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)           
#endif
                )
                    return;
                MousePress = true;
                _lastMousePoint = GetMouseWorldPoint();
                PointerDown?.Invoke(_lastMousePoint);
            // ReSharper disable once PossibleNullReferenceException
            var lastTile = Physics2D.OverlapPointAll(Camera.main.ScreenToWorldPoint(Input.mousePosition)).Where(c => c.GetComponent<Tile>() != null)
                    .Select(d => d.GetComponent<Tile>()).OrderBy(tile => tile.Order).LastOrDefault(tile => tile.Interaction);
                lastTile?.OnClick();
        }

        if (MousePress)
        {
            
            var currentPoint = GetMouseWorldPoint();
            var delta = currentPoint - _lastMousePoint;
            _lastMousePoint = currentPoint;
            PointerDrag?.Invoke(delta);
        }

        if (Input.GetMouseButtonUp(0) && MousePress)
        {
            PointerUp?.Invoke(GetMouseWorldPoint());
        }
    }

    private Vector2 GetMouseWorldPoint()
    {
        var position = Input.mousePosition;
        position.z = Mathf.Abs(Camera.transform.position.z);
        return Camera.ScreenToWorldPoint(position);
    }
}
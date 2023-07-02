using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BoardInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
    private bool _isPressed;
    private bool _hasSwiped;

    private Vector2Int _pressStartBoardPosition;

    private Board _board;


    private void Awake()
    {
        _board = GetComponent<Board>();
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            OnSpacebar();
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        var cellBackground = eventData.pointerCurrentRaycast.gameObject.GetComponent<BoardCellBackground>();
        if (cellBackground == null)
            return;

        _isPressed = true;
        _hasSwiped = false;

        _pressStartBoardPosition = cellBackground.BoardPosition;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_isPressed)
            return;

        _isPressed = false;

        if (!_hasSwiped)
            OnClick(_pressStartBoardPosition);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (!_isPressed || _hasSwiped)
            return;

        var cellBackground = eventData.pointerCurrentRaycast.gameObject?.GetComponent<BoardCellBackground>();
        if (cellBackground == null)
            return;

        var currentBoardPosition = cellBackground.BoardPosition;
        if(currentBoardPosition != _pressStartBoardPosition)
        {
            var distance = Vector2Int.Distance(_pressStartBoardPosition, currentBoardPosition);
            if (distance > 1.0f)
                return;

            _hasSwiped = true;
            OnSwipe(_pressStartBoardPosition, currentBoardPosition);
        }
    }


    private void OnClick(Vector2Int boardPosition)
    {
        Debug.Log($"OnClick(): {boardPosition}");
        _board.TrySpawnRandomCell(boardPosition);
    }

    private void OnSwipe(Vector2Int boardPositionA, Vector2Int boardPositionB)
    {
        Debug.Log($"OnSwipe(): {boardPositionA} -> {boardPositionB}");
        _board.TrySwapCells(boardPositionA, boardPositionB);
    }

    private void OnSpacebar()
    {
        Debug.Log($"OnSpacebar()");
        _board.DestroyAllCells();
    }
}

using UnityEngine;
using Kott.MagicDefence.Hex;
using UnityEngine.EventSystems;

namespace Kott.MagicDefence.LevelEditor
{
    public class BoardInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
    {
        private bool _isPressed;
        private bool _hasSwiped;

        private HexVector _pressStartBoardPosition;

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
            var cellBackground = eventData.pointerCurrentRaycast.gameObject.GetComponent<BoardCell>();
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

            var cellBackground = eventData.pointerCurrentRaycast.gameObject?.GetComponent<BoardCell>();
            if (cellBackground == null)
                return;

            var currentBoardPosition = cellBackground.BoardPosition;
            if (currentBoardPosition != _pressStartBoardPosition)
            {
                var distance = (_pressStartBoardPosition - currentBoardPosition).Length;
                if (distance > 1.0f)
                    return;

                _hasSwiped = true;
                OnSwipe(_pressStartBoardPosition, currentBoardPosition);
            }
        }


        private void OnClick(HexVector boardPosition)
        {
            Debug.Log($"OnClick(): {boardPosition}");
            _board.ToggleCellState(boardPosition);
        }

        private void OnSwipe(HexVector boardPositionA, HexVector boardPositionB)
        {
            Debug.Log($"OnSwipe(): {boardPositionA} -> {boardPositionB}");
        }

        private void OnSpacebar()
        {
            Debug.Log($"OnSpacebar()");
        }
    }
}

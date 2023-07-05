using UnityEngine;
using Kott.MagicDefence.Hex;
using System.Collections.Generic;

namespace Kott.MagicDefence.LevelEditor
{
    public class Board : MonoBehaviour
    {
        private class BoardCellContainer
        {
            public BoardCell Cell;
            public bool IsActive;
        }


        [SerializeField] private int _maxBoardRadius = 3;

        [SerializeField] private float _cellRadius = 0.5f;
        [SerializeField] private BoardCell _cellPrefab;


        private Vector2 _halfCellSize;

        private Dictionary<HexVector, BoardCellContainer> _cells;


        private void Awake()
        {
            _halfCellSize = new Vector2(_cellRadius, _cellRadius * Mathf.Sqrt(3.0f) / 2.0f);
            _cells = new Dictionary<HexVector, BoardCellContainer>();

            foreach (var position in HexVector.Area(_maxBoardRadius))
            {
                var cell = Instantiate(_cellPrefab, BoardToWorldPosition(position), Quaternion.identity, transform);
                cell.SetPosition(position);
                cell.SetState(BoardCell.CellState.Disabled);

                var container = new BoardCellContainer()
                {
                    Cell = cell,
                    IsActive = Random.value > 0.5f
                };

                _cells.Add(position, container);
            }

            UpdateCellStates();
        }


        private Vector3 BoardToWorldPosition(HexVector position)
        {
            var worldPosition = Vector3.zero;

            worldPosition += position.q * new Vector3(_halfCellSize.x * 1.5f, -_halfCellSize.y, 0.0f);
            worldPosition += position.r * new Vector3(0.0f, -_halfCellSize.y * 2.0f, 0.0f);

            return worldPosition;
        }


        private void UpdateCellStates()
        {
            foreach (var position in HexVector.Area(_maxBoardRadius))
            {
                UpdateCellState(position);
            }
        }

        private void UpdateCellState(HexVector position)
        {
            if (!_cells.TryGetValue(position, out var container) || container == null)
                return;

            container.Cell.SetState(GetCellState(position));
            container.Cell.SetNeighbourStates(
                GetCellState(position + HexVector.Down),
                GetCellState(position + HexVector.DownRight),
                GetCellState(position + HexVector.DownLeft),
                GetCellState(position + HexVector.Up),
                GetCellState(position + HexVector.UpRight),
                GetCellState(position + HexVector.UpLeft));
        }


        BoardCell.CellState GetCellState(HexVector position)
        {
            if (!_cells.TryGetValue(position, out var container) || container == null)
                return BoardCell.CellState.None;

            return container.IsActive ? BoardCell.CellState.Enabled : BoardCell.CellState.Disabled;
        }


        public void ToggleCellState(HexVector position)
        {
            if (!_cells.TryGetValue(position, out var container) || container == null)
                return;

            container.IsActive = !container.IsActive;

            foreach (var offset in HexVector.Area(1))
            {
                UpdateCellState(position + offset);
            }
        }
    }
}

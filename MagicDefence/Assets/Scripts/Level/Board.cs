using Kott.MagicDefence.Hex;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kott.MagicDefence.Level
{
    public class Board<TStone, TLayer> where TStone : IStone where TLayer : ILayer
    {
        private class Cell
        {
            public TStone Stone;
            public TLayer[] Layers;
        }


        private Dictionary<HexVector, Cell> _cells;


        public Board(int capacity = 100)
        {
            _cells = new Dictionary<HexVector, Cell>(capacity);
        }


        /// <summary>
        /// Adds an empty cell at <paramref name="position"/> to the board, if it doesn't already exists.
        /// </summary>
        /// <param name="position">The position to add the cell at.</param>
        /// <returns>If an empty cell was added.</returns>
        public bool AddCell(HexVector position)
        {
            if (_cells.ContainsKey(position))
                return false;

            _cells.Add(position, new Cell
            {
                Stone = default,
                Layers = new TLayer[(int)LayerType.MAX],
            });
            return true;
        }

        /// <summary>
        /// Checks, whether or not <paramref name="position"/> exists on the board.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <returns>Does <paramref name="position"/> exist on the board.</returns>
        public bool HasCell(HexVector position)
        {
            return _cells.ContainsKey(position);
        }

        /// <summary>
        /// Removes the cell at <paramref name="position"/>, if it exists.
        /// </summary>
        /// <param name="position">The position to remove the cell from.</param>
        /// <returns>If a cell got removed.</returns>
        public bool RemoveCell(HexVector position)
        {
            return _cells.Remove(position);
        }


        /// <summary>
        /// Gets the stone object at <paramref name="position"/>, if the position exists on the board.
        /// </summary>
        /// <param name="position">The position to get the stone from.</param>
        /// <param name="stone">The stone object.</param>
        /// <returns>Does the specified position exist on the board.</returns>
        public bool TryGetStone(HexVector position, out TStone stone)
        {
            if (!_cells.TryGetValue(position, out var cell))
            {
                stone = default;
                return false;
            }

            stone = cell.Stone;
            return true;
        }

        /// <summary>
        /// Swaps the stone objects at <paramref name="positionA"/> and <paramref name="positionB"/>, if both positions exists on the board.
        /// </summary>
        /// <param name="positionA">The position of the first stone.</param>
        /// <param name="positionB">The position of the second stone.</param>
        /// <returns>Do the specified positions exist on the board.</returns>
        public bool TrySwapStones(HexVector positionA, HexVector positionB)
        {
            if (!_cells.TryGetValue(positionA, out var cellA))
                return false;

            if (!_cells.TryGetValue(positionB, out var cellB))
                return false;

            var tmp = cellA.Stone;
            cellA.Stone = cellB.Stone;
            cellB.Stone = tmp;
            return true;
        }

        public bool TrySetStone(HexVector position, TStone stone)
        {
            if (!_cells.TryGetValue(position, out var cell))
                return false;

            cell.Stone = stone;
            return true;
        }

        public bool TrySetStone(HexVector position, TStone stone, out TStone replacedStone)
        {
            if (!_cells.TryGetValue(position, out var cell))
            {
                replacedStone = default;
                return false;
            }

            replacedStone = cell.Stone;
            cell.Stone = stone;
            return true;
        }


        public bool TryGetLayer(HexVector position, LayerType layerType, out TLayer layer)
        {
            if (!_cells.TryGetValue(position, out var cell))
            {
                layer = default;
                return false;
            }

            layer = cell.Layers[(int)layerType];
            return true;
        }

        public bool TryGetLayers(HexVector position, out IEnumerable layers)
        {
            if (!_cells.TryGetValue(position, out var cell))
            {
                layers = default;
                return false;
            }

            layers = cell.Layers;
            return true;
        }

        public bool TrySetStone(HexVector position, LayerType layerType, TLayer layer)
        {
            if (!_cells.TryGetValue(position, out var cell))
                return false;

            cell.Layers[(int)layerType] = layer;
            return true;
        }

        public bool TrySetStone(HexVector position, LayerType layerType, TLayer layer, out TLayer replacedLayer)
        {
            if (!_cells.TryGetValue(position, out var cell))
            {
                replacedLayer = default;
                return false;
            }

            replacedLayer = cell.Layers[(int)layerType];
            cell.Layers[(int)layerType] = layer;
            return true;
        }
    }
}

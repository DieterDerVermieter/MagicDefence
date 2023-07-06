using UnityEngine;
using Kott.MagicDefence.Hex;
using Kott.MagicDefence.Level;

namespace Kott.MagicDefence.Game
{
    public class BoardCell : MonoBehaviour, IStone, ILayer
    {
        public HexVector BoardPosition;

        public int CellColor = 0;

        public bool CanCombine = true;
        public bool CanMove = true;

        public LayerType LayerType => LayerType.LayerA;
    }
}

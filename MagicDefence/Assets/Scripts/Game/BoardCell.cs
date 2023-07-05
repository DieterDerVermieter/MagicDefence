using UnityEngine;
using Kott.MagicDefence.Hex;

namespace Kott.MagicDefence.Game
{
    public class BoardCell : MonoBehaviour
    {
        public HexVector BoardPosition;

        public int CellColor = 0;

        public bool CanCombine = true;
        public bool CanMove = true;
    }
}

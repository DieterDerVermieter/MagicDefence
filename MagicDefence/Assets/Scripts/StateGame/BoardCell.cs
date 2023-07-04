using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardCell : MonoBehaviour
{
    public HexVector BoardPosition;

    public int CellColor = 0;

    public bool CanCombine = true;
    public bool CanMove = true;
}

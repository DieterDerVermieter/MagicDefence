using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardCell : MonoBehaviour
{
    public HexVector GridPosition { get; private set; }


    public void Setup(HexVector gridPosition)
    {
        GridPosition = gridPosition;
    }
}

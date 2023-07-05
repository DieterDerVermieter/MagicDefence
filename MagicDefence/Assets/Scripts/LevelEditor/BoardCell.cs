using Kott.MagicDefence.Hex;
using UnityEngine;

namespace Kott.MagicDefence.LevelEditor
{
    public class BoardCell : MonoBehaviour
    {
        public enum CellState
        {
            None,
            Disabled,
            Enabled
        }


        [SerializeField] private GameObject _disabledContainer;
        [SerializeField] private GameObject _enabledContainer;

        [Header("Disabled")]
        [SerializeField] private GameObject _disabledBorderDown;
        [SerializeField] private GameObject _disabledBorderDownRight;
        [SerializeField] private GameObject _disabledBorderDownLeft;
        [SerializeField] private GameObject _disabledBorderUp;
        [SerializeField] private GameObject _disabledBorderUpRight;
        [SerializeField] private GameObject _disabledBorderUpLeft;

        [Header("Enabled")]
        [SerializeField] private GameObject _enabledBorderDown;
        [SerializeField] private GameObject _enabledBorderDownRight;
        [SerializeField] private GameObject _enabledBorderDownLeft;
        [SerializeField] private GameObject _enabledBorderUp;
        [SerializeField] private GameObject _enabledBorderUpRight;
        [SerializeField] private GameObject _enabledBorderUpLeft;


        public HexVector BoardPosition { get; private set; }

        public CellState State { get; private set; }


        public void SetPosition(HexVector position)
        {
            BoardPosition = position;
        }


        public void SetState(CellState state)
        {
            State = state;

            _disabledContainer.SetActive(state == CellState.Disabled);
            _enabledContainer.SetActive(state == CellState.Enabled);
        }

        public void SetNeighbourStates(CellState down, CellState downRight, CellState downLeft, CellState up, CellState upRight, CellState upLeft)
        {
            _disabledBorderDown.SetActive(State != down);
            _disabledBorderDownRight.SetActive(State != downRight);
            _disabledBorderDownLeft.SetActive(State != downLeft);
            _disabledBorderUp.SetActive(State != up);
            _disabledBorderUpRight.SetActive(State != upRight);
            _disabledBorderUpLeft.SetActive(State != upLeft);

            _enabledBorderDown.SetActive(State != down);
            _enabledBorderDownRight.SetActive(State != downRight);
            _enabledBorderDownLeft.SetActive(State != downLeft);
            _enabledBorderUp.SetActive(State != up);
            _enabledBorderUpRight.SetActive(State != upRight);
            _enabledBorderUpLeft.SetActive(State != upLeft);
        }
    }
}

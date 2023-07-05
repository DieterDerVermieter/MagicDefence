using UnityEngine;
using Kott.MagicDefence.Hex;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kott.MagicDefence.Game
{
    public class BoardCellBackground : MonoBehaviour
    {
        [SerializeField] private GameObject _borderDown;
        [SerializeField] private GameObject _borderDownRight;
        [SerializeField] private GameObject _borderDownLeft;
        [SerializeField] private GameObject _borderUp;
        [SerializeField] private GameObject _borderUpRight;
        [SerializeField] private GameObject _borderUpLeft;


        public HexVector BoardPosition;


        public void SetupNeighbours(bool down, bool downRight, bool downLeft, bool up, bool upRight, bool upLeft)
        {
            _borderDown.SetActive(!down);
            _borderDownRight.SetActive(!downRight);
            _borderDownLeft.SetActive(!downLeft);
            _borderUp.SetActive(!up);
            _borderUpRight.SetActive(!upRight);
            _borderUpLeft.SetActive(!upLeft);
        }


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Handles.Label(transform.position, BoardPosition.ToString());
        }
#endif
    }
}

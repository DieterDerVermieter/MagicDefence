using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class Board : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
    [SerializeField] private int _boardRadius = 4;
    [SerializeField] private float _cellRadius = 1.0f;

    [SerializeField] private BoardCell _cellPrefab;

    [SerializeField] private GameObject _itemPrefab;


    private bool _isPressed;
    private bool _hasSwiped;
    private HexVector _pressStartGridPosition;
    private HashSet<HexVector> _pressStartGridNeighbours;

    private Queue<IEnumerator> _visualActionQueue = new Queue<IEnumerator>();
    private IEnumerator _currentVisualAction;


    public readonly HexGrid<GameObject> Grid = new HexGrid<GameObject>();

    public Vector2 HalfCellSize { get; private set; }


    private void Awake()
    {
        HalfCellSize = new Vector2(_cellRadius, Mathf.Sqrt(3.0f) * 0.5f * _cellRadius);
    }

    private void Start()
    {
        CreateBoard();
    }


    private void CreateBoard()
    {
        foreach (var position in HexVector.Spiral(_boardRadius))
        {
            var cell = Instantiate(_cellPrefab, CellPosition(position), Quaternion.identity, transform);
            cell.Setup(position);

            Grid.Add(position, null);
        }
    }


    public Vector3 CellPosition(HexVector hexPosition)
    {
        var worldPosition = Vector3.zero;

        worldPosition += hexPosition.x * new Vector3(HalfCellSize.x * 1.5f, -HalfCellSize.y, 0.0f);
        worldPosition += hexPosition.y * new Vector3(0.0f, -HalfCellSize.y * 2.0f, 0.0f);

        return worldPosition;
    }


    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        var currentCell = eventData.pointerCurrentRaycast.gameObject?.GetComponent<BoardCell>();
        if (currentCell == null)
            return;

        _isPressed = true;

        _pressStartGridPosition = currentCell.GridPosition;
        _pressStartGridNeighbours = HexVector.Ring(1)
            .Select(ringPos => ringPos + _pressStartGridPosition)
            .Where(gridPos => Grid.ContainsKey(gridPos))
            .ToHashSet();
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        if (!_hasSwiped)
        {
            var currentCell = eventData.pointerCurrentRaycast.gameObject?.GetComponent<BoardCell>();
            if (currentCell != null && currentCell.GridPosition == _pressStartGridPosition)
            {
                HandleClick(_pressStartGridPosition);
            }
        }

        _isPressed = false;
        _hasSwiped = false;
    }

    void IPointerMoveHandler.OnPointerMove(PointerEventData eventData)
    {
        if (!_isPressed || _hasSwiped)
            return;

        var currentCell = eventData.pointerCurrentRaycast.gameObject?.GetComponent<BoardCell>();
        if (currentCell == null)
            return;

        if (!_pressStartGridNeighbours.Contains(currentCell.GridPosition))
            return;

        _hasSwiped = true;
        HandleSwipe(_pressStartGridPosition, currentCell.GridPosition);
    }


    private void HandleClick(HexVector position)
    {
        Debug.Log($"Clicked at {position}.");
        SpawnItem(_itemPrefab, position);
    }

    private void HandleSwipe(HexVector originPosition, HexVector targetPosition)
    {
        Debug.Log($"Swiped from {originPosition} to {targetPosition}.");
        SwapItem(originPosition, targetPosition);
    }


    public GameObject SpawnItem(GameObject itemPrefab, HexVector position)
    {
        if (!Grid.TryGetValue(position, out var otherItem) || otherItem != null)
            return null;

        var item = Instantiate(itemPrefab, CellPosition(position), Quaternion.identity);
        if(item.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
            spriteRenderer.color = Random.ColorHSV(0.0f, 1.0f, 0.8f, 1.0f, 0.5f, 0.8f);

        Grid[position] = item;
        return item;
    }

    public void SwapItem(HexVector originPosition, HexVector targetPosition)
    {
        if (!Grid.TryGetValue(originPosition, out var originItem) || originPosition == null)
            return;

        if (!Grid.TryGetValue(targetPosition, out var targetItem) || targetItem == null)
            return;

        Grid[originPosition] = targetItem;
        Grid[targetPosition] = originItem;

        AddAction(SwapAction(originItem, targetItem, CellPosition(originPosition), CellPosition(targetPosition)));
    }


    private IEnumerator SwapAction(GameObject originItem, GameObject targetItem, Vector3 originPosition, Vector3 targetPosition)
    {
        var progress = 0.0f;
        while(progress < 1.0f)
        {
            progress += Time.deltaTime * 2.0f;
            var lerp = progress;
            lerp = 1 - lerp;
            lerp = lerp * lerp;
            lerp = 1 - lerp;

            originItem.transform.position = Vector3.Lerp(originPosition, targetPosition, lerp);
            targetItem.transform.position = Vector3.Lerp(targetPosition, originPosition, lerp);

            yield return null;
        }

        originItem.transform.position = targetPosition;
        targetItem.transform.position = originPosition;
    }


    private void AddAction(IEnumerator visualAction)
    {
        _visualActionQueue.Enqueue(visualAction);
    }


    private void Update()
    {
        if (_currentVisualAction != null)
        {
            if (!_currentVisualAction.MoveNext())
                _currentVisualAction = null;
        }
        else if(_visualActionQueue.Count > 0)
        {
            _currentVisualAction = _visualActionQueue.Dequeue();
        }
    }
}

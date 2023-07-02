using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class Board : MonoBehaviour
{
    [SerializeField] private Vector2Int _boardSize = new Vector2Int(5, 5);

    [SerializeField] private float _cellRadius = 1.0f;
    [SerializeField] private BoardCellBackground _cellBackgroundPrefab;

    [SerializeField] private List<BoardCell> _testCellPrefabs;

    [SerializeField] private TMP_Text _scoreText;


    private Dictionary<Vector2Int, BoardCell> _cells = new Dictionary<Vector2Int, BoardCell>();
    private Dictionary<Vector2Int, BoardCell> _cellsTmp = new Dictionary<Vector2Int, BoardCell>();

    private bool _isTweenRunning;
    private Queue<Tween> _tweenQueue = new Queue<Tween>();

    private bool _isSimulating;
    private delegate void SimulationStep(ref bool hasChanged, Sequence sequence);

    private int _score;


    public bool IsBusy => _isTweenRunning || _isSimulating;


    private void Start()
    {
        for (int i = 0; i < _boardSize.x; i++)
        {
            for (int j = 0; j < _boardSize.y; j++)
            {
                var boardPosition = new Vector2Int(i, j) - _boardSize / 2;
                _cells.Add(boardPosition, null);
                _cellsTmp.Add(boardPosition, null);

                var cellBackground = Instantiate(_cellBackgroundPrefab, BoardToWorldPosition(boardPosition), Quaternion.identity, transform);
                cellBackground.BoardPosition = boardPosition;
            }
        }

        SimulateBoard();
    }


    private void EnqueueTween(Tween tween)
    {
        tween.Pause();
        _tweenQueue.Enqueue(tween);
        PlayNextTween();

        void PlayNextTween()
        {
            if (_isTweenRunning)
                return;

            if (_tweenQueue.Count <= 0)
                return;

            var tween = _tweenQueue.Dequeue();

            tween.onComplete += OnTweenComplete;
            tween.Play();

            _isTweenRunning = true;
        }

        void OnTweenComplete()
        {
            _isTweenRunning = false;
            PlayNextTween();
        }
    }


    public Vector3 CellPosition(HexVector hexPosition)
    {
        var worldPosition = Vector3.zero;

        // worldPosition += hexPosition.x * new Vector3(HalfCellSize.x * 1.5f, -HalfCellSize.y, 0.0f);
        // worldPosition += hexPosition.y * new Vector3(0.0f, -HalfCellSize.y * 2.0f, 0.0f);

        return worldPosition;
    }


    public Vector3 BoardToWorldPosition(Vector2Int boardPosition)
    {
        return new Vector3(boardPosition.x, boardPosition.y, 0.0f);
    }


    public bool TryGetCell(Vector2Int boardPosition, out BoardCell cell)
    {
        return _cells.TryGetValue(boardPosition, out cell);
    }


    public bool TrySpawnRandomCell(Vector2Int boardPosition)
    {
        if (IsBusy)
            return false;

        var sequence = DOTween.Sequence();
        var result = TrySpawnRandomCell(boardPosition, sequence);
        EnqueueTween(sequence);
        SimulateBoard();
        return result;
    }

    private bool TrySpawnRandomCell(Vector2Int boardPosition, Sequence sequence)
    {
        if (_testCellPrefabs.Count <= 0)
            return false;

        var randomPrefab = _testCellPrefabs[Random.Range(0, _testCellPrefabs.Count)];
        return TrySpawnCell(boardPosition, randomPrefab, sequence);
    }


    public bool TrySpawnCell(Vector2Int boardPosition, BoardCell cellPrefab)
    {
        if (IsBusy)
            return false;

        var sequence = DOTween.Sequence();
        var result = TrySpawnCell(boardPosition, cellPrefab, sequence);
        EnqueueTween(sequence);
        SimulateBoard();
        return result;
    }

    private bool TrySpawnCell(Vector2Int boardPosition, BoardCell cellPrefab, Sequence sequence)
    {
        if (!_cells.TryGetValue(boardPosition, out var otherCell) || otherCell != null)
            return false;

        var newCell = Instantiate(cellPrefab, BoardToWorldPosition(boardPosition), Quaternion.identity, transform);
        newCell.BoardPosition = boardPosition;
        _cells[boardPosition] = newCell;

        newCell.transform.localScale = Vector3.zero;

        var tween = newCell.transform.DOScale(1.0f, 0.2f).SetEase(Ease.OutQuad);
        sequence.Join(tween);

        return true;
    }


    public bool DestroyAllCells()
    {
        if (IsBusy)
            return false;

        var sequence = DOTween.Sequence();

        for (int i = 0; i < _boardSize.x; i++)
        {
            for (int j = 0; j < _boardSize.y; j++)
            {
                var boardPosition = new Vector2Int(i, j) - _boardSize / 2;
                TryDestroyCell(boardPosition, sequence);
            }
        }

        EnqueueTween(sequence);
        SimulateBoard();

        return true;
    }

    public bool TryDestroyCell(Vector2Int boardPosition)
    {
        if (IsBusy)
            return false;

        var sequence = DOTween.Sequence();
        var result = TryDestroyCell(boardPosition, sequence);
        EnqueueTween(sequence);
        SimulateBoard();
        return result;
    }

    private bool TryDestroyCell(Vector2Int boardPosition, Sequence sequence)
    {
        if (!_cells.TryGetValue(boardPosition, out var otherCell) || otherCell == null)
            return false;

        var tween = otherCell.transform.DOScale(0.0f, 0.2f).SetEase(Ease.InQuad);
        tween.onComplete += () => Destroy(otherCell.gameObject);
        sequence.Join(tween);

        _cells[boardPosition] = null;

        ChangeScore(100, sequence);

        return true;
    }


    public bool TrySwapCells(Vector2Int boardPositionA, Vector2Int boardPositionB)
    {
        if (IsBusy)
            return false;

        var sequence = DOTween.Sequence();
        var result = TrySwapCells(boardPositionA, boardPositionB, sequence);
        EnqueueTween(sequence);

        if (result && !SimulateBoard())
        {
            sequence = DOTween.Sequence();
            TrySwapCells(boardPositionA, boardPositionB, sequence);
            EnqueueTween(sequence);
        }

        return result;
    }

    private bool TrySwapCells(Vector2Int boardPositionA, Vector2Int boardPositionB, Sequence sequence)
    {
        if (!_cells.TryGetValue(boardPositionA, out var cellA))
            return false;

        if (!_cells.TryGetValue(boardPositionB, out var cellB))
            return false;

        if(cellA != null)
        {
            cellA.BoardPosition = boardPositionB;
            sequence.Join(cellA.transform.DOMove(BoardToWorldPosition(boardPositionB), 0.2f).SetEase(Ease.OutQuad));
        }

        if(cellB != null)
        {
            cellB.BoardPosition = boardPositionA;
            sequence.Join(cellB.transform.DOMove(BoardToWorldPosition(boardPositionA), 0.2f).SetEase(Ease.OutQuad));
        }

        _cells[boardPositionA] = cellB;
        _cells[boardPositionB] = cellA;

        return true;
    }


    private void ChangeScore(int scoreChange, Sequence sequence)
    {
        SetScore(_score + scoreChange, sequence);
    }

    private void SetScore(int newScore, Sequence sequence)
    {
        var oldScore = _score;
        _score = newScore;
        sequence.Join(DOTween.To(() => oldScore, (value) => _scoreText.text = $"{value:0,0}", newScore, 0.5f).SetEase(Ease.OutQuad));
    }


    public bool SimulateBoard()
    {
        _isSimulating = true;
        var sequence = DOTween.Sequence();
        if (!StepBoard(sequence))
        {
            _isSimulating = false;
            return false;
        }

        sequence.onComplete += () => SimulateBoard();
        EnqueueTween(sequence);
        return true;
    }

    public bool StepBoard(Sequence sequence)
    {
        var hasChanged = false;

        StepGravity(ref hasChanged, sequence);
        StepRefill(ref hasChanged, sequence);
        StepCombination(ref hasChanged, sequence);

        return hasChanged;
    }


    private void StepGravity(ref bool hasChanged, Sequence sequence)
    {
        if (hasChanged)
            return;

        var time = 0.0f;
        do {
            hasChanged = false;

            var subSequence = DOTween.Sequence();
            for (int i = 0; i < _boardSize.x; i++)
            {
                var currentPosition = new Vector2Int(i, _boardSize.y - 1) - _boardSize / 2;
                if (!_cells.TryGetValue(currentPosition, out var current) || current != null)
                    continue;

                if (!TrySpawnRandomCell(currentPosition, subSequence))
                    continue;

                hasChanged = true;
            }

            sequence.Insert(time, subSequence);

            for (int i = 0; i < _boardSize.x; i++)
            {
                for (int j = 0; j < _boardSize.y; j++)
                {
                    var currentPosition = new Vector2Int(i, j) - _boardSize / 2;
                    if (!_cells.TryGetValue(currentPosition, out var current) || current == null)
                        continue;

                    if (TryFall(current, currentPosition + Vector2Int.down))
                    {
                        hasChanged = true;
                        continue;
                    }

                    var randomDirection = Random.value > 0.5f ? 1 : -1;

                    if (TryFall(current, currentPosition + Vector2Int.down + Vector2Int.right * randomDirection))
                    {
                        hasChanged = true;
                        continue;
                    }

                    if (TryFall(current, currentPosition + Vector2Int.down + Vector2Int.left * randomDirection))
                    {
                        hasChanged = true;
                        continue;
                    }
                }
            }

            time += 0.1f;
        }
        while (hasChanged);

        bool TryFall(BoardCell cell, Vector2Int position)
        {
            if (!_cells.TryGetValue(position, out var other) || other != null)
                return false;

            var oldPosition = cell.BoardPosition;
            cell.BoardPosition = position;

            sequence.Insert(time, cell.transform.DOMove(BoardToWorldPosition(position), 0.2f).SetEase(Ease.OutQuad));

            _cells[oldPosition] = null;
            _cells[position] = cell;

            return true;
        }
    }

    private void StepRefill(ref bool hasChanged, Sequence sequence)
    {
        if (hasChanged)
            return;

        var subSequence = DOTween.Sequence();

        for (int i = 0; i < _boardSize.x; i++)
        {
            var currentPosition = new Vector2Int(i, _boardSize.y - 1) - _boardSize / 2;
            if (!_cells.TryGetValue(currentPosition, out var current) || current != null)
                continue;

            if (!TrySpawnRandomCell(currentPosition, subSequence))
                continue;

            hasChanged = true;
        }

        sequence.Append(subSequence);
    }

    private void StepCombination(ref bool hasChanged, Sequence sequence)
    {
        if (hasChanged)
            return;

        for (int i = 0; i < _boardSize.x; i++)
        {
            for (int j = 0; j < _boardSize.y; j++)
            {
                var currentPosition = new Vector2Int(i, j) - _boardSize / 2;
                if (!_cells.ContainsKey(currentPosition) || !_cellsTmp.ContainsKey(currentPosition))
                    continue;

                _cellsTmp[currentPosition] = _cells[currentPosition];
            }
        }

        for (int i = 0; i < _boardSize.x; i++)
        {
            for (int j = 0; j < _boardSize.y; j++)
            {
                var currentPosition = new Vector2Int(i, j) - _boardSize / 2;

                if(CheckDirection(currentPosition, Vector2Int.up))
                {
                    hasChanged = true;
                    continue;
                }

                if (CheckDirection(currentPosition, Vector2Int.right))
                {
                    hasChanged = true;
                    continue;
                }
            }
        }

        // sequence.Append(subSequence);

        bool CheckDirection(Vector2Int currentPosition, Vector2Int direction)
        {
            if (!_cellsTmp.TryGetValue(currentPosition, out var current) || current == null)
                return false;

            var otherPositionA = currentPosition + direction;
            if (!_cellsTmp.TryGetValue(otherPositionA, out var otherA) || otherA == null)
                return false;

            var otherPositionB = currentPosition - direction;
            if (!_cellsTmp.TryGetValue(otherPositionB, out var otherB) || otherB == null)
                return false;

            if (current.CellColor != otherA.CellColor || current.CellColor != otherB.CellColor)
                return false;

            TryDestroyCell(currentPosition, sequence);
            TryDestroyCell(otherPositionA, sequence);
            TryDestroyCell(otherPositionB, sequence);
            return true;
        }
    }
}

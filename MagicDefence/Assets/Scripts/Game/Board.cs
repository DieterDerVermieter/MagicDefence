using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Kott.MagicDefence.Hex;

namespace Kott.MagicDefence.Game
{
    public class Board : MonoBehaviour
    {
        [SerializeField] private int _boardRadius = 2;

        [SerializeField] private float _cellRadius = 1.0f;
        [SerializeField] private BoardCellBackground _cellBackgroundPrefab;

        [SerializeField] private List<BoardCell> _testStonePrefabs;
        [SerializeField] private List<BoardCell> _testBlockerPrefabs;

        [SerializeField] private TMP_Text _scoreText;


        private IEnumerable<HexVector> _cellPositions;
        private IEnumerable<HexVector> _cellTopPositions;

        private Dictionary<HexVector, BoardCell> _cells = new Dictionary<HexVector, BoardCell>();
        private Dictionary<HexVector, BoardCell> _cellsTmp = new Dictionary<HexVector, BoardCell>();

        private bool _isTweenRunning;
        private Queue<Tween> _tweenQueue = new Queue<Tween>();

        private bool _isSimulating;
        private delegate void SimulationStep(ref bool hasChanged, Sequence sequence);

        private int _score;


        public Vector2 HalfCellSize { get; private set; }

        public bool IsBusy => _isTweenRunning || _isSimulating;


        private void Awake()
        {
            HalfCellSize = new Vector2(_cellRadius, _cellRadius * Mathf.Sqrt(3.0f) / 2.0f);
        }

        private void Start()
        {
            CreateBoard();
            SimulateBoard();
        }


        private void CreateBoard()
        {
            _cellPositions = HexVector.Area(_boardRadius).Where(position => position.Length > _boardRadius - 2).ToList();
            _cellTopPositions = _cellPositions.Where(position => position.r == -_boardRadius || position.s == _boardRadius).ToList();

            var cellBackgrounds = new Dictionary<HexVector, BoardCellBackground>();

            foreach (var boardPosition in _cellPositions)
            {
                _cells.Add(boardPosition, null);
                _cellsTmp.Add(boardPosition, null);

                var cellBackground = Instantiate(_cellBackgroundPrefab, BoardToWorldPosition(boardPosition), Quaternion.identity, transform);
                cellBackground.BoardPosition = boardPosition;

                cellBackgrounds.Add(boardPosition, cellBackground);
            }

            foreach (var boardPosition in _cellPositions)
            {
                if (!cellBackgrounds.TryGetValue(boardPosition, out var cellBackground) || cellBackground == null)
                    continue;

                cellBackground.SetupNeighbours(
                    _cells.ContainsKey(boardPosition + HexVector.Down),
                    _cells.ContainsKey(boardPosition + HexVector.DownRight),
                    _cells.ContainsKey(boardPosition + HexVector.DownLeft),
                    _cells.ContainsKey(boardPosition + HexVector.Up),
                    _cells.ContainsKey(boardPosition + HexVector.UpRight),
                    _cells.ContainsKey(boardPosition + HexVector.UpLeft));
            }

            var seqeunce = DOTween.Sequence();
            foreach (var boardPosition in _cellPositions)
            {
                var isBlocker = Random.value > 0.9f;
                var prefabList = isBlocker ? _testBlockerPrefabs : _testStonePrefabs;

                TrySpawnCell(boardPosition, prefabList[Random.Range(0, prefabList.Count)], seqeunce);
            }

            EnqueueTween(seqeunce);
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


        public Vector3 BoardToWorldPosition(HexVector boardPosition)
        {
            var worldPosition = Vector3.zero;

            worldPosition += boardPosition.q * new Vector3(HalfCellSize.x * 1.5f, -HalfCellSize.y, 0.0f);
            worldPosition += boardPosition.r * new Vector3(0.0f, -HalfCellSize.y * 2.0f, 0.0f);

            return worldPosition;
        }


        public bool TryGetCell(HexVector boardPosition, out BoardCell cell)
        {
            return _cells.TryGetValue(boardPosition, out cell);
        }


        public bool TrySpawnRandomCell(HexVector boardPosition)
        {
            if (IsBusy)
                return false;

            var sequence = DOTween.Sequence();
            var result = TrySpawnRandomCell(boardPosition, sequence);
            EnqueueTween(sequence);
            SimulateBoard();
            return result;
        }

        private bool TrySpawnRandomCell(HexVector boardPosition, Sequence sequence)
        {
            if (_testStonePrefabs.Count <= 0)
                return false;

            var randomPrefab = _testStonePrefabs[Random.Range(0, _testStonePrefabs.Count)];
            return TrySpawnCell(boardPosition, randomPrefab, sequence);
        }


        public bool TrySpawnCell(HexVector boardPosition, BoardCell cellPrefab)
        {
            if (IsBusy)
                return false;

            var sequence = DOTween.Sequence();
            var result = TrySpawnCell(boardPosition, cellPrefab, sequence);
            EnqueueTween(sequence);
            SimulateBoard();
            return result;
        }

        private bool TrySpawnCell(HexVector boardPosition, BoardCell cellPrefab, Sequence sequence)
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

            foreach (var boardPosition in _cellPositions)
            {
                TryDestroyCell(boardPosition, sequence);
            }

            EnqueueTween(sequence);
            SimulateBoard();

            return true;
        }

        public bool TryDestroyCell(HexVector boardPosition)
        {
            if (IsBusy)
                return false;

            var sequence = DOTween.Sequence();
            var result = TryDestroyCell(boardPosition, sequence);
            EnqueueTween(sequence);
            SimulateBoard();
            return result;
        }

        private bool TryDestroyCell(HexVector boardPosition, Sequence sequence)
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


        public bool TrySwapCells(HexVector boardPositionA, HexVector boardPositionB)
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

        private bool TrySwapCells(HexVector boardPositionA, HexVector boardPositionB, Sequence sequence)
        {
            if (!_cells.TryGetValue(boardPositionA, out var cellA) || cellA == null || !cellA.CanMove)
                return false;

            if (!_cells.TryGetValue(boardPositionB, out var cellB) || cellB == null || !cellB.CanMove)
                return false;

            cellA.BoardPosition = boardPositionB;
            sequence.Join(cellA.transform.DOMove(BoardToWorldPosition(boardPositionB), 0.2f).SetEase(Ease.OutQuad));

            cellB.BoardPosition = boardPositionA;
            sequence.Join(cellB.transform.DOMove(BoardToWorldPosition(boardPositionA), 0.2f).SetEase(Ease.OutQuad));

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

            StepMovementAndSpawning(ref hasChanged, sequence);
            StepCombination(ref hasChanged, sequence);

            return hasChanged;
        }


        private void StepMovementAndSpawning(ref bool hasChanged, Sequence sequence)
        {
            if (hasChanged)
                return;

            var time = 0.0f;
            do
            {
                hasChanged = false;

                var subSequence = DOTween.Sequence();
                foreach (var boardPosition in _cellTopPositions)
                {
                    if (!_cells.TryGetValue(boardPosition, out var current) || current != null)
                        continue;

                    if (!TrySpawnRandomCell(boardPosition, subSequence))
                        continue;

                    hasChanged = true;
                }

                sequence.Insert(time, subSequence);

                foreach (var boardPosition in _cellPositions)
                {
                    if (!_cells.TryGetValue(boardPosition, out var current) || current == null || !current.CanMove)
                        continue;

                    if (TryFall(current, boardPosition + HexVector.Down))
                    {
                        hasChanged = true;
                        continue;
                    }
                }

                if (hasChanged)
                    continue;

                foreach (var boardPosition in _cellPositions)
                {
                    if (!_cells.TryGetValue(boardPosition, out var current) || current == null || !current.CanMove)
                        continue;

                    var mirror = Random.value > 0.5f;

                    if (TryFall(current, boardPosition + (mirror ? HexVector.DownRight : HexVector.DownLeft)))
                    {
                        hasChanged = true;
                        continue;
                    }

                    if (TryFall(current, boardPosition + (mirror ? HexVector.DownLeft : HexVector.DownRight)))
                    {
                        hasChanged = true;
                        continue;
                    }
                }

                time += 0.1f;
            }
            while (hasChanged);

            bool TryFall(BoardCell cell, HexVector position)
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

        private void StepCombination(ref bool hasChanged, Sequence sequence)
        {
            if (hasChanged)
                return;

            foreach (var boardPosition in _cellPositions)
            {
                if (!_cells.ContainsKey(boardPosition) || !_cellsTmp.ContainsKey(boardPosition))
                    continue;

                _cellsTmp[boardPosition] = _cells[boardPosition];
            }

            foreach (var boardPosition in _cellPositions)
            {
                if (!_cellsTmp.TryGetValue(boardPosition, out var current) || current == null)
                    continue;

                if (!current.CanCombine)
                    continue;

                if (CheckDirection(current, boardPosition, HexVector.Up))
                {
                    hasChanged = true;
                    continue;
                }

                if (CheckDirection(current, boardPosition, HexVector.UpRight))
                {
                    hasChanged = true;
                    continue;
                }

                if (CheckDirection(current, boardPosition, HexVector.UpLeft))
                {
                    hasChanged = true;
                    continue;
                }
            }

            // sequence.Append(subSequence);

            bool CheckDirection(BoardCell current, HexVector position, HexVector direction)
            {
                var otherPositionA = position + direction;
                if (!_cellsTmp.TryGetValue(otherPositionA, out var otherA) || otherA == null)
                    return false;

                var otherPositionB = position - direction;
                if (!_cellsTmp.TryGetValue(otherPositionB, out var otherB) || otherB == null)
                    return false;

                if (current.CellColor != otherA.CellColor || current.CellColor != otherB.CellColor)
                    return false;

                TryDestroyCell(position, sequence);
                TryDestroyCell(otherPositionA, sequence);
                TryDestroyCell(otherPositionB, sequence);
                return true;
            }
        }
    }
}

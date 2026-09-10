using UnityEngine;

/// Fills LevelRoot once at startup from a fixed seed. Not streaming: the result
/// is ordinary static children, so checkpoints and scroll rewind behave exactly
/// as with a hand-authored level.
///
/// Rows are divided into horizontal slots and each object takes a whole slot,
/// so spawned objects can never overlap each other.
public class LevelPopulator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _levelRoot;

    [Header("Content")]
    [SerializeField] private GameObject[] _hazardPrefabs;
    [SerializeField] private GameObject[] _obstaclePrefabs;
    [SerializeField] private GameObject _pickupPrefab;
    [SerializeField] private GameObject _checkpointPrefab;
    [SerializeField] private GameObject _winTriggerPrefab;

    [Header("Shape")]
    [Tooltip("Same seed always produces the same level.")]
    [SerializeField] private int _seed = 12345;

    [Tooltip("Total climb height in world units.")]
    [SerializeField] private float _levelHeight = 1000f;

    [Tooltip("Distance above the player's start before anything spawns.")]
    [SerializeField] private float _startClearance = 20f;

    [Tooltip("Vertical gap between rows.")]
    [SerializeField] private float _rowSpacing = 12f;

    [Tooltip("Random vertical jitter per row, so rows do not look like a grid.")]
    [SerializeField] private float _rowJitter = 3f;

    [Header("Horizontal Slots")]
    [SerializeField] private float _minX = -7f;
    [SerializeField] private float _maxX = 7f;

    [Tooltip("Slots per row. Objects occupy a whole slot, so they cannot overlap.")]
    [SerializeField] private int _slots = 5;

    [Tooltip("Max objects per row. Keep well below slot count to leave gaps to fly through.")]
    [SerializeField] private int _maxPerRow = 2;

    [Header("Rates")]
    [Range(0f, 1f)]
    [Tooltip("Chance a spawned object is a hazard rather than a harmless obstacle.")]
    [SerializeField] private float _hazardChance = 0.5f;

    [Tooltip("Spawn a repair pickup every N rows. 0 = never.")]
    [SerializeField] private int _pickupEveryRows = 8;

    [Tooltip("Spawn a checkpoint every N world units. 0 = never.")]
    [SerializeField] private float _checkpointInterval = 150f;

    private System.Random _rng;

    private void Awake()
    {
        if (_levelRoot == null)
        {
            Debug.LogError("[LevelPopulator] No LevelRoot assigned.", this);
            return;
        }

        _rng = new System.Random(_seed);
        Populate();
    }

    private void Populate()
    {
        float slotWidth = (_maxX - _minX) / _slots;
        int rowIndex = 0;
        float nextCheckpoint = _checkpointInterval;

        for (float y = _startClearance; y < _levelHeight; y += _rowSpacing, rowIndex++)
        {
            float rowY = y + RandomRange(-_rowJitter, _rowJitter);

            // Pick distinct slots so two objects never land on the same spot.
            int count = 1 + _rng.Next(_maxPerRow);
            int taken = 0;
            for (int slot = 0; slot < _slots && taken < count; slot++)
            {
                // Reservoir-ish pick: spread choices across the row.
                int remainingSlots = _slots - slot;
                int stillNeeded = count - taken;
                if (_rng.Next(remainingSlots) >= stillNeeded) continue;

                float x = _minX + slotWidth * (slot + 0.5f);
                SpawnObject(x, rowY);
                taken++;
            }

            if (_pickupEveryRows > 0 && rowIndex > 0 && rowIndex % _pickupEveryRows == 0 && _pickupPrefab != null)
            {
                float x = RandomRange(_minX, _maxX);
                Spawn(_pickupPrefab, x, rowY + _rowSpacing * 0.5f);
            }

            if (_checkpointInterval > 0f && _checkpointPrefab != null && rowY >= nextCheckpoint)
            {
                // Centred, and offset off the row so it does not sit inside a hazard.
                Spawn(_checkpointPrefab, 0f, rowY + _rowSpacing * 0.5f);
                nextCheckpoint += _checkpointInterval;
            }
        }

        if (_winTriggerPrefab != null)
            Spawn(_winTriggerPrefab, 0f, _levelHeight + _rowSpacing);
    }

    private void SpawnObject(float x, float y)
    {
        bool hazard = _rng.NextDouble() < _hazardChance;
        GameObject[] pool = hazard ? _hazardPrefabs : _obstaclePrefabs;

        // Fall back to the other pool if one is empty.
        if (pool == null || pool.Length == 0)
            pool = hazard ? _obstaclePrefabs : _hazardPrefabs;

        if (pool == null || pool.Length == 0) return;

        Spawn(pool[_rng.Next(pool.Length)], x, y);
    }

    private void Spawn(GameObject prefab, float x, float y)
    {
        if (prefab == null) return;
        GameObject instance = Instantiate(prefab, _levelRoot);
        instance.transform.localPosition = new Vector3(x, y, 0f);
    }

    private float RandomRange(float min, float max)
    {
        return min + (float)_rng.NextDouble() * (max - min);
    }
}
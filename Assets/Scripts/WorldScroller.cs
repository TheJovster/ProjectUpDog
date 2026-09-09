using UnityEngine;

/// Moves LevelRoot downward and owns the authoritative scroll distance (GDD §9).
/// Distance is the single source of truth: parallax offsets and checkpoint
/// restore are pure functions of it, so nothing else needs scroll state.
///
/// This component has no opinion about game state — GameManager starts and
/// stops it.
public class WorldScroller : MonoBehaviour
{
    [Header("Scene References")]
    [Tooltip("Parent of all gameplay geometry: obstacles, hazards, NPCs, checkpoints, pickups.")]
    [SerializeField] private Transform _levelRoot;

    [Tooltip("Optional. When set, the player's upward effort adds to scroll speed (GDD §12).")]
    [SerializeField] private PlayerBalloonController _player;

    [Header("Speed")]
    [SerializeField] private float _baseScrollSpeed = 6f;

    [Tooltip("Extra units/sec added per unit of player upward input.")]
    [SerializeField] private float _playerInfluence = 4f;

    [Tooltip("Global scale on scroll speed. Raise over the level to increase difficulty.")]
    [SerializeField] private float _speedMultiplier = 1f;

    /// Total downward travel of the world so far.
    public float Distance { get; private set; }

    /// Scroll speed this frame, after input and multiplier (0 while stopped).
    public float CurrentSpeed { get; private set; }

    public bool IsScrolling { get; private set; } = true;

    public float SpeedMultiplier
    {
        get => _speedMultiplier;
        set => _speedMultiplier = value;
    }

    private void Start()
    {
        ApplyDistance();
    }

    private void Update()
    {
        if (!IsScrolling)
        {
            CurrentSpeed = 0f;
            return;
        }

        float speed = _baseScrollSpeed;
        if (_player != null) speed += _player.UpwardContribution * _playerInfluence;

        CurrentSpeed = speed * _speedMultiplier;
        Distance += CurrentSpeed * Time.deltaTime;

        ApplyDistance();
    }

    public void StartScrolling() => IsScrolling = true;

    public void StopScrolling()
    {
        IsScrolling = false;
        CurrentSpeed = 0f;
    }

    /// Rewind or jump the world to a distance (checkpoint restore, GDD §19).
    /// Applies immediately so the world does not show a stale frame.
    public void SetDistance(float distance)
    {
        Distance = distance;
        ApplyDistance();
    }

    private void ApplyDistance()
    {
        if (_levelRoot == null) return;
        Vector3 pos = _levelRoot.position;
        pos.y = -Distance;
        _levelRoot.position = pos;
    }
}
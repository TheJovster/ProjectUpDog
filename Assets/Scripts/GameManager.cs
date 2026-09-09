using UnityEngine;

/// Prototype god class: owns game state, scroll distance, health, checkpoint,
/// death/respawn and win. Deliberately monolithic for build speed.
/// Distance is the single source of truth — LevelRoot placement and parallax
/// are pure functions of it, so checkpoint restore is one float assignment.
public class GameManager : MonoBehaviour
{
    public enum State { Playing, Dialogue, Dead, Win }

    [Header("Scene References")]
    [SerializeField] private PlayerBalloonController _player;
    [SerializeField] private Transform _levelRoot;

    [Header("Scroll")]
    [SerializeField] private float _baseScrollSpeed = 6f;
    [SerializeField] private float _playerInfluence = 4f;

    [Header("Health")]
    [SerializeField] private int _maxHealth = 3;

    [Header("Flow")]
    [SerializeField] private float _respawnDelay = 1.5f;
    [SerializeField] private Vector3 _startPosition = Vector3.zero;

    public State Current { get; private set; } = State.Playing;
    public float Distance { get; private set; }
    public int Health { get; private set; }

    // Latest checkpoint. Defaults to level start.
    private float _checkpointDistance;
    private Vector3 _checkpointPosition;
    private int _checkpointHealth;
    private float _respawnTimer;

    private void Awake()
    {
        ServiceRegistry.Instance.Register(this);

        Health = _maxHealth;
        _checkpointHealth = _maxHealth;
        _checkpointPosition = _startPosition;
        _checkpointDistance = 0f;
    }

    private void OnDestroy()
    {
        // Registry is DontDestroyOnLoad; this is scene-scoped, so clear the entry
        // or the next scene load resolves a destroyed object.
        if (ServiceRegistry.Instance != null)
            ServiceRegistry.Instance.Unregister(this);
    }

    private void Update()
    {
        if (Current == State.Dead)
        {
            _respawnTimer -= Time.deltaTime;
            if (_respawnTimer <= 0f) RespawnAtCheckpoint();
            return;
        }

        if (Current != State.Playing) return;

        // Distance accumulates at base speed plus the player's upward effort (GDD §12).
        float speed = _baseScrollSpeed;
        if (_player != null) speed += _player.UpwardContribution * _playerInfluence;
        Distance += speed * Time.deltaTime;

        ApplyDistance();
    }

    private void ApplyDistance()
    {
        if (_levelRoot == null) return;
        Vector3 pos = _levelRoot.position;
        pos.y = -Distance;
        _levelRoot.position = pos;
    }

    // --- Health ---------------------------------------------------------

    public void Damage(int amount = 1)
    {
        if (Current != State.Playing) return;

        Health -= amount;
        if (Health <= 0)
        {
            Health = 0;
            Die();
        }
    }

    public void Heal(int amount = 1)
    {
        if (Current != State.Playing) return;
        Health = Mathf.Min(Health + amount, _maxHealth);
    }

    // --- Checkpoints ----------------------------------------------------

    public void SetCheckpoint(Vector3 playerPosition)
    {
        _checkpointDistance = Distance;
        _checkpointPosition = playerPosition;
        _checkpointHealth = Health;
    }

    private void RespawnAtCheckpoint()
    {
        Distance = _checkpointDistance;
        Health = _checkpointHealth;
        ApplyDistance();

        if (_player != null)
        {
            _player.ResetTo(_checkpointPosition);
            _player.enabled = true;
        }

        Current = State.Playing;
    }

    // --- Flow -----------------------------------------------------------

    private void Die()
    {
        Current = State.Dead;
        _respawnTimer = _respawnDelay;
        if (_player != null) _player.enabled = false;
    }

    public void Win()
    {
        Current = State.Win;
        if (_player != null) _player.enabled = false;
    }

    /// Called by NPC triggers to pause play for dialogue (GDD §21).
    public void EnterDialogue()
    {
        if (Current != State.Playing) return;
        Current = State.Dialogue;
        if (_player != null) _player.enabled = false;
    }

    public void ExitDialogue()
    {
        if (Current != State.Dialogue) return;
        Current = State.Playing;
        if (_player != null) _player.enabled = true;
    }
}
using UnityEngine;

/// Prototype god class: owns game state, health, checkpoint, death/respawn
/// and win. Deliberately monolithic for build speed.
/// Scroll distance lives on WorldScroller; this drives it and stores its
/// distance in checkpoints, so restore is one float assignment.
public class GameManager : MonoBehaviour
{
    public enum State { Playing, Dialogue, Cutscene, Dead, Win }

    [Header("Scene References")]
    [SerializeField] private PlayerBalloonController _player;
    [SerializeField] private WorldScroller _worldScroller;

    [Header("Health")]
    [SerializeField] private int _maxHealth = 3;

    [Header("Flow")]
    [SerializeField] private float _respawnDelay = 1.5f;
    [SerializeField] private Vector3 _startPosition = Vector3.zero;

    public State Current { get; private set; } = State.Playing;
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
        if (Current != State.Dead) return;

        _respawnTimer -= Time.deltaTime;
        if (_respawnTimer <= 0f) RespawnAtCheckpoint();
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
        _checkpointDistance = _worldScroller != null ? _worldScroller.Distance : 0f;
        _checkpointPosition = playerPosition;
        _checkpointHealth = Health;
    }

    private void RespawnAtCheckpoint()
    {
        Health = _checkpointHealth;
        if (_worldScroller != null)
        {
            _worldScroller.SetDistance(_checkpointDistance);
            _worldScroller.StartScrolling();
        }

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
        if (_worldScroller != null) _worldScroller.StopScrolling();
    }

    public void Win()
    {
        Current = State.Win;
        if (_player != null) _player.enabled = false;
        if (_worldScroller != null) _worldScroller.StopScrolling();
    }

    /// Called by NPC triggers to pause play for dialogue (GDD §21).
    public void EnterDialogue()
    {
        if (Current != State.Playing) return;
        Current = State.Dialogue;
        if (_player != null) _player.enabled = false;
        if (_worldScroller != null) _worldScroller.StopScrolling();
    }

    /// Slideshow cutscenes freeze play the same way dialogue does (GDD §21 pattern).
    public void EnterCutscene()
    {
        if (Current != State.Playing) return;
        Current = State.Cutscene;
        if (_player != null) _player.enabled = false;
        if (_worldScroller != null) _worldScroller.StopScrolling();
    }

    public void ExitCutscene()
    {
        if (Current != State.Cutscene) return;
        Current = State.Playing;
        if (_player != null) _player.enabled = true;
        if (_worldScroller != null) _worldScroller.StartScrolling();
    }

    public void ExitDialogue()
    {
        if (Current != State.Dialogue) return;
        Current = State.Playing;
        if (_player != null) _player.enabled = true;
        if (_worldScroller != null) _worldScroller.StartScrolling();
    }
}
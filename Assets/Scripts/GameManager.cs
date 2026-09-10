using System.Collections.Generic;
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
    [SerializeField] private PlayerDeathEffect _deathEffect;

    [Header("Health")]
    [SerializeField] private int _maxHealth = 3;

    [Tooltip("Seconds of invulnerability after taking a hit. Also absorbs duplicate collision callbacks from multi-collider setups.")]
    [SerializeField] private float _invulnerabilityTime = 0.5f;

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
    private float _invulnerableUntil;

    // Hazards and pickups consumed since the last checkpoint, re-enabled on respawn.
    private readonly List<GameObject> _consumed = new List<GameObject>();

    private void Awake()
    {
        ServiceRegistry.Instance.Register(this);

        Health = _maxHealth;
        _checkpointHealth = _maxHealth;
        _checkpointPosition = _startPosition;
        _checkpointDistance = 0f;

        Application.targetFrameRate = 60;
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

    public bool IsInvulnerable => Time.time < _invulnerableUntil;

    public void Damage(int amount = 1)
    {
        if (Current != State.Playing) return;
        if (IsInvulnerable) return;

        _invulnerableUntil = Time.time + _invulnerabilityTime;

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

    /// Register something disabled during play so respawn can restore it.
    public void RegisterConsumed(GameObject obj)
    {
        if (obj != null) _consumed.Add(obj);
    }

    public void SetCheckpoint(Vector3 playerPosition)
    {
        _checkpointDistance = _worldScroller != null ? _worldScroller.Distance : 0f;
        _checkpointPosition = playerPosition;
        _checkpointHealth = Health;

        // Anything consumed before this checkpoint stays consumed.
        _consumed.Clear();
    }

    private void RespawnAtCheckpoint()
    {
        Health = _checkpointHealth;
        _invulnerableUntil = 0f;

        // Restore hazards and pickups taken since the checkpoint.
        for (int i = 0; i < _consumed.Count; i++)
        {
            if (_consumed[i] != null) _consumed[i].SetActive(true);
        }
        _consumed.Clear();

        if (_worldScroller != null)
        {
            _worldScroller.SetDistance(_checkpointDistance);
            _worldScroller.StartScrolling();
        }

        if (_deathEffect != null) _deathEffect.ResetVisuals();

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

        // Freeze everything first, then play the pop.
        if (_player != null) _player.enabled = false;
        if (_worldScroller != null) _worldScroller.StopScrolling();
        if (_deathEffect != null) _deathEffect.Play();
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
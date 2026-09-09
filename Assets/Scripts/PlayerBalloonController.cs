using UnityEngine;
using UnityEngine.InputSystem;

/// Balloon-style player movement: buoyant upward drift, acceleration-based
/// steering, soft screen containment, and velocity-reflecting bounce.
/// All feel values are exposed for Inspector tuning (GDD §7).
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerBalloonController : MonoBehaviour
{
    [Header("Input")]
    [Tooltip("Reference to the Move action (Value / Vector2) in the project's Input Actions asset.")]
    [SerializeField] private InputActionReference _moveAction;

    [Header("Horizontal")]
    [SerializeField] private float _horizontalAcceleration = 30f;
    [SerializeField] private float _horizontalMaxSpeed = 8f;
    [SerializeField] private float _horizontalDrag = 20f;

    [Header("Vertical")]
    [SerializeField] private float _upwardBaseSpeed = 3f;      // neutral buoyancy (GDD §6)
    [SerializeField] private float _upwardMaxSpeed = 8f;       // holding Up
    [SerializeField] private float _downwardMaxSpeed = 4f;     // holding Down (not gravity)
    [SerializeField] private float _verticalAcceleration = 25f;

    [Header("Bounce")]
    [SerializeField] private float _bounceStrength = 2f;
    [SerializeField, Range(0f, 1f)] private float _collisionDamping = 0.6f;

    [Header("Screen Containment")]
    [SerializeField] private float _screenBoundaryStrength = 40f;
    [SerializeField, Range(0f, 1f)] private float _lowerBandFraction = 0.25f;
    [SerializeField, Range(0f, 1f)] private float _upperBandFraction = 0.60f;
    [SerializeField] private float _bandStrength = 40f;

    private Rigidbody2D _rigidbody;
    private Camera _camera;
    private InputAction _move;
    private Vector2 _input;
    private Vector2 _velocity;

    /// Normalized upward effort (-1..1). Read by ParallaxController so holding
    /// Up increases perceived ascent speed (GDD §12). Temporary coupling —
    /// see notes: this should feed WorldScroller once that exists.
    public float UpwardContribution => _input.y;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.gravityScale = 0f;      // buoyancy handled here, not by physics
        _rigidbody.freezeRotation = true;  // balloon shouldn't spin from collisions
        _camera = Camera.main;

        _move = _moveAction.action;
    }

    private void OnEnable() => _move.Enable();
    private void OnDisable() => _move.Disable();

    private void Update()
    {
        // Poll input in Update; apply physics in FixedUpdate.
        _input = _move.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        // Horizontal: accelerate toward the input target, drift back to zero otherwise.
        float targetX = _input.x * _horizontalMaxSpeed;
        float horizontalRate = Mathf.Abs(_input.x) > 0.01f ? _horizontalAcceleration : _horizontalDrag;
        _velocity.x = Mathf.MoveTowards(_velocity.x, targetX, horizontalRate * dt);

        // Vertical: neutral target is buoyancy; Up/Down bias it (GDD §6).
        float targetY;
        if (_input.y > 0.01f)
            targetY = Mathf.Lerp(_upwardBaseSpeed, _upwardMaxSpeed, _input.y);
        else if (_input.y < -0.01f)
            targetY = Mathf.Lerp(_upwardBaseSpeed, -_downwardMaxSpeed, -_input.y);
        else
            targetY = _upwardBaseSpeed;
        _velocity.y = Mathf.MoveTowards(_velocity.y, targetY, _verticalAcceleration * dt);

        ApplyScreenContainment(dt);

        _rigidbody.linearVelocity = _velocity;
    }

    private void ApplyScreenContainment(float dt)
    {
        if (_camera == null) return;

        Vector3 pos = transform.position;
        Vector3 min = _camera.ViewportToWorldPoint(new Vector3(0f, _lowerBandFraction, 0f));
        Vector3 max = _camera.ViewportToWorldPoint(new Vector3(1f, _upperBandFraction, 0f));

        // Horizontal edges: soft push back into the playable area (GDD §24).
        if (pos.x < min.x && _velocity.x < 0f)
            _velocity.x += (min.x - pos.x) * _screenBoundaryStrength * dt;
        else if (pos.x > max.x && _velocity.x > 0f)
            _velocity.x -= (pos.x - max.x) * _screenBoundaryStrength * dt;

        // Vertical band: keep the balloon inside the authored gameplay band (GDD §8).
        if (pos.y > max.y && _velocity.y > 0f)
            _velocity.y -= (pos.y - max.y) * _bandStrength * dt;
        else if (pos.y < min.y && _velocity.y < 0f)
            _velocity.y += (min.y - pos.y) * _bandStrength * dt;
    }

    /// Hard reset used by checkpoint respawn — clears momentum so the player
    /// does not resume with pre-death velocity.
    public void ResetTo(Vector3 position)
    {
        transform.position = position;
        _velocity = Vector2.zero;
        _input = Vector2.zero;
        if (_rigidbody != null) _rigidbody.linearVelocity = Vector2.zero;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Reflect off the surface normal, bleed energy (damping < 1 prevents
        // runaway velocity, GDD §15), add a small normal-direction pop.
        Vector2 normal = collision.GetContact(0).normal;
        _velocity = Vector2.Reflect(_velocity, normal) * _collisionDamping + normal * _bounceStrength;
    }
}
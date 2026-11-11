using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// 2D cannon (Unity 6). The cannon stays in place; moving the mouse up/down changes the
/// firing angle. A trajectory arc is shown while aiming and disappears when you fire.
///
/// SETUP
/// 1. Add this script to your cannon object (the object that should rotate).
/// 2. Add a child empty at the barrel tip and assign it to "Muzzle".
/// 3. Add a LineRenderer (World Space on, width ~0.05-0.1, simple material) and assign it.
///    Put it on a separate object (not a child of the cannon) or leave it as is; the script
///    writes world-space positions either way.
/// 4. Make a projectile prefab with Rigidbody2D + Collider2D and assign it.
/// 5. Play: move the mouse up/down to aim, left click or Space to fire.
///
/// Works with the new Input System, the legacy Input Manager, or both.
/// </summary>
public class CannonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform muzzle;
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private Rigidbody2D projectilePrefab;
    [Tooltip("Optional. Leave empty to keep the whole cannon static (only the arc moves).")]
    [SerializeField] private Transform barrelPivot;

    [Header("Aiming")]
    [SerializeField] private bool facingRight = true;
    [SerializeField] private float pitchSensitivity = 2f;   // degrees per mouse unit
    [SerializeField] private float minAngle = 5f;           // degrees above horizontal
    [SerializeField] private float maxAngle = 80f;
    [SerializeField] private float startAngle = 35f;
    [SerializeField] private bool lockCursor = true;

    [Header("Shooting")]
    [SerializeField] private float launchSpeed = 15f;
    [SerializeField] private float fireCooldown = 0.5f;     // arc reappears after this delay
    [SerializeField] private float projectileLifetime = 10f;

    [Header("Trajectory Preview")]
    [SerializeField] private int maxPoints = 60;
    [SerializeField] private float timeStep = 0.05f;
    [SerializeField] private LayerMask collisionMask = ~0;  // layers that stop the preview arc

    private float angle;
    private float nextFireTime;
    private Vector3[] points;

    private void Start()
    {
        angle = Mathf.Clamp(startAngle, minAngle, maxAngle);
        points = new Vector3[maxPoints];

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        bool canAim = Time.time >= nextFireTime;

        if (canAim)
        {
            angle = Mathf.Clamp(angle + GetMouseY() * pitchSensitivity, minAngle, maxAngle);
        }

        ApplyRotation();

        if (canAim)
        {
            DrawTrajectory();
            if (FirePressed()) Fire();
        }
        else
        {
            trajectoryLine.positionCount = 0; // arc hidden after firing
        }

        if (EscapePressed())
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private Vector2 AimDirection()
    {
        float rad = angle * Mathf.Deg2Rad;
        return new Vector2(facingRight ? Mathf.Cos(rad) : -Mathf.Cos(rad), Mathf.Sin(rad));
    }

    private void ApplyRotation()
    {
        // The cannon stays static. Only rotate something if a barrel pivot is assigned.
        if (barrelPivot == null)
        {
            return;
        }

        Vector2 dir = AimDirection();
        float z = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        //transform.rotation = Quaternion.Euler(0f, 0f, z);
        barrelPivot.rotation = Quaternion.Euler(0f, 0f, z);
    }

    private void DrawTrajectory()
    {
        if (trajectoryLine == null || muzzle == null) return;

        Vector2 origin = muzzle.position;
        Vector2 velocity = AimDirection() * launchSpeed;

        // Match the projectile's real gravity (project gravity * its gravityScale)
        float gravityScale = projectilePrefab != null ? projectilePrefab.gravityScale : 1f;
        Vector2 gravity = Physics2D.gravity * gravityScale;

        int count = 0;
        points[count++] = origin;

        for (int i = 1; i < maxPoints; i++)
        {
            float t = i * timeStep;
            Vector2 next = origin + velocity * t + 0.5f * gravity * t * t;
            Vector2 prev = points[count - 1];
            Vector2 segment = next - prev;

            RaycastHit2D hit = Physics2D.Raycast(prev, segment.normalized, segment.magnitude, collisionMask);
            if (hit.collider != null && !hit.collider.isTrigger)
            {
                points[count++] = hit.point;
                break;
            }

            points[count++] = next;
        }

        trajectoryLine.positionCount = count;
        trajectoryLine.SetPositions(points);
    }

    private void Fire()
    {
        nextFireTime = Time.time + fireCooldown;
        trajectoryLine.positionCount = 0;

        if (projectilePrefab == null || muzzle == null) return;

        Rigidbody2D proj = Instantiate(projectilePrefab, muzzle.position, Quaternion.identity);
        proj.linearVelocity = AimDirection() * launchSpeed;
        Destroy(proj.gameObject, projectileLifetime);
    }

    // ---------- Input helpers (new Input System or legacy) ----------

    private float GetMouseY()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null ? Mouse.current.delta.ReadValue().y * 0.05f : 0f;
#else
        return Input.GetAxis("Mouse Y");
#endif
    }

    private bool FirePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            || (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame);
#else
        return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space);
#endif
    }

    private bool EscapePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }
}
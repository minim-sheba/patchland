using UnityEngine;
using UnityEngine.InputSystem;
using OscJack;
using StarterAssets;

// Attach to PlayerArmature. Feeds Seaboard data into StarterAssetsInputs, as if it
// were another input device, so ThirdPersonController handles movement, animation
// and the Cinemachine camera.
[RequireComponent(typeof(StarterAssetsInputs))]
[RequireComponent(typeof(PlayerInput))]
public class SeaboardReceiver3D : MonoBehaviour
{
    // ThirdPersonController ignores look input smaller than this (its _threshold of 0.01 is squared)
    private const float MinLookInput = 0.1f;

    // mpe_voice limits each voice's glide to +/-1, then the Pd patch multiplies the average by 50
    private const float MaxGlide = 50f;

    [Header("OSC Settings")]
    [SerializeField] private int port = 8000;
    [SerializeField] private string oscAddress = "/seaboard/note_active";

    [Header("Seaboard Control")]
    [Tooltip("Degrees per second of turning at maximum glide")]
    [SerializeField] private float maxTurnRate = 45f;
    [Tooltip("Glide values below this have no effect. Pd's glide reaches its maximum of 50 at a small bend, so this is a small slice of a semitone")]
    [SerializeField] private float glideDeadZone = 5f;
    [Tooltip("Seconds taken to respond to glide. Longer ignores more vibrato, but feels slower")]
    [Min(0.01f)]
    [SerializeField] private float glideSmoothTime = 0.3f;

    [Header("Recovery")]
    [Tooltip("Let the mouse turn the camera, e.g. if stuck facing a wall. Turn off to protect against accidental bumps")]
    [SerializeField] private bool allowMouseLook = true;

    [Header("Debug")]
    [SerializeField] private bool logOscMessages = false;
    [SerializeField] private bool showOverlay = false;

    private StarterAssetsInputs input;
    private PlayerInput playerInput;
    private OscServer server;

    // Written on OscJack's background thread, read in Update()
    private volatile bool noteActive = false;
    private volatile float currentGlideValue = 0f;

    private bool seaboardIsMoving = false;
    private bool seaboardIsTurning = false;
    private float pendingYaw = 0f;
    private float smoothedGlide = 0f;

    void Start()
    {
        input = GetComponent<StarterAssetsInputs>();
        playerInput = GetComponent<PlayerInput>();

        // Set up OSC server
        server = new OscServer(port);
        server.MessageDispatcher.AddCallback(oscAddress, OnNoteActiveReceived);

        Debug.Log($"OSC Server started on port {port}, listening for {oscAddress}");
    }

    void Update()
    {
        UpdateMouseLook();

        bool isActive = noteActive;

        // Only write to the inputs while the Seaboard is in use, plus once to release
        // them, so that keyboard and mouse still work when it's idle
        if (isActive)
        {
            input.MoveInput(Vector2.up);
            seaboardIsMoving = true;
        }
        else if (seaboardIsMoving)
        {
            input.MoveInput(Vector2.zero);
            seaboardIsMoving = false;
        }

        // Smoothing averages out fast vibrato, leaving only sustained bends. Glide only
        // counts while a note is held, and easing back to 0 lets turns settle after note off.
        float targetGlide = isActive ? currentGlideValue : 0f;
        smoothedGlide = Mathf.Lerp(smoothedGlide, targetGlide, 1f - Mathf.Exp(-Time.deltaTime / glideSmoothTime));

        float turnRate = GetTurnRate(smoothedGlide);

        if (turnRate != 0f)
        {
            input.LookInput(new Vector2(GetLookInput(turnRate), 0f));
            seaboardIsTurning = true;
        }
        else if (seaboardIsTurning)
        {
            input.LookInput(Vector2.zero);
            pendingYaw = 0f;
            seaboardIsTurning = false;
        }
    }

    void OnDestroy()
    {
        if (server != null)
        {
            server.Dispose();
        }
    }

    private void UpdateMouseLook()
    {
        if (input.cursorInputForLook != allowMouseLook)
        {
            input.cursorInputForLook = allowMouseLook;

            // Otherwise the last mouse value would stick and the camera would keep turning
            input.LookInput(Vector2.zero);
        }
    }

    // Measured from the edge of the dead zone, so turning starts gently rather than with a jump
    private float GetTurnRate(float glide)
    {
        float amount = Mathf.InverseLerp(glideDeadZone, MaxGlide, Mathf.Abs(glide));
        return Mathf.Sign(glide) * amount * maxTurnRate;
    }

    // ThirdPersonController treats look input from a mouse as degrees this frame, and from
    // other devices as degrees per second, so convert to match the current control scheme
    private float GetLookInput(float degreesPerSecond)
    {
        if (playerInput.currentControlScheme != "KeyboardMouse")
        {
            return degreesPerSecond;
        }

        // Slow turns can be below MinLookInput on a single frame, and the faster the machine, the
        // smaller each frame's share. Saving them up keeps slow glides working at any frame rate.
        pendingYaw += degreesPerSecond * Time.deltaTime;

        if (Mathf.Abs(pendingYaw) < MinLookInput)
        {
            return 0f;
        }

        float yaw = pendingYaw;
        pendingYaw = 0f;
        return yaw;
    }

    private void OnNoteActiveReceived(string address, OscDataHandle data)
    {
        if (data.GetElementCount() > 1) // Check we have at least 2 values
        {
            bool isActive = data.GetElementAsFloat(0) > 0f; // First value
            float glideValue = data.GetElementAsFloat(1);   // Second value

            currentGlideValue = glideValue; // Store for use in Update()
            noteActive = isActive;

            if (logOscMessages)
            {
                Debug.Log($"Received OSC: {address} = {isActive}, glide {glideValue}");
            }
        }
    }

    // Off by default, as it would appear in the projection
    void OnGUI()
    {
        if (!showOverlay)
        {
            return;
        }

        GUI.Label(new Rect(10, 10, 200, 20), $"OSC Port: {port}");
        GUI.Label(new Rect(10, 30, 200, 20), $"Note Active: {noteActive}");
        GUI.Label(new Rect(10, 50, 200, 20), $"Glide: {currentGlideValue}");
    }
}

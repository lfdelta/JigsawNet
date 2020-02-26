using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float HeightSpeed;
    public float HeightSpeedHeld;
    public float HeightSpeedTransitionTime;

    public float PanSpeed;
    public float PanSpeedHeld;
    public float PanSpeedTransitionTime;

    public float[] PitchAngles;

    public Vector2 XBounds;
    public Vector2 YBounds;
    public Vector2 ZBounds;

    private Camera cam;
    private int PitchInd;
    private float HeightHeldTime;
    private float PanHeldTime;

    private JigsawPlayerController LocalPlayer;
    

    void Start()
    {
        cam = Camera.main;

        PitchInd = 0;
        UpdateCameraPitch();

        HeightHeldTime = 0.0f;
        PanHeldTime = 0.0f;

        StaticJigsawData.ObjectManager.RequestObject("LocalJigsawPlayer", ReceiveLocalJigsawPlayer);

        // TODO: set starting height based on board dimensions (see BoardScalingHandler)
    }


    private void ReceiveLocalJigsawPlayer(GameObject PlayerObject)
    {
        LocalPlayer = PlayerObject.GetComponent<JigsawPlayerController>();
    }


    void Update()
    {
        bool cameraMoved = false;
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        float moveY = Input.GetAxis("Zoom");

        if (moveX != 0.0f || moveZ != 0.0f)
        {
            PanHeldTime += Time.deltaTime;
            float speed = (PanHeldTime >= PanSpeedTransitionTime) ? PanSpeedHeld : PanSpeed;
            moveX *= speed * Time.deltaTime;
            moveZ *= speed * Time.deltaTime;
            cameraMoved = true;
        }
        else
        {
            PanHeldTime = 0.0f;
        }
        if (moveY != 0.0f)
        {
            HeightHeldTime += Time.deltaTime;
            moveY *= (HeightHeldTime >= HeightSpeedTransitionTime) ? HeightSpeedHeld : HeightSpeed;
            moveY *= Time.deltaTime;
            cameraMoved = true;
        }
        else
        {
            HeightHeldTime = 0.0f;
        }

        Vector3 newPos = cam.transform.position + new Vector3(moveX, moveY, moveZ);
        newPos.x = Mathf.Clamp(newPos.x, XBounds.x, XBounds.y);
        newPos.y = Mathf.Clamp(newPos.y, YBounds.x, YBounds.y);
        newPos.z = Mathf.Clamp(newPos.z, ZBounds.x, ZBounds.y);
        cam.transform.position = newPos;

        if (Input.GetButtonDown("AngleUp"))
        {
            if (PitchInd > 0)
            {
                --PitchInd;
                UpdateCameraPitch();
                cameraMoved = true;
            }
        }
        else if (Input.GetButtonDown("AngleDown"))
        {
            if (PitchInd < PitchAngles.Length - 1)
            {
                ++PitchInd;
                UpdateCameraPitch();
                cameraMoved = true;
            }
        }

        if (cameraMoved && LocalPlayer != null)
        {
            LocalPlayer.HandleMouseWorldPositionUpdated();
        }
    }


    private void UpdateCameraPitch()
    {
        float pitch = PitchAngles[PitchInd];
        cam.transform.rotation = Quaternion.Euler(90.0f - pitch, 0.0f, 0.0f);
    }
}

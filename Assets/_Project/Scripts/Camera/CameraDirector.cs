using Cinemachine;
using UnityEngine;

public class CameraDirector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SkaterController skater;
    [SerializeField] private CinemachineVirtualCamera groundCam;
    [SerializeField] private CinemachineVirtualCamera airCam;
    [SerializeField] private CinemachineVirtualCamera staticCam;

    [Header("Toggle")]
    [SerializeField] private KeyCode toggleStaticKey = KeyCode.Tab;

    private const int ActivePriority = 20;
    private const int InactivePriority = 10;
    private const int DormantPriority = 0;

    private bool staticMode;

    private void Start()
    {
        ApplyPriorities();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleStaticKey))
        {
            staticMode = !staticMode;
            ApplyPriorities();
        }

        if (!staticMode)
        {
            bool airborne = skater.CurrentState == SkaterController.State.Airborne;
            airCam.Priority = airborne ? ActivePriority : InactivePriority;
            groundCam.Priority = airborne ? InactivePriority : ActivePriority;
        }
    }

    private void ApplyPriorities()
    {
        if (staticMode)
        {
            staticCam.Priority = ActivePriority;
            groundCam.Priority = DormantPriority;
            airCam.Priority = DormantPriority;
        }
        else
        {
            staticCam.Priority = DormantPriority;
            groundCam.Priority = ActivePriority;
            airCam.Priority = InactivePriority;
        }
    }
}

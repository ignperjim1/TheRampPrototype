using UnityEngine;

// Instantiates the skater prefab at a spawn point and wires its components
// into the scene-level consumers (HUD, camera, audio, post-process...).
// Runs in Start so all consumers' Awake/OnEnable have already executed with
// their refs null; the Set* calls then cycle enable to re-subscribe cleanly.
public class SkaterSpawner : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private GameObject skaterPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Scene-side dependencies (injected into the skater)")]
    [SerializeField] private SpeedMaintainer speedMaintainer;

    [Header("Consumers to wire")]
    [SerializeField] private Hitstop hitstop;
    [SerializeField] private CameraFeedback cameraFeedback;
    [SerializeField] private PostProcessFeedback postProcessFeedback;
    [SerializeField] private SkaterAudio skaterAudio;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private HUD hud;

    private void Start()
    {
        if (skaterPrefab == null)
        {
            Debug.LogError("SkaterSpawner: skaterPrefab not assigned.");
            return;
        }

        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion rot = spawnPoint != null ? spawnPoint.rotation : transform.rotation;
        GameObject instance = Instantiate(skaterPrefab, pos, rot);

        var skater = instance.GetComponent<SkaterController>();
        var trickPerformer = instance.GetComponent<TrickPerformer>();

        if (skater == null || trickPerformer == null)
        {
            Debug.LogError("SkaterSpawner: prefab is missing SkaterController or TrickPerformer at root.");
            return;
        }

        skater.SetSpeedMaintainer(speedMaintainer);

        if (hitstop != null) hitstop.SetTrickPerformer(trickPerformer);
        if (cameraFeedback != null)
        {
            cameraFeedback.SetTrickPerformer(trickPerformer);
            cameraFeedback.SetFocusTarget(instance.transform);
        }
        if (postProcessFeedback != null)
        {
            postProcessFeedback.SetSkater(skater);
            postProcessFeedback.SetTrickPerformer(trickPerformer);
        }
        if (skaterAudio != null)
        {
            skaterAudio.SetSkater(skater);
            skaterAudio.SetTrickPerformer(trickPerformer);
        }
        if (scoreManager != null) scoreManager.SetTrickPerformer(trickPerformer);
        if (hud != null) hud.SetTrickPerformer(trickPerformer);
    }
}

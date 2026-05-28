using UnityEngine;

public class SkaterAudio : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SkaterController skater;
    [SerializeField] private TrickPerformer trickPerformer;

    [Header("Mix")]
    [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
    [SerializeField, Range(0f, 0.3f)] private float pitchVariance = 0.06f;
    [SerializeField] private int oneShotVoices = 6;

    [Header("Rolling Loop")]
    [SerializeField, Range(0f, 1f)] private float rollingVolume = 0.5f;
    [SerializeField] private float rollingFadeSpeed = 6f;

    [Header("One-Shot Volumes")]
    [SerializeField, Range(0f, 1f)] private float jumpVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float trickVolume = 0.6f;
    [SerializeField, Range(0f, 1f)] private float landVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float successVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float failVolume = 1f;

    private AudioClip jumpClip;
    private AudioClip trickClip;
    private AudioClip landClip;
    private AudioClip successClip;
    private AudioClip failClip;

    private AudioSource rollingSource;
    private AudioSource[] voices;
    private int nextVoice;

    private SkaterController.State lastState;
    private bool hasTouchedGround;
    private bool wasRespawning;

    private void Awake()
    {
        if (skater == null) skater = GetComponent<SkaterController>() ?? FindObjectOfType<SkaterController>();
        if (trickPerformer == null) trickPerformer = GetComponent<TrickPerformer>() ?? FindObjectOfType<TrickPerformer>();

        jumpClip = SfxSynth.JumpSwoosh();
        trickClip = SfxSynth.TrickSwoosh();
        landClip = SfxSynth.LandImpact();
        successClip = SfxSynth.SuccessJingle();
        failClip = SfxSynth.FailThud();

        rollingSource = CreateSource();
        rollingSource.clip = SfxSynth.RollingLoop();
        rollingSource.loop = true;
        rollingSource.volume = 0f;
        rollingSource.Play();

        voices = new AudioSource[Mathf.Max(1, oneShotVoices)];
        for (int i = 0; i < voices.Length; i++) voices[i] = CreateSource();

        if (skater != null) lastState = skater.CurrentState;
    }

    private void OnEnable()
    {
        if (trickPerformer != null)
        {
            trickPerformer.OnTrickStarted += HandleTrickStarted;
            trickPerformer.OnCleanLanding += HandleCleanLanding;
            trickPerformer.OnTrickFailed += HandleTrickFailed;
        }
    }

    private void OnDisable()
    {
        if (trickPerformer != null)
        {
            trickPerformer.OnTrickStarted -= HandleTrickStarted;
            trickPerformer.OnCleanLanding -= HandleCleanLanding;
            trickPerformer.OnTrickFailed -= HandleTrickFailed;
        }
    }

    private void Update()
    {
        if (skater == null) return;

        bool respawning = skater.IsRespawning;
        bool rolling = skater.CurrentState == SkaterController.State.Rolling;

        // During (and on the frame after) a respawn, keep the state tracker in
        // sync so the teleport back to spawn never fires a phantom land sound.
        if (respawning || wasRespawning)
        {
            lastState = skater.CurrentState;
            wasRespawning = respawning;
            FadeRolling(0f);
            return;
        }

        if (rolling) hasTouchedGround = true;

        if (skater.CurrentState != lastState)
        {
            if (hasTouchedGround)
            {
                if (lastState == SkaterController.State.Rolling)
                    PlayOneShot(jumpClip, jumpVolume);
                else
                    PlayOneShot(landClip, landVolume);
            }
            lastState = skater.CurrentState;
        }

        FadeRolling(rolling ? rollingVolume : 0f);
    }

    private void FadeRolling(float target)
    {
        rollingSource.volume = Mathf.MoveTowards(
            rollingSource.volume,
            target * masterVolume,
            rollingFadeSpeed * Time.unscaledDeltaTime);
    }

    private void HandleTrickStarted(TrickDefinition trick) => PlayOneShot(trickClip, trickVolume);
    private void HandleCleanLanding() => PlayOneShot(successClip, successVolume);

    private void HandleTrickFailed(TrickDefinition trick)
    {
        // The wipeout cuts any in-flight one-shot (e.g. a lingering success
        // jingle) so the thud lands clean instead of overlapping.
        for (int i = 0; i < voices.Length; i++) voices[i].Stop();
        PlayOneShot(failClip, failVolume);
    }

    private void PlayOneShot(AudioClip clip, float volume)
    {
        if (clip == null) return;
        AudioSource src = voices[nextVoice];
        nextVoice = (nextVoice + 1) % voices.Length;
        src.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
        src.PlayOneShot(clip, Mathf.Clamp01(volume) * masterVolume);
    }

    private AudioSource CreateSource()
    {
        var src = gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.spatialBlend = 0f;
        return src;
    }
}

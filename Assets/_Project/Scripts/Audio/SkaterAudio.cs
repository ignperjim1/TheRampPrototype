using UnityEngine;

public class SkaterAudio : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ScoreManager scoreManager;

    [Header("Mix")]
    [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
    [Tooltip("Variación aleatoria de pitch aplicada a cada one-shot (±este valor). Pequeñas cantidades (0.05-0.1) evitan que los SFX repetidos suenen idénticos. Rango limitado a 0.3 porque más arriba ya distorsiona la identidad del sonido sintetizado.")]
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
    [SerializeField, Range(0f, 1f)] private float scoreVolume = 0.4f;

    private AudioClip jumpClip;
    private AudioClip trickClip;
    private AudioClip landClip;
    private AudioClip successClip;
    private AudioClip failClip;
    private AudioClip scoreClip;

    private AudioSource rollingSource;
    // Dedicated source for the jump swoosh (instead of the round-robin pool)
    // so HandleTrickStarted can Stop() it cleanly when a trick begins right
    // after takeoff — the trick swoosh overrides the jump in seconds.
    private AudioSource jumpSource;
    private AudioSource[] voices;
    private int nextVoice;

    private SkaterController.State lastState;
    private bool hasTouchedGround;
    private bool wasRespawning;
    private SkaterController skater;
    private TrickPerformer trickPerformer;


    private void Awake()
    {
        jumpClip = SfxSynth.JumpSwoosh();
        trickClip = SfxSynth.TrickSwoosh();
        landClip = SfxSynth.LandImpact();
        successClip = SfxSynth.SuccessJingle();
        failClip = SfxSynth.FailThud();
        scoreClip = SfxSynth.ScoreClick();

        rollingSource = CreateSource();
        rollingSource.clip = SfxSynth.RollingLoop();
        rollingSource.loop = true;
        rollingSource.volume = 0f;
        rollingSource.Play();

        jumpSource = CreateSource();

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
        if (scoreManager != null) scoreManager.OnScoreChanged += HandleScoreChanged;
    }

    private void OnDisable()
    {
        if (trickPerformer != null)
        {
            trickPerformer.OnTrickStarted -= HandleTrickStarted;
            trickPerformer.OnCleanLanding -= HandleCleanLanding;
            trickPerformer.OnTrickFailed -= HandleTrickFailed;
        }
        if (scoreManager != null) scoreManager.OnScoreChanged -= HandleScoreChanged;
    }

    public void SetSkater(SkaterController s)
    {
        skater = s;
        if (skater != null) lastState = skater.CurrentState;
    }

    public void SetTrickPerformer(TrickPerformer tp)
    {
        enabled = false;
        trickPerformer = tp;
        enabled = true;
    }

    private void HandleScoreChanged(int newScore, int delta)
    {
        if (delta > 0) PlayOneShot(scoreClip, scoreVolume);
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
                {
                    jumpSource.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
                    jumpSource.PlayOneShot(jumpClip, Mathf.Clamp01(jumpVolume) * masterVolume);
                }
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

    private void HandleTrickStarted(TrickDefinition trick)
    {
        jumpSource.Stop();
        PlayOneShot(trickClip, trickVolume);
    }
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

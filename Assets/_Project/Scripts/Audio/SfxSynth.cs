using UnityEngine;

// Procedural sound generation: every SFX is synthesized into an AudioClip at
// runtime, so the prototype needs no external audio assets.
public static class SfxSynth
{
    private const int SampleRate = 44100;

    private enum Wave { Sine, Square, Triangle, Saw }

    public static AudioClip JumpSwoosh()
    {
        int n = Samples(0.30f);
        float[] d = new float[n];
        float phase = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            float freq = Mathf.Lerp(220f, 900f, t);
            phase = Advance(phase, freq);
            float tone = Osc(Wave.Sine, phase);
            float noise = White();
            float env = AR(t, 0.05f);
            d[i] = (tone * 0.45f + noise * 0.55f) * env * 0.7f;
        }
        return Make("sfx_jump", d);
    }

    public static AudioClip TrickSwoosh()
    {
        int n = Samples(0.18f);
        float[] d = new float[n];
        float phase = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            float freq = Mathf.Lerp(700f, 1500f, t);
            phase = Advance(phase, freq);
            float tone = Osc(Wave.Triangle, phase);
            float noise = White();
            float env = AR(t, 0.04f);
            d[i] = (tone * 0.6f + noise * 0.4f) * env * 0.6f;
        }
        return Make("sfx_trick", d);
    }

    public static AudioClip LandImpact()
    {
        int n = Samples(0.28f);
        int clickN = Samples(0.02f);
        float[] d = new float[n];
        float phase = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            float freq = Mathf.Lerp(140f, 55f, t);
            phase = Advance(phase, freq);
            float tone = Osc(Wave.Sine, phase);
            float click = i < clickN ? White() * (1f - (float)i / clickN) : 0f;
            float env = ExpDecay(t, 6f);
            d[i] = (tone * 0.9f + click * 0.5f) * env * 0.9f;
        }
        return Make("sfx_land", d);
    }

    public static AudioClip SuccessJingle()
    {
        // C5, E5, G5, C6 ascending arpeggio.
        float[] notes = { 523.25f, 659.25f, 783.99f, 1046.50f };
        return Arpeggio("sfx_success", notes, 0.08f, Wave.Triangle, 0.85f);
    }

    public static AudioClip FailThud()
    {
        // Heavy crash: a sharp noise crack, a deep sine boom dropping in pitch,
        // and a lowpassed noise tail that rumbles out (the tumble/wipeout).
        int n = Samples(0.45f);
        int crackN = Samples(0.04f);
        float[] d = new float[n];
        float phase = 0f;
        float lp = 0f;
        const float cutoff = 0.25f;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            float freq = Mathf.Lerp(120f, 35f, Mathf.Sqrt(t));
            phase = Advance(phase, freq);
            float boom = Osc(Wave.Sine, phase) * ExpDecay(t, 4f);

            float noise = White();
            lp += cutoff * (noise - lp);
            float crack = i < crackN ? noise * (1f - (float)i / crackN) : 0f;
            float body = lp * ExpDecay(t, 3.5f);

            d[i] = boom * 0.85f + body * 0.6f + crack * 0.7f;
        }
        Normalize(d, 0.95f);
        return Make("sfx_fail", d);
    }

    public static AudioClip RollingLoop()
    {
        int n = Samples(1.0f);
        float[] d = new float[n];
        float lp = 0f;
        float subPhase = 0f;
        const float cutoff = 0.08f;
        for (int i = 0; i < n; i++)
        {
            float noise = White();
            lp += cutoff * (noise - lp);
            subPhase = Advance(subPhase, 60f);
            float rumble = Osc(Wave.Sine, subPhase) * 0.15f;
            d[i] = lp * 0.6f + rumble;
        }
        Normalize(d, 0.85f);
        CrossfadeForLoop(d, Samples(0.02f));
        return Make("sfx_rolling", d);
    }

    private static AudioClip Arpeggio(string name, float[] freqs, float noteDur, Wave wave, float volume)
    {
        int per = Samples(noteDur);
        int n = per * freqs.Length;
        float[] d = new float[n];
        for (int note = 0; note < freqs.Length; note++)
        {
            float phase = 0f;
            for (int i = 0; i < per; i++)
            {
                float t = (float)i / per;
                phase = Advance(phase, freqs[note]);
                d[note * per + i] = Osc(wave, phase) * AR(t, 0.06f) * volume;
            }
        }
        return Make(name, d);
    }

    private static float Advance(float phase, float freq)
    {
        phase += freq / SampleRate;
        if (phase >= 1f) phase -= 1f;
        return phase;
    }

    private static float Osc(Wave wave, float phase)
    {
        switch (wave)
        {
            case Wave.Square: return phase < 0.5f ? 1f : -1f;
            case Wave.Triangle: return 1f - 4f * Mathf.Abs(phase - 0.5f);
            case Wave.Saw: return 2f * phase - 1f;
            default: return Mathf.Sin(phase * 2f * Mathf.PI);
        }
    }

    private static float White() => Random.value * 2f - 1f;

    // Linear attack over attackFrac of the clip, then linear decay to zero.
    private static float AR(float t, float attackFrac)
    {
        if (t < attackFrac) return t / attackFrac;
        return Mathf.Clamp01(1f - (t - attackFrac) / (1f - attackFrac));
    }

    private static float ExpDecay(float t, float k) => Mathf.Exp(-k * t);

    private static void Normalize(float[] d, float peak)
    {
        float max = 0f;
        for (int i = 0; i < d.Length; i++) max = Mathf.Max(max, Mathf.Abs(d[i]));
        if (max < 1e-5f) return;
        float scale = peak / max;
        for (int i = 0; i < d.Length; i++) d[i] *= scale;
    }

    // Blends the head of the buffer with its tail so the loop point is seamless.
    private static void CrossfadeForLoop(float[] d, int fade)
    {
        fade = Mathf.Min(fade, d.Length / 2);
        for (int k = 0; k < fade; k++)
        {
            float f = (float)k / fade;
            d[k] = d[k] * f + d[d.Length - fade + k] * (1f - f);
        }
    }

    private static int Samples(float seconds) => Mathf.Max(1, Mathf.CeilToInt(seconds * SampleRate));

    private static AudioClip Make(string name, float[] data)
    {
        var clip = AudioClip.Create(name, data.Length, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}

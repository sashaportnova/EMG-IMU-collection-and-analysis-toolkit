using System;
using UnityEngine;
public class BiquadFilter
{
    protected float a0, a1, a2, b1, b2;
    protected float z1, z2;

    public BiquadFilter(float a0, float a1, float a2, float b1, float b2)
    {
        SetCoefficients(a0, a1, a2, b1, b2);
    }

    public void SetCoefficients(float a0, float a1, float a2, float b1, float b2)
    {
        this.a0 = a0;
        this.a1 = a1;
        this.a2 = a2;
        this.b1 = b1;
        this.b2 = b2;
        //Reset();
    }

    // ✅ Mark these as virtual
    public virtual float Apply(float input)
    {
        float output = a0 * input + a1 * z1 + a2 * z2 - b1 * z1 - b2 * z2;
        z2 = z1;
        z1 = output;
        return output;
    }

    public virtual void Reset()
    {
        z1 = z2 = 0f;
    }

    // ---------- Filter Designers ----------

    public static BiquadFilter DesignLowpass(float fs, float cutoff, int order)
    {
        return CreateCascade(order, (f) => DesignLowpassSection(fs, cutoff));
    }

    public static BiquadFilter DesignHighpass(float fs, float cutoff, int order)
    {
        return CreateCascade(order, (f) => DesignHighpassSection(fs, cutoff));
    }

    private static BiquadFilter CreateCascade(int order, Func<int, BiquadFilter> sectionDesigner)
    {
        int sections = order / 2;
        var cascade = new BiquadFilter[sections];
        for (int i = 0; i < sections; i++)
            cascade[i] = sectionDesigner(i);

        return new BiquadFilterCascade(cascade);
    }

    private static BiquadFilter DesignLowpassSection(float fs, float cutoff)
    {
        float omega = 2 * Mathf.PI * cutoff / fs;
        float sinOmega = Mathf.Sin(omega);
        float cosOmega = Mathf.Cos(omega);
        float alpha = sinOmega / (2.0f * Mathf.Sqrt(2)); // Q = sqrt(2)/2 for Butterworth

        float a0 = 1 + alpha;
        float b0 = (1 - cosOmega) / 2 / a0;
        float b1 = (1 - cosOmega) / a0;
        float b2 = b0;
        float a1 = -2 * cosOmega / a0;
        float a2 = (1 - alpha) / a0;

        return new BiquadFilter(b0, b1, b2, a1, a2);
    }

    private static BiquadFilter DesignHighpassSection(float fs, float cutoff)
    {
        float omega = 2 * Mathf.PI * cutoff / fs;
        float sinOmega = Mathf.Sin(omega);
        float cosOmega = Mathf.Cos(omega);
        float alpha = sinOmega / (2.0f * Mathf.Sqrt(2)); // Q = sqrt(2)/2 for Butterworth

        float a0 = 1 + alpha;
        float b0 = (1 + cosOmega) / 2 / a0;
        float b1 = -(1 + cosOmega) / a0;
        float b2 = b0;
        float a1 = -2 * cosOmega / a0;
        float a2 = (1 - alpha) / a0;

        return new BiquadFilter(b0, b1, b2, a1, a2);
    }
}

// Simple subclass to handle cascaded filters (e.g., 4th-order)
public class BiquadFilterCascade : BiquadFilter
{
    private BiquadFilter[] filters;

    public BiquadFilterCascade(BiquadFilter[] filters) : base(0, 0, 0, 0, 0)
    {
        this.filters = filters;
        Reset(); // Now safe to call because filters is initialized
    }

    public override float Apply(float input)
    {
        float x = input;
        foreach (var f in filters)
            x = f.Apply(x);
        return x;
    }

    public override void Reset()
    {
        foreach (var f in filters)
            f.Reset();
    }
}
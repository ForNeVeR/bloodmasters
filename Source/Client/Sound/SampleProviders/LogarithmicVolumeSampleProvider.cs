using System;
using CodeImp.Bloodmasters.Client.Extensions;
using NAudio.Wave;

namespace CodeImp.Bloodmasters.Client.SampleProviders;

public class LogarithmicVolumeSampleProvider : ISampleProvider
{
    /// <summary>Should be completely silent.</summary>
    public const float MinVolumeHundredthsOfDb = -10000f;
    /// <summary>Unadjusted original volume.</summary>
    private const float MaxVolumeHundredthsOfDb = 0f;

    public WaveFormat WaveFormat => this.source.WaveFormat;

    /// <summary>The volume is measured in 1/100 of decibel, same as it was back in DirectSound.</summary>
    public float VolumeHundredthsOfDb
    {
        get => _volumeHundredthsOfDb;
        set
        {
            _volumeHundredthsOfDb = Math.Clamp(value, MinVolumeHundredthsOfDb, MaxVolumeHundredthsOfDb);
            _multiplier = MathF.Pow(10, _volumeHundredthsOfDb / 2000f);
        }
    }

    private readonly ISampleProvider source;
    private float _volumeHundredthsOfDb;
    private float _multiplier;

    public LogarithmicVolumeSampleProvider(ISampleProvider source)
    {
        this.source = source;
    }

    public int Read(float[] buffer, int offset, int count)
    {
        int samplesRead = source.Read(buffer, offset, count);
        if (VolumeHundredthsOfDb != 0)
        {
            buffer.AsSpan(offset, count).MultiplyByScalar(_multiplier);
        }
        return samplesRead;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NAudio.Wave;

namespace CodeImp.Bloodmasters.Client;

public class AudioSampleProvider : ISampleProvider
{
    /// <summary>Should be completely silent.</summary>
    private const float MinVolumeHundredthsOfDb = -10000f;
    /// <summary>Unadjusted original volume.</summary>
    private const float MaxVolumeHundredthsOfDb = 0f;

    private readonly object _stateLock = new();
    private readonly float[] _audioData;
    private bool _playing;
    private bool _stopped;
    private int _position;
    private bool _shouldRepeat;
    private float _volumeHundredthsOfDb = MinVolumeHundredthsOfDb;

    public WaveFormat WaveFormat { get; }

    public bool IsPlaying
    {
        get { lock (_stateLock) return _playing; }
    }

    public int CurrentPosition
    {
        get { lock(_stateLock) return _position; }
        set { lock(_stateLock) _position = value; }
    }

    public int Length
    {
        get { lock (_stateLock) return _audioData.Length; }
    }

    public void Stop()
    {
        lock (_stateLock) _stopped = true;
    }

    public bool ShouldRepeat
    {
        get { lock (_stateLock) return _shouldRepeat; }
        set { lock (_stateLock) _shouldRepeat = value; }
    }

    /// <summary>The volume is measured in 1/100 of decibel, same as it was back in DirectSound.</summary>
    public float VolumeHundredthsOfDb
    {
        get { lock (_stateLock) return _volumeHundredthsOfDb; }
        set { lock (_stateLock) _volumeHundredthsOfDb = Math.Clamp(value, MinVolumeHundredthsOfDb, MaxVolumeHundredthsOfDb); }
    }

    public AudioSampleProvider Clone()
    {
        return new AudioSampleProvider(WaveFormat, _audioData)
        {
            CurrentPosition = CurrentPosition,
            ShouldRepeat = ShouldRepeat
        };
    }

    private AudioSampleProvider(WaveFormat waveFormat, float[] audioData)
    {
        WaveFormat = waveFormat;
        _audioData = audioData;
    }

    public static AudioSampleProvider ReadFromFile(string audioFilePath)
    {
        using var audioFileReader = new AudioFileReader(audioFilePath);
        var wholeFile = new List<float>((int)(audioFileReader.Length / 4));
        var readBuffer = new float[audioFileReader.WaveFormat.SampleRate * audioFileReader.WaveFormat.Channels];
        int samplesRead;
        while ((samplesRead = audioFileReader.Read(readBuffer, 0, readBuffer.Length)) > 0)
        {
            wholeFile.AddRange(readBuffer.Take(samplesRead));
        }
        return new AudioSampleProvider(audioFileReader.WaveFormat, wholeFile.ToArray());
    }

    public int Read(float[] buffer, int offset, int count)
    {
        lock (_stateLock)
        {
            if (!_shouldRepeat || Length <= 0) return ReadNoLock(buffer, offset, count);

            while (count > 0)
            {
                var samplesRead = ReadNoLock(buffer, offset, count);
                offset += samplesRead;
                count -= samplesRead;
            }

            return count;
        }
    }

    private int ReadNoLock(float[] buffer, int offset, int count)
    {
        var availableSamples = _audioData.Length - _position;
        var samplesToCopy = _stopped ? 0 : Math.Min(availableSamples, count);
        _playing = samplesToCopy > 0;
        if (!_playing) return 0;

        Array.Copy(_audioData, _position, buffer, offset, samplesToCopy);

        var multiplier = MathF.Pow(10, _volumeHundredthsOfDb / 2000f);
        for (var i = 0; i < samplesToCopy; ++i)
        {
            buffer[offset + i] *= multiplier;
        }

        Interlocked.Add(ref _position, samplesToCopy);
        return samplesToCopy;
    }
}

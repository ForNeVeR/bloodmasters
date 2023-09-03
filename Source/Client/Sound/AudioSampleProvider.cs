using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using NAudio.Wave;

namespace CodeImp.Bloodmasters.Client;

internal enum SoundState
{
    Stopped, Playing
}

internal class AudioSampleProvider : ISampleProvider
{
    /// <summary>Should be completely silent.</summary>
    public const float MinVolumeHundredthsOfDb = -10000f;
    /// <summary>Unadjusted original volume.</summary>
    private const float MaxVolumeHundredthsOfDb = 0f;

    private readonly string _fileName;
    private readonly object _stateLock = new();
    private readonly float[] _audioData;

    public override string ToString() => _fileName;

    private SoundState _state;
    private int _position;
    private bool _shouldRepeat;
    private float _volumeHundredthsOfDb = MaxVolumeHundredthsOfDb;

    public WaveFormat WaveFormat { get; }

    public SoundState State
    {
        get { lock (_stateLock) return _state; }
        set { lock (_stateLock) _state = value; }
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

    public void Stop() => State = SoundState.Stopped;

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
        return new AudioSampleProvider(_fileName, WaveFormat, _audioData)
        {
            CurrentPosition = CurrentPosition,
            ShouldRepeat = ShouldRepeat
        };
    }

    private AudioSampleProvider(string fileName, WaveFormat waveFormat, float[] audioData)
    {
        _fileName = fileName;
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
        return new AudioSampleProvider(
            Path.GetFileName(audioFilePath),
            audioFileReader.WaveFormat,
            wholeFile.ToArray());
    }

    public int Read(float[] buffer, int offset, int count)
    {
        lock (_stateLock)
        {
            if (State == SoundState.Stopped) return 0;
            if (!_shouldRepeat || Length <= 0) return ReadNoLock(buffer, offset, count);

            var remaining = count;
            while (remaining > 0)
            {
                var samplesRead = ReadNoLock(buffer, offset, remaining);
                offset += samplesRead;
                remaining -= samplesRead;
                if (_position >= _audioData.Length)
                {
                    Debug.Assert(_shouldRepeat);
                    _position = 0;
                }
            }

            return count;
        }
    }

    private int ReadNoLock(float[] buffer, int offset, int count)
    {
        var availableSamples = _audioData.Length - _position;
        var samplesToCopy = Math.Min(availableSamples, count);
        if (samplesToCopy == 0) return 0;

        Buffer.BlockCopy(_audioData, _position * sizeof(float), buffer, offset * sizeof(float), samplesToCopy * sizeof(float));

        var multiplier = MathF.Pow(10, _volumeHundredthsOfDb / 2000f);
        for (var i = 0; i < samplesToCopy; ++i)
        {
            buffer[offset + i] *= multiplier;
        }

        Interlocked.Add(ref _position, samplesToCopy);
        return samplesToCopy;
    }
}

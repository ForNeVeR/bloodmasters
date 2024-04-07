using System;
using System.Diagnostics;
using System.IO;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Bloodmasters.Client.SampleProviders;

[DebuggerDisplay("{_fileName}")]
public class CombinedSampleProvider : ISampleProvider
{
    public WaveFormat WaveFormat { get; }

    public float VolumeHundredthsOfDb
    {
        get
        {
            lock (_stateLock) return volumeSampleSampleProvider.VolumeHundredthsOfDb;
        }
        set
        {
            lock (_stateLock) volumeSampleSampleProvider.VolumeHundredthsOfDb = value;
        }
    }

    /// <summary>
    /// Controls pan effect of the source sample provider. Must be in range [-1, 1], where -1 is full left, 0 is center and 1 is full right.
    /// </summary>
    public float Pan
    {
        get
        {
            if (_panningSampleProvider == null) return 0f;

            lock (_stateLock)
            {
                return _panningSampleProvider.Pan;
            }

        }
        set
        {
            if (_panningSampleProvider == null) return;

            lock (_stateLock)
            {
                _panningSampleProvider.Pan = Math.Clamp(value, -1f, 1f);
            }
        }
    }

    public int CurrentPosition
    {
        get { lock(_stateLock) return _audioSampleProvider.CurrentPosition; }
        set { lock(_stateLock) _audioSampleProvider.CurrentPosition = value; }
    }

    public int Length
    {
        get { lock (_stateLock) return _audioSampleProvider.Length; }
    }

    public bool ShouldRepeat
    {
        get { lock (_stateLock) return _audioSampleProvider.ShouldRepeat; }
        set { lock (_stateLock) _audioSampleProvider.ShouldRepeat = value; }
    }

    public bool Playing => State == SoundState.Playing;

    private SoundState State
    {
        get { lock (_stateLock) return _state; }
        set { lock (_stateLock) _state = value; }
    }

    private SoundState _state;

    private readonly object _stateLock;

    private readonly AudioSampleProvider _audioSampleProvider;
    private readonly LogarithmicVolumeSampleProvider volumeSampleSampleProvider;
    private readonly PanningSampleProvider? _panningSampleProvider;

    private readonly string _fileName;

    private CombinedSampleProvider(
        WaveFormat waveFormat,
        AudioSampleProvider audioSampleProvider,
        LogarithmicVolumeSampleProvider volumeSampleSampleProvider,
        PanningSampleProvider? panningSampleProvider,
        object stateLock,
        string fileName)
    {
        WaveFormat = waveFormat;
        _audioSampleProvider = audioSampleProvider;
        this.volumeSampleSampleProvider = volumeSampleSampleProvider;
        _panningSampleProvider = panningSampleProvider;
        _stateLock = stateLock;
        _fileName = fileName;
    }

    public static CombinedSampleProvider ReadFromFile(string audioFilePath)
    {
        var locker = new object();

        var audioSampleProvider = AudioSampleProvider.ReadFromFile(audioFilePath, locker);
        var volumeSampleProvider = new LogarithmicVolumeSampleProvider(audioSampleProvider);

        return new CombinedSampleProvider(
            volumeSampleProvider.WaveFormat,
            audioSampleProvider,
            volumeSampleProvider,
            panningSampleProvider: null,
            locker,
            Path.GetFileName(audioFilePath));
    }

    public void Play()
    {
        State = SoundState.Playing;
    }

    public void Stop()
    {
        State = SoundState.Stopped;
    }

    public CombinedSampleProvider Clone(bool positional)
    {
        var audioSampleProvider = _audioSampleProvider.Clone();
        var volumeSampleProvider = new LogarithmicVolumeSampleProvider(audioSampleProvider);

        PanningSampleProvider? panningSampleProvider = null;
        if (positional)
        {
            panningSampleProvider = new PanningSampleProvider(volumeSampleProvider);
        }

        return new CombinedSampleProvider(
            panningSampleProvider?.WaveFormat ?? volumeSampleProvider.WaveFormat,
            audioSampleProvider,
            volumeSampleProvider,
            panningSampleProvider,
            _stateLock,
            _fileName)
        {
            ShouldRepeat = ShouldRepeat,
            CurrentPosition = CurrentPosition,
            State = State
        };
    }

    public int Read(float[] buffer, int offset, int count)
    {
        if (State == SoundState.Stopped) return 0;

        if (_panningSampleProvider != null)
        {
            return _panningSampleProvider.Read(buffer, offset, count);
        }

        return volumeSampleSampleProvider.Read(buffer, offset, count);
    }
}

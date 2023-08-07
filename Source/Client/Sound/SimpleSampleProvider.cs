using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NAudio.Wave;

namespace CodeImp.Bloodmasters.Client;

public class SimpleSampleProvider : ISampleProvider
{
    private readonly object _stateLock = new();
    private readonly float[] _audioData;
    private bool _playing;
    private bool _stopped;
    private int _position;
    private bool _shouldRepeat;

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

    public SimpleSampleProvider Clone()
    {
        return new SimpleSampleProvider(WaveFormat, _audioData)
        {
            CurrentPosition = CurrentPosition,
            ShouldRepeat = ShouldRepeat
        };
    }

    private SimpleSampleProvider(WaveFormat waveFormat, float[] audioData)
    {
        WaveFormat = waveFormat;
        _audioData = audioData;
    }

    public static SimpleSampleProvider ReadFromFile(string audioFilePath)
    {
        using var audioFileReader = new AudioFileReader(audioFilePath);
        var wholeFile = new List<float>((int)(audioFileReader.Length / 4));
        var readBuffer = new float[audioFileReader.WaveFormat.SampleRate * audioFileReader.WaveFormat.Channels];
        int samplesRead;
        while ((samplesRead = audioFileReader.Read(readBuffer, 0, readBuffer.Length)) > 0)
        {
            wholeFile.AddRange(readBuffer.Take(samplesRead));
        }
        return new SimpleSampleProvider(audioFileReader.WaveFormat, wholeFile.ToArray());
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
        Interlocked.Add(ref _position, samplesToCopy);
        return samplesToCopy;
    }
}

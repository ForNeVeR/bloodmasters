using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using NAudio.Wave;

namespace CodeImp.Bloodmasters.Client.SampleProviders;

[DebuggerDisplay("{_fileName}")]
public class AudioSampleProvider : ISampleProvider
{
    private readonly string _fileName;
    private readonly object _stateLock;
    private readonly float[] _audioData;

    private int _position;
    private bool _shouldRepeat;

    public WaveFormat WaveFormat { get; }

    public int CurrentPosition
    {
        get { lock(_stateLock) return _position; }
        set { lock(_stateLock) _position = value; }
    }

    public int Length
    {
        get { lock (_stateLock) return _audioData.Length; }
    }

    public bool ShouldRepeat
    {
        get { lock (_stateLock) return _shouldRepeat; }
        set { lock (_stateLock) _shouldRepeat = value; }
    }

    private AudioSampleProvider(WaveFormat waveFormat, float[] audioData, string fileName, object stateLock)
    {
        WaveFormat = waveFormat;
        _audioData = audioData;
        _fileName = fileName;
        _stateLock = stateLock;
    }

    public static AudioSampleProvider ReadFromFile(string audioFilePath, object stateLock)
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
            audioFileReader.WaveFormat,
            wholeFile.ToArray(),
            Path.GetFileName(audioFilePath),
            stateLock);
    }

    public int Read(float[] buffer, int offset, int count)
    {
        lock (_stateLock)
        {
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

    public AudioSampleProvider Clone()
    {
        return new AudioSampleProvider(WaveFormat, _audioData, _fileName, _stateLock)
        {
            CurrentPosition = CurrentPosition,
            ShouldRepeat = ShouldRepeat,
        };
    }

    private int ReadNoLock(float[] buffer, int offset, int count)
    {
        var availableSamples = _audioData.Length - _position;
        var samplesToCopy = Math.Min(availableSamples, count);
        if (samplesToCopy == 0) return 0;

        Buffer.BlockCopy(_audioData, _position * sizeof(float), buffer, offset * sizeof(float), samplesToCopy * sizeof(float));

        Interlocked.Add(ref _position, samplesToCopy);
        return samplesToCopy;
    }
}

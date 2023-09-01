using System;
using JetBrains.Lifetimes;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace CodeImp.Bloodmasters.Client;

internal class NAudioPlaybackEngine : IDisposable
{
    private readonly IWavePlayer _outputDevice;
    private readonly MixingSampleProvider _mixer;

    public NAudioPlaybackEngine(WaveFormat waveFormat)
    {
        _outputDevice = new DirectSoundOut();
        _mixer = new MixingSampleProvider(waveFormat)
        {
            ReadFully = true
        };
        _outputDevice.Init(_mixer);
        _outputDevice.Play();
    }

    public ISampleProvider ConvertToRightChannelCount(ISampleProvider input)
    {
        if (input.WaveFormat.SampleRate != 44100)
        {
            var outFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, input.WaveFormat.Channels);
            using var resampler = new MediaFoundationResampler(input.ToWaveProvider(), outFormat);

            input = resampler.ToSampleProvider();
        }

        if (input.WaveFormat.Channels == _mixer.WaveFormat.Channels)
        {
            return input;
        }
        if (input.WaveFormat.Channels == 1 && _mixer.WaveFormat.Channels == 2)
        {
            return new MonoToStereoSampleProvider(input);
        }

        throw new NotImplementedException("Not yet implemented this channel count conversion");
    }

    public void PlaySound(Lifetime lifetime, ISampleProvider sound)
    {
        lifetime.TryBracket(
            () => _mixer.AddMixerInput(sound),
            () => _mixer.RemoveMixerInput(sound));
    }

    public void Dispose()
    {
        _outputDevice.Dispose();
    }
}

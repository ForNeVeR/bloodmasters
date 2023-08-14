using System;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace CodeImp.Bloodmasters.Client;

class NAudioPlaybackEngine : IDisposable
{
    private readonly IWavePlayer _outputDevice;
    private readonly MixingSampleProvider _mixer;

    public WaveFormat WaveFormat => _mixer.WaveFormat;

    public NAudioPlaybackEngine(WaveFormat waveFormat)
    {
        _outputDevice = new WaveOutEvent();
        _mixer = new MixingSampleProvider(waveFormat)
        {
            ReadFully = true
        };
        _outputDevice.Init(_mixer);
        _outputDevice.Play();
    }

    private ISampleProvider ConvertToRightChannelCount(ISampleProvider input)
    {
        // TODO: Convert the sound to 44khz if it isn't already
        if (input.WaveFormat.Channels == _mixer.WaveFormat.Channels)
        {
            return input;
        }
        if (input.WaveFormat.Channels == 1 && _mixer.WaveFormat.Channels == 2)
        {
            // TODO: Figure out how to dispose of these providers correctly. They won't work for _mixer.RemoveMixerInput below
            return new MonoToStereoSampleProvider(input);
        }
        throw new NotImplementedException("Not yet implemented this channel count conversion");
    }

    public void PlaySound(AudioSampleProvider sound)
    {
        _mixer.AddMixerInput(ConvertToRightChannelCount(sound));
    }

    public void StopSound(ISampleProvider sound) =>
        _mixer.RemoveMixerInput(sound);

    public void Dispose()
    {
        _outputDevice.Dispose();
    }
}

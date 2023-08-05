using System.Collections.Generic;
using System.Linq;
using CodeImp.Bloodmasters.Client;
using NAudio.Wave;

namespace FireAndForgetAudioSample
{
    class CachedSound
    {
        public float[] AudioData { get; private set; }
        public WaveFormat WaveFormat { get; private set; }
        public CachedSound(ISound sound) : this(sound.Filename) {} // TODO: Get rid of this, move everything into Sound
        public CachedSound(string audioFileName)
        {
            using (var audioFileReader = new AudioFileReader(audioFileName))
            {
                // TODO: could add resampling in here if required
                var resampler = new MediaFoundationResampler(audioFileReader, new WaveFormat(44100, 16, 2));
                WaveFormat = resampler.WaveFormat;

                //WaveFormat = audioFileReader.WaveFormat;
                var wholeFile = new List<float>((int)(audioFileReader.Length / 4));
                var readBuffer = new float[audioFileReader.WaveFormat.SampleRate * audioFileReader.WaveFormat.Channels];
                int samplesRead;
                while ((samplesRead = audioFileReader.Read(readBuffer, 0, readBuffer.Length)) > 0)
                {
                    wholeFile.AddRange(readBuffer.Take(samplesRead));
                }
                AudioData = wholeFile.ToArray();
            }
        }
    }
}

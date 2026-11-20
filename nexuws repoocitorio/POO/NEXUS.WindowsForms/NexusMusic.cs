using System;
using System.IO;
using System.Media;

namespace NEXUS.WindowsForms;

// Original ambient synthesizer loop generated locally. No copyrighted audio files needed.
public sealed class NexusMusic : IDisposable
{
    private SoundPlayer? player;
    private MemoryStream? stream;
    public bool Enabled { get; private set; }
    public int Volume { get; private set; } = 65;

    public void Set(bool enabled, int volume)
    {
        Stop();
        Enabled = enabled;
        Volume = Math.Clamp(volume, 0, 100);
        if (!Enabled || Volume == 0) return;
        const int rate = 22050, seconds = 8;
        int samples = rate * seconds;
        stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true))
        {
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + samples * 2);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
            writer.Write(16); writer.Write((short)1); writer.Write((short)1);
            writer.Write(rate); writer.Write(rate * 2); writer.Write((short)2); writer.Write((short)16);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            writer.Write(samples * 2);
            for (int i = 0; i < samples; i++)
            {
                double t = (double)i / rate;
                double fade = Math.Min(1, Math.Min(t / .6, (seconds - t) / .6));
                double wave = Math.Sin(2 * Math.PI * 110 * t) * .40 +
                              Math.Sin(2 * Math.PI * 164.81 * t) * .25 +
                              Math.Sin(2 * Math.PI * 220 * t) * .20 +
                              Math.Sin(2 * Math.PI * .125 * t) * Math.Sin(2 * Math.PI * 329.63 * t) * .10;
                short value = (short)Math.Clamp(wave * fade * (Volume / 100.0) * 28000, -32767, 32767);
                writer.Write(value);
            }
        }
        stream.Position = 0;
        player = new SoundPlayer(stream);
        player.Load();
        player.PlayLooping();
    }

    public void Stop()
    {
        player?.Stop();
        player?.Dispose();
        player = null;
        stream?.Dispose();
        stream = null;
    }

    public void Dispose() => Stop();
}

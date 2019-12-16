using NAudio.Wave;

namespace Webmilio.Launchpad.SoundBoard.Sounds
{
    public class SoundBind
    {
        public SoundBind(string path, int gridIndex) : this(path, gridIndex % 8, gridIndex / 8)
        {
        }

        public SoundBind(string path, int x, int y)
        {
            Path = path;

            X = x;
            Y = y;
        }


        public ISampleProvider Play() => new AudioFileReader(Path);


        public string Path { get; }

        public int X { get; }
        public int Y { get; }
    }
}
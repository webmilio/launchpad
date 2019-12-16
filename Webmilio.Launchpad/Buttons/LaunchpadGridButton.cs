namespace Webmilio.Launchpad.Buttons
{
    public class LaunchpadGridButton : LaunchpadButton
    {
        public LaunchpadGridButton(LaunchpadDevice device, int index) : base(device, LaunchpadButtonType.Grid, index)
        {
            X = index % 8;
            Y = index / 8;
        }

        
        public int X { get; }

        public int Y { get; }
    }
}

using Midi;

namespace Webmilio.Launchpad.Buttons
{
    public class LaunchpadToolbarButton : LaunchpadButton
    {
        public LaunchpadToolbarButton(LaunchpadDevice device, int index) : base(device, LaunchpadButtonType.Toolbar, index)
        {
        }


        protected override void SetLED(int value) => 
            Device.OutputDevice.SendControlChange(Channel.Channel1, (Control) Index, value);
    }
}

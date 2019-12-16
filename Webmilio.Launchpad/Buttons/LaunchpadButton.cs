using System.Threading.Tasks;
using Midi;

namespace Webmilio.Launchpad.Buttons
{
    public abstract class LaunchpadButton
    {
        protected LaunchpadButton(LaunchpadDevice device, LaunchpadButtonType type, int index)
        {
            Device = device;
            Type = type;
            Index = index;
        }


        public void TurnOn() => SetBrightness(LaunchpadButtonBrightness.Full, LaunchpadButtonBrightness.Full);

        public void TurnOff() => SetBrightness(LaunchpadButtonBrightness.Off, LaunchpadButtonBrightness.Off);


        public void SetBrightness(LaunchpadButtonBrightness red, LaunchpadButtonBrightness green)
        {
            if (RedBrightness == red && GreenBrightness == green)
                return;

            RedBrightness = red;
            GreenBrightness = green;

            int vel = ((int) GreenBrightness << 4) | (int) RedBrightness;

            if (Device.DoubleBuffered)
                vel |= 12;

            SetLED(vel);
        }

        protected virtual void SetLED(int value) => Device.OutputDevice.SendNoteOn(Channel.Channel1, (Pitch)Index, value);


        public async void Flash(LaunchpadButtonBrightness red, LaunchpadButtonBrightness green, int duration, bool original = true)
        {
            LaunchpadButtonBrightness
                originalRed = RedBrightness,
                originalGreen = GreenBrightness;

            SetBrightness(red, green);

            await Task.Delay(duration);

            SetBrightness(original ? originalRed : LaunchpadButtonBrightness.Off, original ? originalGreen : LaunchpadButtonBrightness.Off);
        }


        public LaunchpadDevice Device { get; }

        public LaunchpadButtonType Type { get; }

        public int Index { get; }


        public LaunchpadButtonState State { get; internal set; }

        public LaunchpadButtonBrightness RedBrightness { get; private set; }

        public LaunchpadButtonBrightness GreenBrightness { get; private set; }
    }
}

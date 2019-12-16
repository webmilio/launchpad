using System;

namespace Webmilio.Launchpad.Buttons
{
    public class LaunchpadButtonPressedEventArgs<T> : EventArgs where T : LaunchpadButton
    {
        public LaunchpadButtonPressedEventArgs(T button)
        {
            Button = button;
        }


        public T Button { get; }
    }
}
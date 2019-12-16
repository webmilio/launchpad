using System;

namespace Webmilio.Launchpad
{
    public class LaunchpadException : Exception
    {
        public LaunchpadException()
        {
        }

        public LaunchpadException(string message) : base(message)
        {
        }
    }
}
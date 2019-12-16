using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using Webmilio.Launchpad.Buttons;
using Webmilio.Launchpad.SoundBoard.Sounds;

namespace Webmilio.Launchpad.SoundBoard
{
    internal class Program
    {
        public const LaunchpadButtonBrightness
            IDLE_RED = LaunchpadButtonBrightness.Medium,
            IDLE_GREEN = LaunchpadButtonBrightness.Medium;


        private static void Main(string[] args)
        {
            try
            {
                Device = new LaunchpadDevice
                {
                    DoubleBuffered = true
                };

                Console.WriteLine("Launchpad found");
            }
            catch
            {
                Console.WriteLine("No launchpad found");
                Console.ReadLine();
                return;
            }

            Device.Reset();
            Device.IndicateWorking().Wait();

            SoundLoader = new SoundLoader();

            SoundLoader.ForAll(soundBind => Device.GetGridButton(soundBind.X, soundBind.Y).SetBrightness(IDLE_RED, IDLE_GREEN));


            /*for (int i = 0; i < WaveIn.DeviceCount; i++)
                Console.WriteLine("{0}: {1}", i, WaveIn.GetCapabilities(i).ProductName);

            Console.Write("Use Microphone ? ");
            InputChoice = int.Parse(Console.ReadLine());

            WaveInEvent wi = new WaveInEvent()
            {
                DeviceNumber = InputChoice,
                BufferMilliseconds = 10,
                NumberOfBuffers = 3
            };*/

            WasapiCapture wi = new WasapiCapture();

            /*List<DirectSoundDeviceInfo> deviceInfos = DirectSoundOut.Devices.ToList();

            for (int i = 0; i < deviceInfos.Count; i++)
                Console.WriteLine("{0}: {1} - {2}", i, deviceInfos[i].Guid, deviceInfos[i].Description);*/


            var enumerator = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.All, DeviceState.All).ToList();
            for (int i = 0; i < enumerator.Count; i++)
            {
                MMDevice wasapi = enumerator[i];

                Console.WriteLine($"{i} {wasapi.DataFlow} {wasapi.FriendlyName} {wasapi.DeviceFriendlyName} {wasapi.State}");
            }

            Console.Write("Output Device ? ");
            OutputChoice = int.Parse(Console.ReadLine());

            WasapiOut wo = new WasapiOut(enumerator[OutputChoice], AudioClientShareMode.Shared, true, 10);


            //WO = new DirectSoundOut(deviceInfos[OutputChoice].Guid);
            WO = wo;
            WO.Init(new WaveInProvider(wi));
            WO.Play();

            /*WOEvent = new DirectSoundOut(deviceInfos[OutputChoice].Guid);
            WOEventFeedback = new DirectSoundOut();*/

            wi.StartRecording();
            Device.LaunchpadButtonPressed += Device_OnLaunchpadButtonPressed;

            string input = null;

            while (!(input = Console.ReadLine()).Equals("exit", StringComparison.CurrentCultureIgnoreCase))
            {
            }
        }


        private static void Device_OnLaunchpadButtonPressed(object sender, LaunchpadButtonPressedEventArgs<LaunchpadButton> e)
        {
            LaunchpadButton button = e.Button;

            button.Flash(LaunchpadButtonBrightness.Off, LaunchpadButtonBrightness.Full, 250);


            //using AudioFileReader afr = new AudioFileReader(@"L:\Music\Sounds\movie_1.mp3");

            if (e.Button is LaunchpadGridButton lgb)
            {
                SoundBind soundBind = SoundLoader.Find(lgb.X, lgb.Y);

                if (soundBind != default)
                    PlayWO(soundBind.Play());
            }
        }

        private static void PlayWO(ISampleProvider provider)
        {
            WOEvent.Init(provider);
            WOEvent.Play();

            WOEventFeedback.Init(provider);
            WOEventFeedback.Play();
        }


        public static LaunchpadDevice Device { get; private set; }

        public static SoundLoader SoundLoader { get; private set; }

        public static int InputChoice { get; private set; }
        public static int OutputChoice { get; private set; }
        public static int OutputChoiceFeedback { get; private set; }

        public static IWavePlayer WO { get; private set; }
        public static IWavePlayer WOEvent { get; private set; }
        public static IWavePlayer WOEventFeedback { get; private set; }
    }
}

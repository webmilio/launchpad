using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Midi;
using Webmilio.Launchpad.Buttons;

namespace Webmilio.Launchpad
{
    public class LaunchpadDevice
    {
        public const int
            TOOLBAR_FLAT = 104, TOOLBAR_MULT = 1,
            SIDE_FLAT = 8, SIDE_MULT = 16,
            GRID_FLAT = 0, GRID_MULT = 16;


        public event EventHandler<LaunchpadButtonPressedEventArgs<LaunchpadButton>> LaunchpadButtonPressed;
        public event EventHandler<LaunchpadButtonPressedEventArgs<LaunchpadToolbarButton>> LaunchpadToolbarButtonPressed;
        public event EventHandler<LaunchpadButtonPressedEventArgs<LaunchpadSideButton>> LaunchpadSideButtonPressed;
        public event EventHandler<LaunchpadButtonPressedEventArgs<LaunchpadGridButton>> LaunchpadGridButtonPressed;


        private readonly LaunchpadToolbarButton[] _toolbarButtons;
        private readonly LaunchpadSideButton[] _sideButtons;
        private readonly LaunchpadGridButton[,] _gridButtons;

        private bool _doubleBuffered;


        public LaunchpadDevice() : this(0)
        {
        }

        public LaunchpadDevice(int index) : this(index, 8, 8)
        {
        }

        public LaunchpadDevice(int index, int rows, int columns) : this(index, rows, columns, 120)
        {
        }

        public LaunchpadDevice(int index, int rows, int columns, int clockRate)
        {
            Index = index;

            Rows = rows;
            Columns = columns;

            _toolbarButtons = CreateArray(columns, i => new LaunchpadToolbarButton(this, i * TOOLBAR_MULT + TOOLBAR_FLAT));
            _sideButtons = CreateArray(rows, i => new LaunchpadSideButton(this, i * SIDE_MULT + SIDE_FLAT));


            _gridButtons = new LaunchpadGridButton[Rows, Columns];

            for (int i = 0; i < _gridButtons.GetLength(0); i++)
                for (int j = 0; j < _gridButtons.GetLength(1); j++)
                    _gridButtons[i, j] = new LaunchpadGridButton(this, i + j * GRID_MULT + GRID_FLAT);


            int deviceIndex = 0;
            InputDevice = InputDevice.InstalledDevices.Where(x => x.Name.Contains("Launchpad")).FirstOrDefault(x => deviceIndex++ == index);

            deviceIndex = 0;
            OutputDevice = OutputDevice.InstalledDevices.Where(x => x.Name.Contains("Launchpad")).FirstOrDefault(x => deviceIndex++ == index);

            if (InputDevice == null)
                throw new LaunchpadException("Unable to find input device.");
            if (OutputDevice == null)
                throw new LaunchpadException("Unable to find output device.");

            InputDevice.Open();
            OutputDevice.Open();

            InputDevice.StartReceiving(new Clock(clockRate));
            InputDevice.NoteOn += InputDevice_OnNoteOn;
            InputDevice.ControlChange += InputDevice_OnControlChange;
        }

        private static T[] CreateArray<T>(int count, Func<int, T> creator)
        {
            T[] array = new T[count];

            for (int i = 0; i < array.Length; i++)
                array[i] = creator(i);

            return array;
        }


        protected virtual void InputDevice_OnControlChange(ControlChangeMessage msg)
        {
            LaunchpadToolbarButton button = GetToolbarButton((int)msg.Control - 104);

            if (button == default)
                return;

            button.State = (LaunchpadButtonState)msg.Value;

            if (button.State == LaunchpadButtonState.Down)
            {
                LaunchpadButtonPressed?.Invoke(this, new LaunchpadButtonPressedEventArgs<LaunchpadButton>(button));
                LaunchpadToolbarButtonPressed?.Invoke(this, new LaunchpadButtonPressedEventArgs<LaunchpadToolbarButton>(button));
            }
        }

        protected virtual void InputDevice_OnNoteOn(NoteOnMessage msg)
        {
            LaunchpadButton button = GetButton(msg.Pitch);

            if (button == default)
                return;

            button.State = (LaunchpadButtonState)msg.Velocity;

            if (button.State == LaunchpadButtonState.Down)
            {
                LaunchpadButtonPressed?.Invoke(this, new LaunchpadButtonPressedEventArgs<LaunchpadButton>(button));

                if (button is LaunchpadSideButton lsb)
                    LaunchpadSideButtonPressed?.Invoke(this, new LaunchpadButtonPressedEventArgs<LaunchpadSideButton>(lsb));

                if (button is LaunchpadGridButton lgb)
                    LaunchpadGridButtonPressed?.Invoke(this, new LaunchpadButtonPressedEventArgs<LaunchpadGridButton>(lgb));
            }
        }


        public LaunchpadButton GetButton(LaunchpadButtonType type, int index)
        {
            switch (type)
            {
                case LaunchpadButtonType.Grid:
                    return GetGridButton(index % 8, index / 8);
                case LaunchpadButtonType.Side:
                    return GetSideButton(index);
                case LaunchpadButtonType.Toolbar:
                    return GetToolbarButton(index);
                default:
                    return default;
            }
        }

        public LaunchpadGridButton GetGridButton(int x, int y) => _gridButtons[x, y];
        public LaunchpadToolbarButton GetToolbarButton(int index) => _toolbarButtons[index];
        public LaunchpadSideButton GetSideButton(int index) => _sideButtons[index];

        private LaunchpadButton GetButton(Pitch pitch)
        {
            int
                x = (int)pitch % 16,
                y = (int)pitch / 16;

            if (x < 8 && y < 8)
                return _gridButtons[x, y];

            if (x == 8 && y < 8)
                return _sideButtons[y];

            return null;
        }


        public LaunchpadButton this[LaunchpadButtonType type, int index] => GetButton(type, index);
        public LaunchpadButton this[int x, int y] => GetGridButton(x, y);


        public async Task IndicateWorking()
        {
            SetLighting(LaunchpadButtonBrightness.Full, LaunchpadButtonBrightness.Off);
            await Task.Delay(250);

            SetLighting(LaunchpadButtonBrightness.Full, LaunchpadButtonBrightness.Full);
            await Task.Delay(250);

            SetLighting(LaunchpadButtonBrightness.Off, LaunchpadButtonBrightness.Full);
            await Task.Delay(250);

            SetLighting(LaunchpadButtonBrightness.Off, LaunchpadButtonBrightness.Off);
            await Task.Delay(250);
        }

        public virtual void Flash(LaunchpadButtonBrightness red, LaunchpadButtonBrightness green, int duration, bool original = true)
        {
            for (int i = 0; i < _toolbarButtons.Length; i++)
                _toolbarButtons[i].Flash(red, green, duration, original);

            for (int x = 0; x < _gridButtons.GetLength(0); x++)
                for (int y = 0; y < _gridButtons.GetLength(1); y++)
                    _gridButtons[y, x].Flash(red, green, duration, original);

            for (int i = 0; i < _sideButtons.Length; i++)
                _sideButtons[i].Flash(red, green, duration, original);
        }

        public virtual void SetLighting(LaunchpadButtonBrightness red, LaunchpadButtonBrightness green)
        {
            for (int i = 0; i < _toolbarButtons.Length; i++)
                _toolbarButtons[i].SetBrightness(red, green);

            for (int x = 0; x < _gridButtons.GetLength(0); x++)
                for (int y = 0; y < _gridButtons.GetLength(1); y++)
                    _gridButtons[x, y].SetBrightness(red, green);

            for (int i = 0; i < _sideButtons.Length; i++)
                _sideButtons[i].SetBrightness(red, green);
        }


        private void StartDoubleBuffering() => OutputDevice.SendControlChange(Channel.Channel1, (Control)0, 32 | 16 | 1);
        private void EndDoubleBuffering() => OutputDevice.SendControlChange(Channel.Channel1, (Control)0, 32 | 16);


        public void Reset()
        {
            OutputDevice.SendControlChange(Channel.Channel1, (Control)0, 0);

            foreach (LaunchpadButton button in Buttons)
                button.SetBrightness(LaunchpadButtonBrightness.Off, LaunchpadButtonBrightness.Off);
        }

        public void Refresh()
        {
            if (DoubleBuffered)
                OutputDevice.SendControlChange(Channel.Channel1, (Control)0, 32 | 16 | 1);
            else
                OutputDevice.SendControlChange(Channel.Channel1, (Control)0, 32 | 16 | 4);

            _doubleBuffered = !_doubleBuffered;
        }


        public int Index { get; }

        public int Rows { get; }
        public int Columns { get; }

        public InputDevice InputDevice { get; }
        public OutputDevice OutputDevice { get; }

        public bool DoubleBuffered
        {
            get => _doubleBuffered;
            set
            {
                if (_doubleBuffered == value)
                    return;

                _doubleBuffered = value;

                if (value)
                    StartDoubleBuffering();
                else
                    EndDoubleBuffering();
            }
        }


        public IEnumerable<LaunchpadToolbarButton> ToolbarButtons => _toolbarButtons.AsEnumerable();
        public IEnumerable<LaunchpadSideButton> SideButtons => _sideButtons.AsEnumerable();
        public IEnumerable<LaunchpadGridButton> GridButtons
        {
            get
            {
                for (int x = 0; x < _gridButtons.GetLength(0); x++)
                    for (int y = 0; y < _gridButtons.GetLength(1); y++)
                        yield return _gridButtons[x, y];
            }
        }

        public IEnumerable<LaunchpadButton> Buttons
        {
            get
            {
                foreach (LaunchpadToolbarButton button in ToolbarButtons)
                    yield return button;

                foreach (LaunchpadSideButton button in SideButtons)
                    yield return button;

                foreach (LaunchpadGridButton button in GridButtons)
                    yield return button;
            }
        }
    }
}

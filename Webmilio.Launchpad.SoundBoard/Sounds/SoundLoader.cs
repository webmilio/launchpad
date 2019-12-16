using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Webmilio.Launchpad.Commons;

namespace Webmilio.Launchpad.SoundBoard.Sounds
{
    public class SoundLoader
    {
        private readonly List<SoundBind> _soundBinds;


        public SoundLoader()
        {
            _soundBinds = new List<SoundBind>();

            foreach (Type type in Assembly.GetExecutingAssembly().GetAllSubtypesOf<SoundBind>())
                _soundBinds.Add(Activator.CreateInstance(type) as SoundBind);
        }


        public void ForAll(Action<SoundBind> action)
        {
            for (int i = 0; i < _soundBinds.Count; i++)
                action(_soundBinds[i]);
        }


        public SoundBind Find(int x, int y) => _soundBinds.Find(soundBind => soundBind.X == x && soundBind.Y == y);


        public int Count => _soundBinds.Count;

        public SoundBind this[int index] => _soundBinds[index];
    }
}
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Proto.Data
{
    public class SoundClips : ScriptableObject
    {
        public List<SoundClip> Values;

        public string Find(int id)
        {
            return Values.FirstOrDefault(p => p.ID == id)?.Path;
        }
    }
}


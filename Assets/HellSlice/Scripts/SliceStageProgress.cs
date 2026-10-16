using System.Collections.Generic;
using UnityEngine;

namespace HellSlice
{
    // Run progress survives scene reloads; mutable puzzle/enemy state stays in each scene.
    public static class SliceStageProgress
    {
        static readonly HashSet<int> completed = new HashSet<int>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void NewRun() { completed.Clear(); }
        public static void Complete(int stage) { if (stage > 0) completed.Add(stage); }
        public static int CountBefore(int stage)
        {
            int count = 0;
            foreach (int number in completed) if (number < stage) count++;
            return count;
        }
    }
}

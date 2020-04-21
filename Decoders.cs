using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMFConvertionBasedOnPython
{
    public static class Decoders
    {
        public static List<KeyValuePair<int, string>> AudioFormat = new List<KeyValuePair<int, string>>()
        {
            new KeyValuePair<int, string>(0, "g729"),
            new KeyValuePair<int, string>(1, "g726"),
            new KeyValuePair<int, string>(2, "g726"),
            new KeyValuePair<int, string>(3, "alaw"),
            new KeyValuePair<int, string>(7, "mulaw"),
            new KeyValuePair<int, string>(8, "g729"),
            new KeyValuePair<int, string>(9, "g723_1"),
            new KeyValuePair<int, string>(10, "g723_1"),
            new KeyValuePair<int, string>(19, "g722")
        };
    }
}

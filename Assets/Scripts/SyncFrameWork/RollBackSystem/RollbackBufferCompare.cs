using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RollBack
{
    internal static class RollbackBufferCompare
    {
        public static bool CompareBuffers(byte[] buffer1, byte[] buffer2)
        {
            if (buffer1.Length != buffer2.Length)
                return false;
            for (int i = 0; i < buffer1.Length; i++)
            {
                if (buffer1[i] != buffer2[i])
                    return false;
            }
            return true; 
          
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RollBack
{
    public interface IAcceptsDesyncDumps
    {
        void ExportComparativeDesyncDump(byte[] lastGoodSnapshot, byte[] localSnapshot, byte[] remoteSnapshot);

        void ExportSimpleDesyncFrame(byte[] localSnapshot);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETModel
{
    public class SimulateHelper
    {
        public enum SimulateState
        {
            online = 0,
            local  = 1
        }
        public readonly static SimulateState simulateState = SimulateState.local;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Com.Game.Core
{
    public interface ITick
    {
        void OnTick();
    }
    public interface ILateTick
    {
        void OnLateTick();
    }
}

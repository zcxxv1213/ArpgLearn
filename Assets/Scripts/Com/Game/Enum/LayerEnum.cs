using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Com.Game.Enum
{
    class LayerEnum
    {
        public static readonly int DEFAULT = LayerMask.NameToLayer("Default");
        public static readonly int UI = LayerMask.NameToLayer("UI");
    }
}

using RollBack.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace ETModel
{
    public static class InputHelper
    {
        public static InputState GetInputStateByOperation()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                return InputState.Input0;
            }
            return InputState.None;
        }
        public static KeyCode GetKeyCodeByInputState(InputState state)
        {
            if (state == InputState.Input0)
            {
                return KeyCode.UpArrow;
            }
            return KeyCode.None;
        }
        public static bool CheckKeyDown(KeyCode code1,KeyCode code2)
        {
            if (code1 == code2)
            {
                return true;
            }
            return false;
        }
    }
}

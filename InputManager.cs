using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SnakeAI
{
    public static class InputManager
    {
        private static MouseState _oldM;
        private static MouseState _newM;

        private static KeyboardState _oldK;
        private static KeyboardState _newK;

        public static void Update()
        {
            _oldM = _newM;
            _oldK = _newK;

            _newM = Mouse.GetState();
            _newK = Keyboard.GetState();
        }

        public static bool KeyDown(Keys key)
        {
            return _newK.IsKeyDown(key);
        }

        public static bool KeyJustPressed(Keys key)
        {
            return !_oldK.IsKeyDown(key) && _newK.IsKeyDown(key);
        }
        
        public static bool MouseLeftJustPressed()
        {
            return _oldM.LeftButton == ButtonState.Released && _newM.LeftButton == ButtonState.Pressed;
        }
    }
}

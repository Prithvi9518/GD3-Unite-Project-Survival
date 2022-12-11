using GD.Engine;
using GD.Engine.Globals;
using Microsoft.Xna.Framework;

namespace GD.App
{
    public class RadioController : Component
    {
        private bool radioPlaced = false;

        public bool RadioPlaced
        {
            get => radioPlaced;
            set => radioPlaced = value;
        }

        public RadioController()
        {
        }
    }
}
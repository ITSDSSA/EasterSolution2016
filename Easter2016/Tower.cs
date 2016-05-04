using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Easter2016
{
    class Tower : SimpleSprite
    {
        
        int health = 100;

        public Tower(Game g, string SpriteName, Vector2 StartPosition) : base(g,SpriteName,StartPosition)
        {
            Active = true;
            AttachHealthBar(Vector2.Zero);
        }

        public int Health
        {
            get
            {
                return health;
            }

            set
            {
                health = value;
            }
        }

        public override void Update(GameTime gameTime)
        {
            if(hbar != null)
            {
                hbar.health = Health;
            }
            
        }

    }
}

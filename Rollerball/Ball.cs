using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rollerball
{
    class Ball
    {
        Color color;
        float x;
        float y;
        float xInitial;
        float yInitial;
        float xSpeed;
        float ySpeed;
        float xSpeedInitial;
        float ySpeedInitial;
        int lifetime;
        int delay;
        int sound;
        Texture2D texture;

        public Color Color
        {
            get
            {
                return color;
            }

            set
            {
                color = value;
            }
        }

        public float X
        {
            get
            {
                return x;
            }

            set
            {
                x = value;
            }
        }

        public float Y
        {
            get
            {
                return y;
            }

            set
            {
                y = value;
            }
        }

        public float XInitial
        {
            get
            {
                return xInitial;
            }

            set
            {
                xInitial = value;
            }
        }

        public float YInitial
        {
            get
            {
                return yInitial;
            }

            set
            {
                yInitial = value;
            }
        }

        public float XSpeed
        {
            get
            {
                return xSpeed;
            }

            set
            {
                xSpeed = value;
            }
        }

        public float YSpeed
        {
            get
            {
                return ySpeed;
            }

            set
            {
                ySpeed = value;
            }
        }

        public float XSpeedInitial
        {
            get
            {
                return xSpeedInitial;
            }

            set
            {
                xSpeedInitial = value;
            }
        }

        public float YSpeedInitial
        {
            get
            {
                return ySpeedInitial;
            }

            set
            {
                ySpeedInitial = value;
            }
        }

        public int Lifetime
        {
            get
            {
                return lifetime;
            }

            set
            {
                lifetime = value;
            }
        }

        public int Delay
        {
            get
            {
                return delay;
            }

            set
            {
                delay = value;
            }
        }

        public int Sound
        {
            get
            {
                return sound;
            }

            set
            {
                sound = value;
            }
        }

        public Texture2D Texture
        {
            get
            {
                return texture;
            }

            set
            {
                texture = value;
            }
        }

        public Ball(float x_, float y_, float xSpeed_, float ySpeed_, Texture2D texture_, int delay_, int sound_, Color color_)
        {
            x = xInitial = x_;
            y = yInitial = y_;
            xSpeed = xSpeedInitial = xSpeed_;
            ySpeed = ySpeedInitial = ySpeed_;
            Lifetime = 2000;
            texture = texture_;
            delay = delay_;
            sound = sound_;
            color = color_;
        }

        public void UpdateFrame()
        {
            Lifetime--;
            // TODO x = ....
        }

        public bool isAlive()
        {
            return Lifetime > 0;
        }
    }
}

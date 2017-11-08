using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rollerball
{
    class Player
    {
        int x;
        int y;
        int score;
        int moving;
        int gameTimeMilliSeconds;
        string name;
        Texture2D texture;
        Texture2D textureBall;
        Texture2D photo;
        Color color;

        public Player(string name_, int x_, int y_, Texture2D texture_, Texture2D textureBall_, Color color_)
        {
            Name = name_;
            x = x_;
            y = y_;
            texture = texture_;
            TextureBall = textureBall_;
            color = color_;
        }

        public int X
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

        public int Y
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

        public int Score
        {
            get
            {
                return score;
            }

            set
            {
                score = value;
            }
        }

        public int Moving
        {
            get
            {
                return moving;
            }

            set
            {
                moving = value;
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

        public Texture2D TextureBall
        {
            get
            {
                return textureBall;
            }

            set
            {
                textureBall = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        public int GameTimeMilliSeconds
        {
            get
            {
                return gameTimeMilliSeconds;
            }

            set
            {
                gameTimeMilliSeconds = value;
            }
        }

        public Texture2D Photo
        {
            get
            {
                return photo;
            }

            set
            {
                photo = value;
            }
        }

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
    }
}

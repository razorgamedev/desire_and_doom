﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Desire_And_Doom.Graphics;

namespace Desire_And_Doom.ECS
{
    class Animated_Sprite : Sprite
    {
        public Dictionary<string, Animation> Animations { get; private set; }

        public string Animation_ID { get; set; } = "";

        public string Current_Animation_ID { get => Animation_ID;
            set {
                if (!Force_Animation)
                    Animation_ID = value;
            } }

        public int Current_Frame { get; set; } = 0;
        public bool Animation_End { get; set; } = false;

        //public Texture2D Texture { get; private set; }
        public float Timer { get; set; } = 0;
        
        public float Flash_Timer { get; set; } = 0;
        //public Color Current_Color { get; set; }

        public bool Playing { get; set; } = true;

        public bool Force_Animation { get; set; } = false;
        public void Force_Play_Animation(string id)
        {
            Current_Animation_ID = id;
            Force_Animation = true;
            Current_Frame = 0;
        }

        public Animated_Sprite(Texture2D _texture, string start_animation) : base(_texture, new Rectangle())
        {
            Type = Types.Animation;
            Animations = new Dictionary<string, Animation>();
            Texture = _texture;

            Current_Animation_ID = start_animation;
        }

        public Animated_Sprite(Texture2D _texture, List<string> anim_ids) : base(_texture, new Rectangle())
        {
            Type = Types.Animation;

            Animations = new Dictionary<string, Animation>();
            Texture = _texture;

            if (anim_ids.Count < 1)  throw new Exception("ERROR:: no anim ids!");

            Current_Animation_ID = anim_ids[0];
            foreach (var id in anim_ids)
            {
                var anim = Assets.It.Get<Animation>(id);
                Animations.Add(anim.ID, anim);
            }
        }
    }
}

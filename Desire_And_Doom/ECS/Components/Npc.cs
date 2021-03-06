﻿using Desire_And_Doom.Graphics;
using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Desire_And_Doom.ECS
{
    class Npc : Component
    {
        public Dialog Dialog;
        public Npc(LuaTable dialog_table) : base(Types.Npc)
        {
            this.Dialog = new Dialog(dialog_table);
        }
    }
}

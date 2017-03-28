﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Desire_And_Doom.ECS.Component;
using System.Collections.Generic;
using Desire_And_Doom.Utils;
using Desire_And_Doom.ECS.Components;

namespace Desire_And_Doom.ECS
{

    class Physics_Engine : System
    {
        private World world;
        private List<RectangleF> solids;
        
        public Physics_Engine(World world) : base(Types.Body, Types.Physics)
        {
            this.world = world;
            this.solids = new List<RectangleF>();
        }

        public RectangleF Add_Solid(RectangleF solid)
        {
            solids.Add(solid);
            return solid;
        }

        public void Clear_Solids()
        {
            solids.Clear();
        }

        public override void Update(GameTime time, Entity entity)
        {
            var body = (Body)entity.Get(Types.Body);
            var physics = (Physics)entity.Get(Types.Physics);

            float dt = (float)time.ElapsedGameTime.TotalSeconds;

            var x_body = new Body(new Vector2(body.X + physics.Vel_X * dt, body.Y), body.Size);
            var y_body = new Body(new Vector2(body.X, body.Y + physics.Vel_Y * dt), body.Size);
            y_body.Z = x_body.Z = body.Z;

            if (physics.Other != null) physics.Other = null;
            if (!entity.Has(Types.Item) && !entity.Has(Types.World_Interaction))
            {
                var bodies = world.Get_All_With_Component(Types.Physics);
                foreach (var other in bodies)
                {

                    if (other.UUID == entity.UUID) continue;
                    var o_physics = (Physics)other.Get(Types.Physics);
                    var o_body = (Body)other.Get(Types.Body);

                    if (o_physics.Physics_Type == Physics.PType.DYNAMIC || o_physics.Physics_Type == Physics.PType.STATIC)
                    {

                        bool coll = false;
                        if (o_body.Contains(x_body)) { x_body = body; coll = true; }
                        if (o_body.Contains(y_body)) { y_body = body; coll = true; }

                        if (coll)
                        {
                            physics.Other = other;
                            physics.Callback?.Invoke(entity, other);
                        }

                    }else if (o_physics.Physics_Type == Physics.PType.WORLD_INTERACTION)
                    {
                        if (o_body.Contains(body))
                        {
                            var o_interaction = (World_Interaction)other.Get(Types.World_Interaction);
                            o_interaction?.Update(other, entity);
                        }
                    }
                }

                foreach(var solid in solids)
                {
                    var o_body = new Body(solid.Location, solid.Size);

                    if (o_body.Contains(x_body)) { x_body = body; }
                    if (o_body.Contains(y_body)) { y_body = body; }
                }
            }
            
            physics.Velocity *= physics.Friction;

            physics.Direction = (float)Math.Atan2(body.Y - (body.Y + physics.Vel_Y * dt),body.X - (body.X + physics.Vel_X * dt)) + Physics.Deg_To_Rad(180);

            physics.Current_Speed = Vector2.Distance(body.Position,body.Position + physics.Velocity * dt);

            body.X = x_body.X;
            body.Y = y_body.Y;

            can_draw = true;
        }

        bool can_draw = true;
        public void Draw_Solids(SpriteBatch batch, Camera_2D camera)
        {
            //solids.ForEach(s => batch.DrawRectangle(s, Color.Red, 1));
            can_draw = false;
        }

        public override void UIDraw(SpriteBatch batch,Camera_2D camera,Entity entity)
        {
            if (!Game1.DEBUG) return;
            var body = (Body)(entity.Get(Types.Body));
            //batch.DrawRectangle(body.Position, body.Size, Color.Red, 1);

            if (can_draw)
                Draw_Solids(batch, camera);
        }

    }
}

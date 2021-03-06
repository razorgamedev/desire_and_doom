﻿using Desire_And_Doom.ECS.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using NLua;
using Penumbra;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Desire_And_Doom.ECS
{
    class World
    {
        private List<Entity> entities;
        private Dictionary<Type, System> systems;
        private PenumbraComponent lighting;

        public World(PenumbraComponent lighting)
        {
            entities = new List<Entity>();
            systems = new Dictionary<Type, System>();
            this.lighting = lighting;
        }

        public Entity Find_With_Tag(string tag)
        {
            foreach(var e in entities)
                if (e.Tags.Contains(tag)) return e;
            return null;
        }

        public Entity Add(Entity e)
        {
            entities.Add(e);
            return e;
        }

        public Entity Create_Entity(LuaTable table, float x = 0, float y = 0)
        {
            var entity = Create_Entity();
            var components = table["components"] as LuaTable;
            if (components == null) {
                entity.Destroy();
                return entity;
            }

            if (table["tags"] is LuaTable tags)
                foreach (var t in tags.Values)
                    entity.Tags.Add(t as string);

            if (table["persistant"] is bool)
            {
                entity.Persistant = (bool)(table["persistant"] as bool?);
            }

            // TODO: refactor all of this into a seperate entity assembler class
            foreach (var key in components.Keys)
            {
                var component = components[key] as LuaTable;
                switch (key)
                {
                    case "Sprite": {
                            string image = component[1] as string;
                            int qx = (int)(component[2] as double?);
                            int qy = (int)(component[3] as double?);
                            int qw = (int)(component[4] as double?);
                            int qh = (int)(component[5] as double?);
                            entity.Add(new Sprite(
                                Assets.It.Get<Texture2D>(image),
                                new Rectangle(qx, qy, qw, qh)));
                            break;
                        }
                    case "Body": {
                            float w = (float)(component[1] as double?);
                            float h = (float)(component[2] as double?);
                            entity.Add(new Body(
                                new Vector2(x, y), new Vector2(w, h)
                                ));
                            break;
                        }
                    case "Physics":
                        {
                            var physics = (Physics)entity.Add(new Physics(Vector2.Zero));
                            if (component["type"] is string str)
                            {
                                switch (str)
                                {
                                    case "dynamic": physics.Physics_Type = Physics.PType.DYNAMIC; break;
                                    case "static": physics.Physics_Type = Physics.PType.STATIC; break;
                                    default:
                                        Console.WriteLine($"[WARNING]::WORLD::CREATE::ENTITY::PHYSICS:: Unknown physics type: {str}");
                                        break;
                                }
                            }

                            if ( component["blacklist"] is LuaTable btags )
                            {
                                for (int i = 1; i < btags.Values.Count + 1; i++ )
                                {
                                    var tag = btags[i] as string;
                                    physics.Blacklisted_Collision_Tags.Add(tag);
                                }
                            }

                            break;
                        }
                    case "Animation": {
                            string image = component[1] as string;

                            var anim = new List<string>();
                            for (int i = 2; i < component.Values.Count + 1; i++)
                            {
                                var t = component[i] as LuaTable;
                                anim.Add(t[1] as string);
                            }

                            entity.Add(new Animated_Sprite(
                                Assets.It.Get<Texture2D>(image), anim
                                ));
                            break;
                        }
                    case "Advanced_Animation": {
                        string image = component[1] as string;
                        var animation = new List<string>();
                        for (int i = 2; i < component.Values.Count + 1; i++){
                            var t = component[i] as LuaTable;
                            animation.Add(t[1] as string);
                        }

                        entity.Add(new Advanced_Animation_Component(
                            Assets.It.Get<Texture2D>(image),
                            animation
                        ));
                        break;
                    }
                    case "Player":
                        entity.Add(new Player());
                        break;
                    case "Equipment":
                        entity.Add(new Equipment());
                        break;
                    case "Invatory": {
                            float w = (float)(component[1] as double?);
                            float h = (float)(component[2] as double?);
                            entity.Add(new Invatory(this, entity,(int)w, (int)h));
                            break;
                        }
                    case "Item": entity.Add(new Item());  break;
                    case "Health": entity.Add(new Health(
                        (int) (component[1] as double?)
                        )); break;
                    case "Light":
                        entity.Add(new Light_Emitter(lighting));
                        break;
                    case "Ai":
                        var type = component[1] as string;
                        if (type == "Table")
                        {
                            var ai = Assets.It.Get<LuaTable>(component[2] as string);
                            entity.Add(new AI(ai));
                        }else if (type == "Function")
                        {
                            var function = component[2] as LuaFunction;
                            entity.Add(new Lua_Function(function));
                        }
                        break;
                    case "Npc":
                        {
                            // TODO(Dustin): Load the table
                            var name = (component[1] as string);
                            var dialog_table = Assets.It.Get<LuaTable>(name);

                            if (dialog_table == null) break;

                            entity.Add(new Npc(dialog_table));

                            break;
                        }
                    case "Timed_Destroy":
                        {
                            var time = (float) (component[1] as double?);
                            entity.Add(new Timed_Destroy(time));
                            break;
                        }

                    case "World_Interaction":
                        {
                            // TODO: implement
                            var world_interaction = new World_Interaction(component[1] as LuaFunction);
                            entity.Add(world_interaction);
                            break;
                        }
                    case "Enemy": {
                        List<string> drop_items = new List<string>();
                        if ( component["drops"] is LuaTable drops )
                        {
                            for (int i = 1; i < drops.Values.Count+1; i++ )
                            {
                                LuaTable dps = drops[i] as LuaTable;
                                string item_name = dps[1] as string;
                                int min = (int) (dps[2] as double?);
                                int max = (int) (dps[3] as double?);

                                float ammout = min + (new Random().Next()) % max;
                                for ( int j = 0; j < ammout; j++ )
                                    drop_items.Add(item_name);

                            }
                        }
                        var enemy = (Enemy) entity.Add(new Enemy(drop_items));
                        break;
                    }
                    case "Spawner":
                        {
                            var entities = new List<string>();
                            if (component["entities"] is LuaTable ents)
                            {
                                for (int i = 1; i < ents.Values.Count + 1; i++)
                                {
                                    LuaTable dps = ents[i] as LuaTable;
                                    string item_name = dps[1] as string;
                                    int min = (int)(dps[2] as double?);
                                    int max = (int)(dps[3] as double?);

                                    float ammout = min + (new Random().Next()) % max;
                                    for (int j = 0; j < ammout; j++)
                                        entities.Add(item_name);
                                }
                            }
                            else
                            {
                                Console.WriteLine("Spawner requires a entities table!");
                            }

                            entity.Add(new Spawner(entities, this));
                            break;
                        }
                    case "Character":
                        {
                            var ch = (Character)entity.Add(new Character());

                            if (component["name"] != null) ch.Name = component["name"] as string;
                            if (component["age"]  != null) ch.Age  = (int)(component["age"] as double?);

                            break;
                        }
                    default:
                        Console.WriteLine("Unknown Component: " + key);
                        break;
                }
            }

            return entity;
        }

        public Entity Create_Entity()
        {
            var entity = new Entity();
            entities.Add(entity);
            return entity;
        }

        public System Add_System<T>(System system) {
            systems.Add(typeof(T), system);
            system.World_Ref = this;
            return system;
        }

        public System Get_System<T>()
        {
            return systems[typeof(T)];
        }

        public void Remove_System<T>()
        {
            systems.Remove(typeof(T));
        }

        public List<Entity> Get_All_With_Tag(string tag)
        {
            List<Entity> list = new List<Entity>();

            foreach(var e in entities)
                if (e.Tags.Contains(tag)) list.Add(e);

            return list;
        }

        public List<Entity> Get_All_With_Component(Component.Types T)
        {
            List<Entity> list = new List<Entity>();

            foreach(var e in entities)
                if (e.Has(T)) list.Add(e);

            return list;
        }

        public int timing = 0;
        public void Update(GameTime time)
        {
            timing = DateTime.Now.Millisecond;
            for(int i = entities.Count - 1; i >= 0; i--) {

                if (i > entities.Count - 1) continue;

                var entity = entities[i];
                if (entity.Remove) {
                    foreach ( var system in systems.Values )
                        if ( system.Has_All_Types(entity) )
                            system.Destroy(entity);
                    entities.Remove(entity);
                    continue;
                }

                // Update the entity if the game is not paused
                if (DesireAndDoom.Game_State == DesireAndDoom.State.PLAYING)
                    entity.Update?.Invoke(entity);

                foreach (var system in systems.Values)
                    if (system.Has_All_Types(entity)) {
                        if ( !entity.Loaded )
                            system.Load(entity);
                        if (DesireAndDoom.Game_State == DesireAndDoom.State.PLAYING)
                            system.Update(time, entity);
                        system.Constant_Update(time, entity);
                    }

                entity.Loaded = true;
            }
        }

        public void Draw(SpriteBatch batch)
        {
            foreach (var entity in entities) {
                foreach (var system in systems.Values)
                    if (system.Has_All_Types(entity))
                        system.Draw(batch, entity);
            }
        }

        public void UIDraw(SpriteBatch batch, GameCamera camera)
        {
            foreach (var entity in entities)
            {
                foreach (var system in systems.Values)
                    if (system.Has_All_Types(entity))
                        system.UIDraw(batch, camera, entity);
            }

            timing = 0;
        }

        public List<Entity> Get_All_Persistant()
        {
            return entities.FindAll(e => e.Persistant == true);
        }

        public void Destroy_All(bool keep_persistant = false)
        {
            if (!keep_persistant) entities.Clear();
            else
            {
                for (var i = entities.Count - 1; i >= 0; i--)
                {
                    var ent = entities[i];
                    if (ent.Persistant == false)
                        entities.Remove(ent);
                }
            }
        }

    }
}

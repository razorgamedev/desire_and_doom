﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Desire_And_Doom.ECS.Component;

namespace Desire_And_Doom.ECS
{
    class Entity
    {

        //private Dictionary<Component.Types, Component> components;
        private Dictionary<Component.Types, Component> components;
        private static int uuid_generater = 0;

        public List<string> Tags { get; set; }

        public int UUID { get; private set; } = uuid_generater++;
        public bool Remove { get; private set; }

        public Entity()
        {
            components = new Dictionary<Types, Component>();
            Tags = new List<string>();
        }

        public void Destroy() {
            Remove = true;
        }

        public bool Has(Types name) {
            return components.ContainsKey(name);
        }

        public List<Component> Get_Components_List() {
            return components.Values.ToList();
        }

        public List<Types> Get_Component_Types_List() {
            return components.Keys.ToList();
        }

        public Component Add(Component component){
            if (components.ContainsKey(component.Type) == false){
                components.Add(component.Type, component);
            }
            return component;
        }

        public Component Get(Component.Types id)
        {
            return (components[id]);
        }
    }
}

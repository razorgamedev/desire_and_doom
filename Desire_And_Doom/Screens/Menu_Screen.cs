﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Nez.UI;
using Microsoft.Xna.Framework;
using Desire_And_Doom.Utils;
using Penumbra;
using Desire_And_Doom.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Desire_And_Doom.Screens
{
    class Menu_Screen : Screen
    {
        Screen_Manager manager;
        
        PenumbraComponent penumbra;
        GameCamera camera;

        Sky_Renderer sky;
        Texture2D rect;
        Vector2 pre_origin;
        Named_Action_List actions;
        SpriteFont font;

        int selector = 0;
        bool called = false;

        public Menu_Screen(Screen_Manager _manager, PenumbraComponent _penumbra, GameCamera _camera) : base("Menu")
        {
            camera = _camera;
            manager = _manager;
            penumbra = _penumbra;

            //new OKore_Parser();
            sky = new Sky_Renderer(Assets.It.Get<Texture2D>("sky_1"), true);
            rect = Assets.It.Get<Texture2D>("gui-rect");
            font = Assets.It.Get<SpriteFont>("gfont");
        }

        public override void Load(params string []args)
        {
            camera.Zoom = 3;
            camera.Position = Vector2.Zero;

            var cont = camera.Get_Controller();

            penumbra.AmbientColor = new Color(1f, 1f, 1f, 1f);
            pre_origin = camera.Origin;
            camera.Origin = new Vector2(0, 0);
            called = false;           

            actions = new Named_Action_List(new Dictionary<string, Action> {
                {"Start", ()=>{
                    manager.Goto_Screen("Level 1", false);
                } },
                {"Level Select", () =>
                {
                    manager.Goto_Screen("Level Select", false);
                } },
                {"Settings", ()=>{
                    Console.WriteLine("YOYOYOYO");
                } },
                {"Exit", ()=>{
                    DesireAndDoom.SHOULD_QUIT = true;
                }}
            });
        }

        public override void Update(GameTime time)
        {
            sky.Update(time);
            camera.Position = Vector2.Zero;

            var down = Input.It.Is_Key_Pressed(Keys.Down) || Input.It.Is_Gamepad_Button_Pressed(Buttons.DPadDown) || Input.It.Is_Gamepad_Button_Pressed(Buttons.LeftThumbstickDown);
            var up = Input.It.Is_Key_Pressed(Keys.Up) || Input.It.Is_Gamepad_Button_Pressed(Buttons.DPadUp) || Input.It.Is_Gamepad_Button_Pressed(Buttons.LeftThumbstickUp);

            if (down)
                selector++;
            if (up)
                selector--;

            if (selector >= actions.Names.Length) selector = 0;
            if (selector < 0) selector = actions.Names.Length - 1;

            if (Input.It.Is_Key_Pressed(Keys.Enter) || Input.It.Is_Gamepad_Button_Pressed(Buttons.A))
            {
                if (!called) actions.Call(selector);
                called = true;
            }
        }

        public override void Draw(SpriteBatch batch)
        {
            sky.Draw(batch);
            batch.Draw(
                Assets.It.Get<Texture2D>("Background"),
                new Rectangle(0, 0, DesireAndDoom.ScreenWidth, DesireAndDoom.ScreenHeight),
                Color.White
                );
        }

        public override void FilteredDraw(SpriteBatch batch)
        {

            int border_size_y = 20;
            
            var names = actions.Names;
            int index = 0;
            foreach(var name in names)
            {
                float scale = 0.3f;
                var size = font.MeasureString(name) * scale;
                float y_margin = 16f;
                float x = ((DesireAndDoom.ScreenWidth / camera.Zoom) / 2) - size.X / 2;
                float y = ((DesireAndDoom.ScreenHeight / camera.Zoom) / 2) - size.Y / 2 - ((size.Y + y_margin) * names.Length / 2) + (index++ * (size.Y + y_margin) + 16);

                batch.DrawString(
                    font,
                    name,
                    new Vector2(x, y),
                    (selector == index - 1) ? Color.Black : Color.White,
                    0f,
                    Vector2.Zero,
                    scale,
                    SpriteEffects.None,
                    1f
                    );

                if (selector == index - 1)
                {
                    batch.Draw(
                    rect,
                    new Rectangle(
                        (int)(((DesireAndDoom.ScreenWidth / camera.Zoom) / 2) / 2),
                        (int)(y),
                        (int)((DesireAndDoom.ScreenWidth / camera.Zoom) / 2),
                        (int)(size.Y * 1.2f)),
                    new Rectangle(0, 0, 512, 512),
                    Color.Orange,
                    0f,
                    Vector2.Zero,
                    SpriteEffects.None,
                    0.8f
                    );
                }
            }

            batch.Draw(
                rect,
                new Rectangle(
                    (int)(((DesireAndDoom.ScreenWidth / camera.Zoom) / 2) - (DesireAndDoom.ScreenWidth / camera.Zoom) / 4), 
                    border_size_y,
                    (int)((DesireAndDoom.ScreenWidth / camera.Zoom) / 2), 
                    (int)(DesireAndDoom.ScreenHeight / camera.Zoom) - border_size_y * 2),
                new Rectangle(0, 0, 512, 512),
                new Color(0, 0, 0, 0.8f),
                0f,
                Vector2.Zero,
                SpriteEffects.None,
                0.5f
                );
        }

        public override void Destroy()
        {
            camera.Origin = pre_origin;
            called = false;
            actions = null;
            base.Destroy();
        }
    }
}

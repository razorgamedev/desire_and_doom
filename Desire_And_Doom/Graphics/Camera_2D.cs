﻿using Desire_And_Doom.ECS;
using Desire_And_Doom.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Desire_And_Doom
{

    class Camera_2D
    {
        private readonly Camera2D camera;

        public float Zoom { get => camera.Zoom; set => camera.Zoom = value; }
        public float Rotation { get; set; }

        public Vector2 Position { get => camera.Position; }
        public Vector2 Origin { get => camera.Origin; }

        public float X { get { return Position.X; } }
        public float Y { get { return Position.Y; } }

        bool can_move = true;

        private float shake_timer = 0;
        private float shake_intensity = 10;

        public Camera_2D(GraphicsDevice device, bool _scrollable = false)
        { 
            Rotation = 0;
            
            camera = new Camera2D(device);
        }

        public void Update(GameTime time)
        {
            var rnd = new Random();
            if (shake_timer > 0 && (int)time.TotalGameTime.TotalMilliseconds % 2 == 0)
            {
                camera.Move(new Vector2(
                    (float)((-shake_intensity / 2) + rnd.NextDouble() * shake_intensity),
                    (float)((-shake_intensity / 2) + rnd.NextDouble() * shake_intensity)
                    ));
                shake_timer -= (float) time.ElapsedGameTime.TotalSeconds;
            }
        }

        public void Track(Body body, float smoothing)
        {
            
            if (!can_move) {
                can_move = true;
                return;
            }

            var dx = (X - (body.X + body.Width / 2) + Game1.WIDTH / 2);
            var dy = (Y - (body.Y + body.Height / 2) + Game1.HEIGHT / 2);

            camera.Move(new Vector2(-dx * smoothing,-dy * smoothing));


            //camera.Position = new Vector2((float)Math.Floor(camera.Position.X), (float)Math.Floor(camera.Position.Y));
            //Console.WriteLine(bounds.X);
        }

        public void Shake(float intensity, float time)
        {
            shake_timer = time;
            shake_intensity = intensity;
        }

        public BoundingFrustum Get_Camera_Frustum()
        {
            return camera.GetBoundingFrustum();
        }

        public Vector2 Left
        {
            get => camera.Position + new Vector2(Game1.WIDTH / 2, Game1.HEIGHT / 2) - new Vector2(Game1.WIDTH / 2 / Zoom, Game1.HEIGHT / 2 / Zoom);
        }

        public void Move(Vector2 by)
        {
            can_move = false;
            camera.Move(by);
        }

        public Camera2D Get_Controller() => camera;

        public Vector2 Get_Camera_Position_In_Worldspace()
        {
            return Left;
        }

        public Vector2 World_To_Screen(Vector2 point)
        {
            return camera.WorldToScreen(point);
        }

        public Vector2 Screen_To_World(Vector2 point)
        {
            return camera.ScreenToWorld(point);
        }

        public Matrix View_Matrix
        {
            get
            {
                return
                    Matrix.CreateTranslation(new Vector3(-Position, 0.0f)) *
                    Matrix.CreateTranslation(new Vector3(-Origin, 0.0f)) *
                    Matrix.CreateRotationZ(Rotation) *
                    Matrix.CreateScale(Zoom, Zoom, 1) *
                    Matrix.CreateTranslation(new Vector3(Origin, 0.0f));
            }
        }
    }
}

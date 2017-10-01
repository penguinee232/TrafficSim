﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace TrafficSim
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Random rand;

        MouseState mouse;
        MouseState oldMouse;

        KeyboardState keyboard;
        KeyboardState oldKeyboard;

        List<Car> cars = new List<Car>();

        List<Street> streets = new List<Street>();

        List<Intersection> intersections = new List<Intersection>();

        bool efficientIntersections = false;

        float scalar = 0.1f;

        public static Texture2D Pixel;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            IsMouseVisible = true;

            rand = new Random();

            graphics.PreferredBackBufferHeight = 1080;
            graphics.PreferredBackBufferWidth = 1920;
            graphics.ApplyChanges();

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Pixel = new Texture2D(GraphicsDevice, 1, 1);
            Pixel.SetData(new Color[] { Color.White });

            cars.Add(new Car(Vector2.One * 1000, 1f, 0.3f, Direction.North, 100f));
            Street s;
            s = new Street(Direction.East, 1000);
            s.Cars = new LinkedList<Car>(cars);

            streets.Add(s);
            streets.Add(new Street(Direction.West, 1500));
            streets.Add(new Street(Direction.South, 1500));
            streets.Add(new Street(Direction.North, 1000));
            intersections.Add(new Intersection(new TimeSpan(0,0,5), new Vector2(1250,1250),IntersectionDirection.NorthSouth,new Street[] { streets[0],streets[1],streets[2],streets[3] }));

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            mouse = Mouse.GetState();
            keyboard = Keyboard.GetState();
            if (keyboard.IsKeyDown(Keys.Up) && oldKeyboard.IsKeyUp(Keys.Up))
            {
                cars[0].TargetSpeed += 10;
            }
            else if (keyboard.IsKeyDown(Keys.Down) && oldKeyboard.IsKeyUp(Keys.Down))
            {
                cars[0].TargetSpeed -= 10;
            }
            if (mouse.LeftButton == ButtonState.Pressed && oldMouse.LeftButton == ButtonState.Released)
            {
                streets[0].Cars.AddFirst(new Car(Vector2.One * 1000, 0.01f, 0.2f, Direction.East, 50f, 30f, 800));
            }
            if (mouse.RightButton == ButtonState.Pressed && oldMouse.RightButton == ButtonState.Released)
            {
                streets[1].Cars.AddFirst(new Car(new Vector2(15000,1500), 0.01f, 0.2f, Direction.West, 50f, 30f, 800, 1300));
            }

            scalar = mouse.ScrollWheelValue * 0.0001f + 0.1f;

            foreach (Street street in streets)
            {
                foreach (Car car in street.Cars)
                {
                    //Car follows lane
                    car.Direction = street.Direction;

                    bool carFirst = street.Cars.Find(car).Next == null;

                    bool carInSlowDownRange = false;
                    //Brake if cars are ahead
                    if (!carFirst)
                    {
                        float distance = (float)Math.Sqrt((car.Position.X - street.Cars.Find(car).Next.Value.Position.X)* (car.Position.X - street.Cars.Find(car).Next.Value.Position.X) + (car.Position.Y - street.Cars.Find(car).Next.Value.Position.Y)* (car.Position.Y - street.Cars.Find(car).Next.Value.Position.Y));
                        if (distance < car.BrakingDistance)
                        {
                            car.TargetSpeed = Math.Max(0f, car.TargetSpeed * distance/car.BrakingDistance);
                            carInSlowDownRange = true;
                        }
                        else
                        {
                            car.TargetSpeed = car.MaxSpeed;
                            carInSlowDownRange = false;
                        }
                    }

                    foreach(Intersection inter in intersections)
                    {
                        float distance = (float)Math.Sqrt((car.Position.X - inter.Position.X) * (car.Position.X - inter.Position.X) + (car.Position.Y - inter.Position.Y) * (car.Position.Y - inter.Position.Y));

                        if(distance - 354 < car.IntersectionBrakingDistance && (int)car.Direction % 2 != (int)inter.Direction)
                        {
                            car.TargetSpeed = Math.Min(Math.Max(0f, car.TargetSpeed * (distance - 354) / car.IntersectionBrakingDistance),car.TargetSpeed);
                        }
                        else if (carFirst)
                        {
                            car.TargetSpeed = car.MaxSpeed;
                        }
                        //else if(efficientIntersections && distance <= car.IntersectionBrakingDistance)
                        //{
                        //    car.TargetSpeed = car.MaxSpeed;
                        //}

                        if (distance < 300 && (int)car.Direction % 2 == (int)inter.Direction)
                        {
                            //Make car move to random street
                            //TODO: Get last car to pass intersection for each street
                            inter.Streets[rand.Next(0, inter.Streets.Length)].Cars.AddAfter();

                        }

                    }

                    car.Update();
                }
            }

           foreach(Intersection inter in intersections)
            {
                inter.Update(gameTime.ElapsedGameTime);
            }


            oldKeyboard = keyboard;
            oldMouse = mouse;
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            foreach (Street street in streets)
            {
                foreach (Car car in street.Cars)
                {
                    spriteBatch.Draw(Pixel, new Rectangle((int)(car.Position.X * scalar), (int)(car.Position.Y * scalar), (int)((((int)car.Direction % 2 + 1)) * 50 * scalar), (int)(((((int)car.Direction + 1) % 2 + 1)) * 50 * scalar)), Color.Red);
                }
            }




            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}

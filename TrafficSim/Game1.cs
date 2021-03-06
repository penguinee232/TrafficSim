﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace TrafficSim
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        XmlDocument document;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Random rand;

        MouseState mouse;
        MouseState oldMouse;

        TimeSpan oldTotalGameTime;

        KeyboardState keyboard;
        KeyboardState oldKeyboard;

        List<Car> cars = new List<Car>();

        Texture2D santaHat;

        int crashes = 0;

        int throughput = 0;

        int timeAccelerationFactor = 120;

        TimeSpan totalGameTimeAccelerated = new TimeSpan();

        List<Street> streets = new List<Street>();

        SpriteFont font;

        List<Intersection> intersections = new List<Intersection>();

        bool efficientIntersections = false;

        float scalar = 0.1f;

        bool endAfterThisTrial = false;

        public static Texture2D Pixel;

        XmlElement root;

        List<XmlElement> trial = new List<XmlElement>();


        //In California in 2013, 156909 injury-causing accidents occurred.
        //That's ~430 PER DAY
        //~18 per hour
        //1 every 3 minutes

        float accelerationError = 0.00f;
        float decelerationError = 0.0f;
        float brakingDistanceError = 00.00f;
        float intersectionBDistanceError = 0.00f;
        int intersectionTimeMin = 10;
        int intersectionTimeMax = 30;

        float accelerationBase = 0.01f;
        float decelerationBase = 0.35f;
        float brakingDistanceBase = 800f;
        float intersectionBDistanceBase = 600f;



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
            rand = new Random();

            accelerationError = (float)rand.NextDouble() * 0.0099f;
            decelerationError = (float)rand.NextDouble() * 0.1f;
            brakingDistanceError = (float)rand.NextDouble() * 100f;
            intersectionBDistanceError = (float)rand.NextDouble() * 100f;
            intersectionTimeMin = rand.Next(5, 11);
            intersectionTimeMax = rand.Next(15, 46);


            document = new XmlDocument();
            //try
            //{
            document.Load("Data.xml");
            root = document.DocumentElement;
            //}
            //catch(Exception e)
            //{
            //    document = null;
            //    root = null;
            //}

            IsMouseVisible = true;

            santaHat = Content.Load<Texture2D>("SantaHat");

            graphics.PreferredBackBufferHeight = 1080;
            graphics.PreferredBackBufferWidth = 1920;
            graphics.ApplyChanges();

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Pixel = new Texture2D(GraphicsDevice, 1, 1);
            Pixel.SetData(new Color[] { Color.White });

            //cars.Add(new Car(Vector2.One * 1000, 1f, 0.3f, Direction.North, 100f));
            //Street s;
            //s = new Street(Direction.East, 1000);
            //s.Cars = new LinkedList<Car>(cars);

            //streets.Add(s);
            //streets.Add(new Street(Direction.West, 1500));
            //streets.Add(new Street(Direction.South, 1500));
            //streets.Add(new Street(Direction.North, 1000));
            //intersections.Add(new Intersection(new TimeSpan(0,0,5), new Vector2(1250,1250),IntersectionDirection.NorthSouth,new Street[] { streets[0],streets[1],streets[2],streets[3] }));
            int width = 5;
            List<Street> vertStreets = new List<Street>();
            List<Street> horizStreets = new List<Street>();

            for (int x = 0; x < width; x++)
            {
                Street y1 = new Street(Direction.North, x * 2000);
                Street y2 = new Street(Direction.South, x * 2000 + 500);
                vertStreets.Add(y1);
                vertStreets.Add(y2);
                Street x1 = new Street(Direction.East, x * 2000);
                Street x2 = new Street(Direction.West, x * 2000 + 500);
                horizStreets.Add(x1);
                horizStreets.Add(x2);
            }
            for (int x = 0; x < width * 2; x += 2)
            {
                for (int y = 0; y < width * 2; y += 2)
                {
                    intersections.Add(new Intersection(new TimeSpan(0, 0, rand.Next(intersectionTimeMin, intersectionTimeMax + 1)), new Vector2(vertStreets[y].Pos + 250, horizStreets[x].Pos + 250), IntersectionDirection.NorthSouth, new Street[] { horizStreets[x], horizStreets[x + 1], vertStreets[y], vertStreets[y + 1] }));
                }
            }

            streets.AddRange(vertStreets);
            streets.AddRange(horizStreets);


            foreach (Street s in streets)
            {
                //feet/pixels * pixels/update * updates/second * seconds/hour * miles/feet = miles/hour
                //15/150 * speed (pixels/update) * 60 * 3600 * 1/5280 = speed (mi/hour)

                s.Cars.AddFirst(new Car(Vector2.Zero, 0.01f, 0.2f, Direction.East, 77f / 9f, 30f, 400, 600));
            }

            font = Content.Load<SpriteFont>("font");
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

            for (int c = 0; c < timeAccelerationFactor; c++)
            {
                mouse = Mouse.GetState();
                keyboard = Keyboard.GetState();
                //if (keyboard.IsKeyDown(Keys.Up) && oldKeyboard.IsKeyUp(Keys.Up))
                //{ 

                //}
                //else if (keyboard.IsKeyDown(Keys.Space) && oldKeyboard.IsKeyUp(Keys.Space))
                //{
                //  foreach(Street s in streets)
                //    {
                //        s.Cars.AddFirst(new Car(Vector2.Zero, 0.01f, 0.2f, Direction.East, 30f, 30f, 400, 600));
                //    }
                //}
                //if (mouse.LeftButton == ButtonState.Pressed && oldMouse.LeftButton == ButtonState.Released)
                //{
                //    streets[rand.Next(0, streets.Count)].Cars.AddFirst(new Car(Vector2.Zero, 0.01f, 0.2f, Direction.East, 30f, 30f, 800, 1300));
                // //   streets[0].Cars.AddFirst(new Car(new Vector2(500,1000), 0.01f, 0.2f, Direction.East, 50f, 30f, 800));
                //}
                //if (mouse.RightButton == ButtonState.Pressed && oldMouse.RightButton == ButtonState.Released)
                //{
                // //   streets[1].Cars.AddFirst(new Car(new Vector2(15000,1500), 0.01f, 0.2f, Direction.West, 50f, 30f, 800, 1300));
                //}


                if (keyboard.IsKeyDown(Keys.Escape))
                {
                    endAfterThisTrial = true;
                }

                scalar = mouse.ScrollWheelValue * 0.0001f + 0.1f;

                foreach (Street street in streets)
                {
                    LinkedList<Car> cars = new LinkedList<Car>(street.Cars);
                    foreach (Car car in cars)
                    {
                        //Car follows lane
                        car.Direction = street.Direction;

                        if ((int)street.Direction % 2 == 0)
                        {
                            car.Position = new Vector2(street.Pos, car.Position.Y);
                        }
                        else
                        {
                            car.Position = new Vector2(car.Position.X, street.Pos);
                        }

                        bool carFirst = street.Cars.Find(car).Next == null;
                        bool carLast = street.Cars.Find(car).Previous == null;

                        bool carInSlowDownRange = false;

                        //List<float> carSpeeds = new List<float>();

                        //Brake if cars are ahead
                        if (!carFirst)
                        {
                            float distance = (float)Math.Sqrt((car.Position.X - street.Cars.Find(car).Next.Value.Position.X) * (car.Position.X - street.Cars.Find(car).Next.Value.Position.X) + (car.Position.Y - street.Cars.Find(car).Next.Value.Position.Y) * (car.Position.Y - street.Cars.Find(car).Next.Value.Position.Y));
                            if (distance < car.BrakingDistance)
                            {
                                car.TargetSpeed = Math.Max(0f, car.TargetSpeed * distance / car.BrakingDistance);
                                carInSlowDownRange = true;
                            }
                            else
                            {
                                car.TargetSpeed = car.MaxSpeed;
                                carInSlowDownRange = false;
                            }

                            if (distance < 150 && car.Acceleration != 0)
                            {
                                crashes++;
                            }
                            if (distance < 150)
                            {
                                car.Acceleration = 0;
                                car.Speed = 0;

                                street.Cars.Find(car).Next.Value.Acceleration = 0;
                                street.Cars.Find(car).Next.Value.Speed = 0;


                            }
                        }
                        if (carLast && car.Position.X * ((int)car.Direction % 2) + car.Position.Y * (((int)car.Direction + 1) % 2) > 200)
                        {
                            street.Cars.AddFirst(new Car(Vector2.Zero, accelerationBase + ErrorOf(accelerationError), decelerationBase + ErrorOf(decelerationError), Direction.East, 77f / 9f, 30f, brakingDistanceBase + ErrorOf(brakingDistanceError), intersectionBDistanceBase + ErrorOf(intersectionBDistanceError)));
                        }

                        foreach (Intersection inter in intersections)
                        {
                            float distance = car.Direction == Direction.West ? (car.Position.X - inter.Position.X) : (car.Direction == Direction.East ? (inter.Position.X - car.Position.X) : (car.Direction == Direction.South ? (inter.Position.Y - car.Position.Y) : (car.Direction == Direction.North ? (car.Position.Y - inter.Position.Y) : 0)));


                            if (distance < car.IntersectionBrakingDistance && (int)car.Direction % 2 != (int)inter.Direction && distance > 0 && Math.Abs(car.Position.X - inter.Position.X) <= car.IntersectionBrakingDistance && Math.Abs(car.Position.Y - inter.Position.Y) <= car.IntersectionBrakingDistance)
                            {
                                car.TargetSpeed = Math.Min(Math.Max(0f, car.TargetSpeed * (distance - 100) / car.IntersectionBrakingDistance), car.TargetSpeed);
                            }
                            else if ((int)car.Direction % 2 == (int)inter.Direction && distance <= car.IntersectionBrakingDistance && Math.Abs(car.Position.X - inter.Position.X) <= car.IntersectionBrakingDistance && Math.Abs(car.Position.Y - inter.Position.Y) <= car.IntersectionBrakingDistance)
                            {
                                car.TargetSpeed = Math.Min(Math.Max(car.MaxSpeed / 5f, car.TargetSpeed * (distance - 100) / car.IntersectionBrakingDistance), car.TargetSpeed);//car.MaxSpeed;
                            }
                            else if (carFirst)
                            {
                                car.TargetSpeed = car.MaxSpeed;
                            }
                            //else if(efficientIntersections && distance <= car.IntersectionBrakingDistance)
                            //{
                            //    car.TargetSpeed = car.MaxSpeed;
                            //}

                            if (distance < 300 && distance > 300 - car.Speed && (int)car.Direction % 2 == (int)inter.Direction && Math.Abs(car.Position.X - inter.Position.X) <= 300 && Math.Abs(car.Position.Y - inter.Position.Y) <= 300)
                            {
                                //Make car move to random street
                                bool turn = true;
                                if (inter.Streets.Count == 0)
                                {
                                    car.Acceleration = 0;
                                    car.Speed = 0;
                                    turn = false;
                                }
                                if (turn)
                                {
                                    int selection = rand.Next(0, inter.Streets.Count);
                                    Street selected = inter.Streets[selection];
                                    if (inter.LastCarsToPass[selected] != null && selected.Cars.Contains(inter.LastCarsToPass[selected]))
                                    {
                                        selected.Cars.AddBefore(selected.Cars.Find(inter.LastCarsToPass[selected]), car);
                                        Car inAccident = car;
                                        for (int i = 0; i < 3; i++)
                                        {
                                            inAccident = selected.Cars.Find(inAccident).Next?.Value;
                                            if (inAccident == null)
                                            { break; }
                                            if (inAccident.Acceleration == 0f)
                                            {
                                                inter.Streets.Remove(inter.Streets[selection]);
                                                break;
                                            }
                                        }
                                    }

                                    #region Teleportation Notes
                                    //Teleportation: WHY?!?!?!?!?
                                    //(Not sure) Cars only teleport when making U-turn or straight turn, don't always tp
                                    //Cars come out w/ same: NS - Y pos, EW - X pos as they went in with
                                    //Do cars come out of the same intersection, despite a weird street?
                                    //Is it just graphical?
                                    //When a car makes a turn, both streets involved go orange until you press R.
                                    //A random intersection and its streets can be turned orange w/ V.

                                    //Finally putting this to rest. The problem was that distance was only measured on one axis. This was intended, but it meant that a car could intersect every intersection at a particular y or x coordinate at one time. Fixed it! 10/28/2017
                                    #endregion

                                    else
                                    {
                                        selected.Cars.AddLast(car);
                                    }
                                    inter.LastCarsToPass[selected] = car;
                                    street.Cars.Remove(car);

                                    car.Direction = selected.Direction;

                                    if ((int)selected.Direction % 2 == 0)
                                    {
                                        car.Position = new Vector2(selected.Pos, car.Position.Y);
                                    }
                                    else
                                    {
                                        car.Position = new Vector2(car.Position.X, selected.Pos);
                                    }



                                    car.Position += new Vector2(((int)car.Direction - 2) * -300, ((int)car.Direction - 1) * 300);
                                }
                            }

                        }
                        if ((car.Position.X >= 5 * 2000 + 500 && car.Position.X <= 5 * 2000 + 900) || (car.Position.Y >= 5 * 2000 + 500 && car.Position.Y <= 5 * 2000 + 900))
                        {
                            throughput++;
                            street.Cars.Remove(car);
                            //car.Position = new Vector2(5 * 2000 + 1100, 5 * 2000 + 1100);
                        }
                        car.Update();
                    }
                }

                foreach (Intersection inter in intersections)
                {
                    inter.Update(gameTime.ElapsedGameTime);
                }

                if (oldTotalGameTime != null && oldTotalGameTime.Seconds / 30 != totalGameTimeAccelerated.Seconds / 30 && document != null)
                {
                    XmlElement element = document.CreateElement("Entry");
                    element.AppendChild(document.CreateTextNode(DateTime.Now.ToString()));
                    XmlElement accelerationE = document.CreateElement("AccelerationVariationPositive");
                    accelerationE.AppendChild(document.CreateTextNode($"{accelerationBase.ToString()} +- {accelerationError.ToString()}"));
                    XmlElement decelerationE = document.CreateElement("DecelerationVariationPositive");
                    decelerationE.AppendChild(document.CreateTextNode($"{decelerationBase.ToString()} +- {decelerationError.ToString()}"));
                    XmlElement bDistE = document.CreateElement("BrakingDistanceVariationPositive");
                    bDistE.AppendChild(document.CreateTextNode($"{brakingDistanceBase.ToString()} +- {brakingDistanceError.ToString()}"));
                    XmlElement iBDistE = document.CreateElement("IntersectionBrakingDistanceVariationPositive");
                    iBDistE.AppendChild(document.CreateTextNode($"{intersectionBDistanceBase.ToString()} +- {intersectionBDistanceError.ToString()}"));
                    XmlElement iTMin = document.CreateElement("IntersectionTimeMin");
                    iTMin.AppendChild(document.CreateTextNode(intersectionTimeMin.ToString()));
                    XmlElement iTMax = document.CreateElement("IntersectionTimeMax");
                    iTMax.AppendChild(document.CreateTextNode(intersectionTimeMax.ToString()));
                    XmlElement time = document.CreateElement("MinutesPassed");
                    time.AppendChild(document.CreateTextNode(Math.Round(totalGameTimeAccelerated.TotalMinutes, 2).ToString()));
                    XmlElement crashEl = document.CreateElement("Crashes");
                    crashEl.AppendChild(document.CreateTextNode(crashes.ToString()));
                    XmlElement throughPutEl = document.CreateElement("Throughput");
                    throughPutEl.AppendChild(document.CreateTextNode(throughput.ToString()));
                    element.AppendChild(time);
                    element.AppendChild(accelerationE);
                    element.AppendChild(decelerationE);
                    element.AppendChild(bDistE);
                    element.AppendChild(iBDistE);
                    element.AppendChild(iTMin);
                    element.AppendChild(iTMax);
                    element.AppendChild(crashEl);
                    element.AppendChild(throughPutEl);
                    root.AppendChild(element);

                    document.Save("Data.xml");
                    trial.Add(element);

                    if ((trial.Count > 3 && trial[trial.Count - 3].ChildNodes[8].InnerText == trial[trial.Count - 2].ChildNodes[8].InnerText && trial[trial.Count - 3].ChildNodes[8].InnerText == trial[trial.Count - 1].ChildNodes[8].InnerText && trial[trial.Count - 3].ChildNodes[9].InnerText == trial[trial.Count - 2].ChildNodes[9].InnerText && trial[trial.Count - 3].ChildNodes[9].InnerText == trial[trial.Count - 1].ChildNodes[9].InnerText) || totalGameTimeAccelerated.TotalMinutes >= 20f)
                    {
                        if (!endAfterThisTrial)
                        {
                            Program.Restart();
                        }
                        Exit();
                    }
                }
                oldKeyboard = keyboard;
                oldMouse = mouse;
                oldTotalGameTime = totalGameTimeAccelerated;
                totalGameTimeAccelerated += gameTime.ElapsedGameTime;
                base.Update(gameTime);
            }
        }

        float ErrorOf(float plusOrMinus)
        {
            float output = (float)rand.NextDouble() * plusOrMinus * rand.Next(-1, 2);
            return output;
        }

        int ErrorOf(int plusOrMinus)
        {
            return rand.Next(-plusOrMinus, plusOrMinus + 1);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //Color clearColor = intersections[0].Direction == IntersectionDirection.NorthSouth ? Color.Green : Color.CornflowerBlue;
            GraphicsDevice.Clear(Color.CornflowerBlue);//clearColor);

            spriteBatch.Begin();

            foreach (Street street in streets)
            {
                Color c = street.Pos % 10 == 0 ? Color.Black : Color.Orange;
                spriteBatch.Draw(Pixel, new Rectangle((((int)street.Direction + 1) % 2) * (int)(street.Pos * scalar), (((int)street.Direction) % 2) * (int)(street.Pos * scalar), (int)(((((int)street.Direction) % 2) * 1000000 + 50) * scalar), (int)(((((int)street.Direction + 1) % 2) * 1000000 + 50) * scalar)), c);
            }

            foreach (Street street in streets)
            {

                foreach (Car car in street.Cars)
                {
                    Color color = car.Acceleration > 0 ? Color.White : Color.Red;
                    spriteBatch.Draw(Pixel, new Rectangle((int)(car.Position.X * scalar), (int)(car.Position.Y * scalar), (int)((((int)car.Direction % 2 + 1)) * 50 * scalar), (int)(((((int)car.Direction + 1) % 2 + 1)) * 50 * scalar)), color);
                    if (DateTime.Today.Day >= 23 && DateTime.Today.Day <= 25 && DateTime.Today.Month == 12)
                    {
                        spriteBatch.Draw(santaHat, new Rectangle((int)(car.Position.X * scalar), (int)((car.Position.Y - 150) * scalar), (int)(santaHat.Width * 0.5f * scalar), (int)(santaHat.Height * 0.5f * scalar)), Color.White);
                    }
                }
            }

            foreach (Intersection inter in intersections)
            {
                spriteBatch.Draw(Pixel, new Rectangle((inter.Position.X * scalar).ToInt(), (inter.Position.Y * scalar).ToInt(), 5, 5), inter.Direction == IntersectionDirection.NorthSouth ? Color.Green : Color.Red);
            }

            spriteBatch.DrawString(font, totalGameTimeAccelerated.ToString() + $"\nThroughput: {throughput}\nCrashes: {crashes}", Vector2.Zero, Color.Red);

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}

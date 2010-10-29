﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

//Klassen unserer eigenen Engine
using Silhouette.Engine;

//Partikel-Engine Klassen
using Silhouette.Engine.PartikelEngine;
using ProjectMercury;
using ProjectMercury.Emitters;
using ProjectMercury.Modifiers;
using ProjectMercury.Renderers;

//Physik-Engine Klassen
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision;

namespace Silhouette
{
    public class GameLoop : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        DisplayFPS displayFPS;
        GameMechs.Player testPlayer = new GameMechs.Player();

        public GameLoop()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            displayFPS = new DisplayFPS(this);
            Components.Add(displayFPS);
        }

        protected override void Initialize()
        {
            //Voreinstellungen gemäß der Spezifikationen
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 768;
            graphics.ApplyChanges();
            testPlayer.Initialise();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            testPlayer.LoadContent(this);
            //Lädt alle Fonts, die im FontManager deklariert wurden
            FontManager.loadFonts(this);
        }

        protected override void UnloadContent(){}

        protected override void Update(GameTime gameTime)
        {
            testPlayer.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);  //Hintergrundfarbe Schwarz
            spriteBatch.Begin();
            testPlayer.Draw(spriteBatch);
            spriteBatch.End();



            base.Draw(gameTime);
        }
    }
}
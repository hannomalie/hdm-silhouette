﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Silhouette.GameMechs;
using System.IO;
using System.Collections;
using Silhouette.Engine.Effects;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace Silhouette.Engine.Manager
{
    // HANNES: Erste Version des EffectManagers. Selbsterklärend.
    // Achja: "This software contains source code provided by NVIDIA Corporation." nicht vergessen :D

    public static class EffectManager
    {
        public static Dictionary<Effects, EffectObject> AllEffects;
        public enum Effects
        { 
            Blur
        }

        public static EffectObject GetEffectObject(Effects type) 
        {
            if(type == Effects.Blur)
            {
                Blur blur = new Blur();
                blur.Initialise();
                blur.LoadContent();
                return blur;
            }

            return null;
        }

        private static Texture2D vignette;
        private static Texture2D clouds;
        private static Texture2D noise;

        private static Effect blender;
        private static Effect blurrer;
        private static Effect weakBlurrer;
        private static Effect strongBlurrer;
        private static Effect bleachBlur;
        private static Effect bleach;
        private static Effect weakBleach;
        private static Effect strongBleach;
        private static Effect bloomer;
        private static Effect vignettenBlur;
        private static Effect water;
        private static Effect colorChange;
        private static bool overallBlur;
        private static bool overallVignette;
        private static Effect godrays;

        public static GameTime gameTime;

        public static void loadEffects()
        {
            AllEffects = new Dictionary<Effects, EffectObject>();
            EffectObject blur = new Blur();
            blur.Initialise();
            blur.LoadContent();
            AllEffects.Add(Effects.Blur, blur);

            overallBlur = true;
            overallVignette = true;

            vignette = GameLoop.gameInstance.Content.Load<Texture2D>("Sprites/Overlays/Vignette");
            GameLoop.gameInstance.GraphicsDevice.Textures[1] = vignette;

            clouds = GameLoop.gameInstance.Content.Load<Texture2D>("Sprites/Overlays/clouds");
            GameLoop.gameInstance.GraphicsDevice.Textures[2] = clouds;

            noise = GameLoop.gameInstance.Content.Load<Texture2D>("Sprites/Overlays/noise");
            GameLoop.gameInstance.GraphicsDevice.Textures[3] = noise;

            blender = GameLoop.gameInstance.Content.Load<Effect>("Effects/blender");

            colorChange = GameLoop.gameInstance.Content.Load<Effect>("Effects/ColorChange");

            blurrer = GameLoop.gameInstance.Content.Load<Effect>("Effects/blurrer");
            weakBlurrer = GameLoop.gameInstance.Content.Load<Effect>("Effects/blurrer");
            strongBlurrer = GameLoop.gameInstance.Content.Load<Effect>("Effects/blurrer");

            bleachBlur = GameLoop.gameInstance.Content.Load<Effect>("Effects/BleachBlur");
            bleachBlur.Parameters["BlurDistance"].SetValue(0.002f);

            bleach = GameLoop.gameInstance.Content.Load<Effect>("Effects/bleach");
            weakBleach = GameLoop.gameInstance.Content.Load<Effect>("Effects/bleach");
            strongBleach = GameLoop.gameInstance.Content.Load<Effect>("Effects/bleach");

            bloomer = GameLoop.gameInstance.Content.Load<Effect>("Effects/Bloom");
            vignettenBlur = GameLoop.gameInstance.Content.Load<Effect>("Effects/VignettenBlur");
            water = GameLoop.gameInstance.Content.Load<Effect>("Effects/Water");

            godrays = GameLoop.gameInstance.Content.Load<Effect>("Effects/godrays");
        }
        public static void loadEffectsInEditor(GraphicsDevice graphics, ContentManager content)
        {
            AllEffects = new Dictionary<Effects, EffectObject>();
            EffectObject blur = new Blur();
            blur.Initialise();
            blur.loadContentInEditor(graphics, content);
            AllEffects.Add(Effects.Blur, blur);

            overallBlur = true;
            overallVignette = true;

            vignette = content.Load<Texture2D>("Sprites/Overlays/Vignette");
            graphics.Textures[1] = vignette;

            clouds = content.Load<Texture2D>("Sprites/Overlays/clouds");
            graphics.Textures[2] = clouds;

            noise = content.Load<Texture2D>("Sprites/Overlays/noise");
            graphics.Textures[3] = noise;

            blender = content.Load<Effect>("Effects/blender");

            colorChange = content.Load<Effect>("Effects/ColorChange");

            blurrer = content.Load<Effect>("Effects/blurrer");
            weakBlurrer = content.Load<Effect>("Effects/blurrer");
            strongBlurrer = content.Load<Effect>("Effects/blurrer");

            bleachBlur = content.Load<Effect>("Effects/BleachBlur");
            bleachBlur.Parameters["BlurDistance"].SetValue(0.002f);

            bleach = content.Load<Effect>("Effects/bleach");
            weakBleach = content.Load<Effect>("Effects/bleach");
            strongBleach = content.Load<Effect>("Effects/bleach");

            bloomer = content.Load<Effect>("Effects/Bloom");
            vignettenBlur = content.Load<Effect>("Effects/VignettenBlur");
            water = content.Load<Effect>("Effects/Water");

            godrays = content.Load<Effect>("Effects/godrays");
        }

        public static Effect Blender()
        {
            return blender;
        }

        public static Effect Blurrer()
        {
            blurrer.Parameters["BlurDistance"].SetValue(0.002f);
            AllEffects[Effects.Blur].Type = "Normal";
            return AllEffects[Effects.Blur].Effect;
            return blurrer;
        }

        public static Effect WeakBlurrer()
        {
            weakBlurrer.Parameters["BlurDistance"].SetValue(0.001f);
            AllEffects[Effects.Blur].Type = "Weak";
            return AllEffects[Effects.Blur].Effect;
            return weakBlurrer;
        }

        public static Effect StrongBlurrer()
        {
            strongBlurrer.Parameters["BlurDistance"].SetValue(0.006f);
            AllEffects[Effects.Blur].Type = "Strong";
            return AllEffects[Effects.Blur].Effect;
            return strongBlurrer;
        }

        public static Effect Bleach()
        {
            //Player player = null;
            Tom player = null;
            try
            {
                player = GameLoop.gameInstance.playerInstance;
                //Tom player = GameLoop.gameInstance.playerInstance;
            }
            catch (Exception e)
            {

            }

            float fadeOrange = 0;
            float fadeBlue = 0;



            bleach.Parameters["fadeOrange"].SetValue(fadeOrange);
            bleach.Parameters["fadeBlue"].SetValue(fadeBlue);
            bleach.Parameters["amount"].SetValue(0.5f);

            return bleach;
        }

        public static Effect WeakBleach()
        {
            //Player player = null;
            Tom player = null;
            try
            {
                player = GameLoop.gameInstance.playerInstance;
                //Tom player = GameLoop.gameInstance.playerInstance;
            }
            catch (Exception e)
            {

            }

            float fadeOrange = 0;
            float fadeBlue = 0;


            bleach.Parameters["fadeOrange"].SetValue(fadeOrange);
            bleach.Parameters["fadeBlue"].SetValue(fadeBlue);
            bleach.Parameters["amount"].SetValue(0.3f);

            return weakBleach;
        }

        public static Effect StrongBleach()
        {
            //Player player = null;
            Tom player = null;
            try
            {
                player = GameLoop.gameInstance.playerInstance;
                //Tom player = GameLoop.gameInstance.playerInstance;
            }
            catch (Exception e)
            {

            }
            float fadeOrange = 0;
            float fadeBlue = 0;


            bleach.Parameters["fadeOrange"].SetValue(fadeOrange);
            bleach.Parameters["fadeBlue"].SetValue(fadeBlue);
            bleach.Parameters["amount"].SetValue(0.6f);

            return weakBleach;
        }

        public static Effect BleachBlur()
        {
            bleachBlur.Parameters["BlurDistance"].SetValue(0.003f);
            return bleachBlur;
        }

        public static Effect Bloom()
        {
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, GameLoop.gameInstance.GraphicsDevice.Viewport.Width, GameLoop.gameInstance.GraphicsDevice.Viewport.Height, 0, 0, 1);
            Matrix halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);

            bloomer.Parameters["MatrixTransform"].SetValue(halfPixelOffset * projection);

            return bloomer;
        }
        public static Effect BloomInEditor(GraphicsDevice graphics)
        {
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, graphics.Viewport.Width, graphics.Viewport.Height, 0, 0, 1);
            Matrix halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);

            bloomer.Parameters["MatrixTransform"].SetValue(halfPixelOffset * projection);

            return bloomer;
        }

        public static Effect Godrays()
        {
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, GameLoop.gameInstance.GraphicsDevice.Viewport.Width, GameLoop.gameInstance.GraphicsDevice.Viewport.Height, 0, 0, 1);
            Matrix halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);

            godrays.Parameters["MatrixTransform"].SetValue(halfPixelOffset * projection);


            // For the rays to change strength slightly (simulating cloud movement)
            double temp = 0.001;
            double noiseMove = 0.5f;
            double lightPosX = 0.5f;
            if (gameTime != null)
            {
                temp = Math.Sin(0.0015f * gameTime.TotalGameTime.TotalMilliseconds) * 0.01 * (new Random().Next(95, 105) * 0.01f);
                noiseMove += Math.Sin(0.000005f * gameTime.TotalGameTime.TotalMilliseconds);
                lightPosX = 10 * Math.Sin(gameTime.TotalGameTime.TotalMilliseconds);
            }

            godrays.Parameters["Exposure"].SetValue(0.04515f + (float)(temp));
            godrays.Parameters["NoiseMove"].SetValue((float)noiseMove);
            godrays.Parameters["LightPositionX"].SetValue((float)lightPosX);

            return godrays;
        }

        public static Effect GodraysInEditor(GraphicsDevice graphics)
        {
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, graphics.Viewport.Width, graphics.Viewport.Height, 0, 0, 1);
            Matrix halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);

            godrays.Parameters["MatrixTransform"].SetValue(halfPixelOffset * projection);


            // For the rays to change strength slightly (simulating cloud movement)
            double temp = 0.001;
            double noiseMove = 0.5f;
            double lightPosX = 0.5f;
            if (gameTime != null)
            {
                temp = Math.Sin(0.0015f * gameTime.TotalGameTime.TotalMilliseconds) * 0.01 * (new Random().Next(95, 105) * 0.01f);
                noiseMove += Math.Sin(0.000005f * gameTime.TotalGameTime.TotalMilliseconds);
                lightPosX = 10 * Math.Sin(gameTime.TotalGameTime.TotalMilliseconds);
            }

            godrays.Parameters["Exposure"].SetValue(0.04515f + (float)(temp));
            godrays.Parameters["NoiseMove"].SetValue((float)noiseMove);
            godrays.Parameters["LightPositionX"].SetValue((float)lightPosX);

            return godrays;
        }

        public static Effect Water()
        {
            if (gameTime != null)
            {
                float temp = (float)Math.Sin((float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f);
                //Console.WriteLine(temp);
                water.Parameters["Time"].SetValue(temp);
            }
            else
            {
                water.Parameters["Time"].SetValue(0);
            }
            return water;
        }

        public static Effect VignettenBlur()
        {
            vignettenBlur.Parameters["bBlur"].SetValue(overallBlur);
            vignettenBlur.Parameters["bVignette"].SetValue(overallVignette);
            return vignettenBlur;
        }

        public static Effect ColorChange()
        {
            //Player player = null;
            Tom player = null;
            try
            {
                player = GameLoop.gameInstance.playerInstance;
                //Tom player = GameLoop.gameInstance.playerInstance;
            }
            catch (Exception e)
            {

            }
            float fadeOrange = 0;
            float fadeBlue = 0;


            float orangeTargetRed = 0f;
            float orangeTargetGreen = -0.32f;
            float orangeTargetBlue = -0.45f;
            float blueTargetRed = -0.47f;
            float blueTargetGreen = -0.41f;
            float blueTargetBlue = -0.31f;

            {
                {
                    colorChange.Parameters["bla"].SetValue(true);
                    colorChange.Parameters["alpha"].SetValue(0);
                    colorChange.Parameters["targetRed"].SetValue(fadeOrange * orangeTargetRed + fadeBlue * blueTargetRed);
                    colorChange.Parameters["targetGreen"].SetValue(fadeOrange * orangeTargetGreen + fadeBlue * blueTargetGreen);
                    colorChange.Parameters["targetBlue"].SetValue(fadeOrange * orangeTargetBlue + fadeBlue * blueTargetBlue);
                }
            }
            return colorChange;
        }

        public static Effect BlurrerInEditor(GraphicsDevice graphics)
        {
            blurrer.Parameters["BlurDistance"].SetValue(0.002f);
            AllEffects[Effects.Blur].Type = "Normal";
            return AllEffects[Effects.Blur].EffectInEditor(graphics);
            return blurrer;
        }

        public static Effect WeakBlurrerInEditor(GraphicsDevice graphics)
        {
            weakBlurrer.Parameters["BlurDistance"].SetValue(0.001f);
            AllEffects[Effects.Blur].Type = "Weak";
            return AllEffects[Effects.Blur].EffectInEditor(graphics);
            return weakBlurrer;
        }

        public static Effect StrongBlurrerInEditor(GraphicsDevice graphics)
        {
            strongBlurrer.Parameters["BlurDistance"].SetValue(0.006f);
            AllEffects[Effects.Blur].Type = "Strong";
            return AllEffects[Effects.Blur].EffectInEditor(graphics);
            return strongBlurrer;
        }

        public static Effect BleachInEditor(GraphicsDevice graphics)
        {
            //Player player = null;
            Tom player = null;
            try
            {
                player = GameLoop.gameInstance.playerInstance;
                //Tom player = GameLoop.gameInstance.playerInstance;
            }
            catch (Exception e)
            {

            }

            float fadeOrange = 0;
            float fadeBlue = 0;



            bleach.Parameters["fadeOrange"].SetValue(fadeOrange);
            bleach.Parameters["fadeBlue"].SetValue(fadeBlue);
            bleach.Parameters["amount"].SetValue(0.5f);

            return bleach;
        }

        public static Effect WeakBleachInEditor(GraphicsDevice graphics)
        {
            //Player player = null;
            Tom player = null;
            try
            {
                player = GameLoop.gameInstance.playerInstance;
                //Tom player = GameLoop.gameInstance.playerInstance;
            }
            catch (Exception e)
            {

            }

            float fadeOrange = 0;
            float fadeBlue = 0;


            bleach.Parameters["fadeOrange"].SetValue(fadeOrange);
            bleach.Parameters["fadeBlue"].SetValue(fadeBlue);
            bleach.Parameters["amount"].SetValue(0.3f);

            return weakBleach;
        }

        public static Effect StrongBleachInEditor(GraphicsDevice graphics)
        {
            //Player player = null;
            Tom player = null;
            try
            {
                player = GameLoop.gameInstance.playerInstance;
                //Tom player = GameLoop.gameInstance.playerInstance;
            }
            catch (Exception e)
            {

            }
            float fadeOrange = 0;
            float fadeBlue = 0;



            bleach.Parameters["fadeOrange"].SetValue(fadeOrange);
            bleach.Parameters["fadeBlue"].SetValue(fadeBlue);
            bleach.Parameters["amount"].SetValue(0.6f);

            return weakBleach;
        }

        public static Effect BleachBlurInEditor(GraphicsDevice graphics)
        {
            bleachBlur.Parameters["BlurDistance"].SetValue(0.003f);
            return bleachBlur;
        }


        public static Effect WaterInEditor(GraphicsDevice graphics)
        {
            if (gameTime != null)
            {
                float temp = (float)Math.Sin((float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f);
                //Console.WriteLine(temp);
                water.Parameters["Time"].SetValue(temp);
            }
            else
            {
                water.Parameters["Time"].SetValue(0);
            }
            return water;
        }

        
        

        public static void setOverallBlur(bool b)
        {
            overallBlur = b;
        }

        public static void setVignette(bool b)
        {
            overallVignette = b;
        }

        public static void Update(GameTime gameTime) {
            EffectManager.gameTime = gameTime;
        }


        /*public Effect LoadFromFile(string filename, GraphicsDevice graphics)
        {
            Effect effect = new Effect(graphics, byte[0];
            try
            {
                FileStream file = FileManager.LoadConfigFile(filename);
                if (file != null)
                {
                        
                    file.Close();
                }
                else
                    return null;
            }
            catch (IOException e)
            {
                return null;
            }
            return effect;
        }*/

    }
}

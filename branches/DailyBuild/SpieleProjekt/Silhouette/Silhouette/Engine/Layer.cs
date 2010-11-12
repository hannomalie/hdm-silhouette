﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Serialization;
using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using Silhouette.GameMechs;

namespace Silhouette.Engine.Screens
{
    public class Layer
    {
        [XmlAttribute()]
        string name;
        [XmlAttribute()]
        bool isVisible;

        public Vector2 scrollSpeed;

        List<LevelObject> loList;
        List<DrawableLevelObject> dloList;

        Texture2D[,] layerTexture;
        string[] assetName;
        int width, height, amount;

        public Layer()
        {
            loList = new List<LevelObject>();
            dloList = new List<DrawableLevelObject>();
        }

        public void initialiseLayer(string name, Vector2 scrollSpeed, bool isVisible)
        {
            this.name = name;
            this.scrollSpeed = scrollSpeed;
            this.isVisible = isVisible;
            layerTexture = new Texture2D[width, height];
            assetName = new string[amount];
        }

        public void loadLayer()
        {
            foreach (LevelObject lo in loList)
            {
                lo.LoadContent();
            }
            foreach (DrawableLevelObject dlo in dloList)
            {
                dlo.LoadContent();
            }
        }

        public void updateLayer(GameTime gameTime)
        {
            foreach (LevelObject lo in loList)
            {
                lo.Update(gameTime);
            }
            foreach (DrawableLevelObject dlo in dloList)
            {
                dlo.Update(gameTime);
            }
        }

        public void drawLayer(SpriteBatch spriteBatch)
        {
            if (!isVisible)
                return;

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    spriteBatch.Draw(layerTexture[x, y], new Vector2(x * GameSettings.Default.resolutionWidth, y * GameSettings.Default.resolutionHeight), Color.White);
                }
            foreach (DrawableLevelObject dlo in dloList)
            {
                dlo.Draw(spriteBatch);
            }
        }
    }
}
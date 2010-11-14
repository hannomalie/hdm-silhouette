﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using Silhouette.Engine.Manager;
using Silhouette.Engine.Screens;

//Physik-Engine Klassen
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision;

namespace Silhouette.Engine
{
    public partial class CollisionLayer
    {
        public void AddFixture(Fixture fixture)
        {
            fixtureList.Add(fixture);
        }
        public void DeleteFixture(Fixture fixture)
        {
            fixtureList.Remove(fixture);
        }
        public void DeleteFixture(int index)
        {
            fixtureList.RemoveAt(index);
        }
    }
}

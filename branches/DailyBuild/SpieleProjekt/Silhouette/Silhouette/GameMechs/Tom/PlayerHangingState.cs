﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using Silhouette.Engine;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Controllers;
using FarseerPhysics.Collision;

using Silhouette.GameMechs.Move;

namespace Silhouette.GameMechs
{
    public class PlayerHangingState : PlayerState
    {
        private MoveStack moves = new MoveStack();

        public PlayerHangingState(Tom tom, Animation animationLeft, Animation animationRight) 
            : base(tom, animationLeft, animationRight)
        {
        }

        public override void Update(GameTime gt)
        {
            if (moves.Finished())
            {
                tom.State = tom.HangingState2;
            }
            base.Update(gt);
            moves.Update(gt);
        }

        public override void onUnset()
        {
            tom.CharFix.Body.BodyType = BodyType.Dynamic;
        }

        public override void onSet(Tom.FacingState facingState)
        {
            base.onSet(facingState);
            tom.CharFix.Body.BodyType = BodyType.Kinematic;
            tom.CharFix.Body.LinearVelocity = Vector2.Zero;
            tom.CharFix.Body.AngularVelocity = 0;
            animationLeft.activeFrameNumber = 0;
            animationRight.activeFrameNumber = 0;
            moves.push(new Move.Move(tom, new Vector2(0,-300), 3000));
        }

    }
}

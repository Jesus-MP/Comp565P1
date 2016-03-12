/*  
    Copyright (C) 2016 G. Michael Barnes
 
    The file Player.cs is part of AGMGSKv7 a port and update of AGXNASKv6 from
    MonoGames 3.2 to MonoGames 3.4  

    AGMGSKv7 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/


#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//#if MONOGAMES //  true, build for MonoGames
//   using Microsoft.Xna.Framework.Storage; 
//#endif
#endregion

namespace Project1 {

/// <summary>
/// Represents the user / player interacting with the stage. 
/// The Update(Gametime) handles both user keyboard and gamepad controller input.
/// If there is a gamepad attached the keyboard inputs are not processed.
/// 
/// removed game controller code from Update()
/// 
/// 2/8/2014 last changed
/// </summary>

public class Player : Agent {
   private KeyboardState oldKeyboardState;  
   private int rotate;
   private float angle;
   private Matrix initialOrientation;
   private List<Treasure> treasure;
   private List<Treasure> marked;
   public int treasureCount;

   public Player(Stage theStage, string label, Vector3 pos, Vector3 orientAxis, 
   float radians, string meshFile, List<Treasure> t, List<Treasure> m)
   : base(theStage, label, pos, orientAxis, radians, meshFile)
      {  // change names for on-screen display of current camera
      first.Name =  "First";
      follow.Name = "Follow";
      above.Name =  "Above";
      IsCollidable = true;  // players test collision with Collidable set.
      stage.Collidable.Add(agentObject);  // player's agentObject can be collided with by others.
      rotate = 0;
      angle = 0.01f;
      initialOrientation = agentObject.Orientation;
      treasure = t;
      marked = m;
      treasureCount = 0;
      }

   /// <summary>
   /// Handle player input that affects the player.
   /// See Stage.Update(...) for handling user input that affects
   /// how the stage is rendered.
   /// First check if gamepad is connected, if true use gamepad
   /// otherwise assume and use keyboard.
   /// </summary>
   /// <param name="gameTime"></param>
   public override void Update(GameTime gameTime) {
       int target = findTreasure(agentObject, treasure);
       float distance = Vector3.Distance(
                    treasure[target].translation,
                    new Vector3(agentObject.Translation.X, 0, agentObject.Translation.Z));
       if (distance <= 300 && treasure[target].Visible == true)
       {
           // snap to nextGoal and orient toward the new nextGoal 
           //nextGoal = path.NextNode;
           treasure[target].tagged = true;
           treasure[target].Visible = false;
           marked[target].Visible = true;
           treasureCount++;
           // agentObject.turnToFace(nextGoal.Translation);
       }

      KeyboardState keyboardState = Keyboard.GetState();
      if (keyboardState.IsKeyDown(Keys.R) && !oldKeyboardState.IsKeyDown(Keys.R)) 
         agentObject.Orientation = initialOrientation; 
      // allow more than one keyboardState to be pressed
      if (keyboardState.IsKeyDown(Keys.Up)) agentObject.Step++;
      if (keyboardState.IsKeyDown(Keys.Down)) agentObject.Step--; 
      if (keyboardState.IsKeyDown(Keys.Left)) rotate++;
      if (keyboardState.IsKeyDown(Keys.Right)) rotate--;
      oldKeyboardState = keyboardState;    // Update saved state.
      agentObject.Yaw = rotate * angle;
      base.Update(gameTime);
      rotate = agentObject.Step = 0;
      }

   private int findTreasure(Object3D agentObject, List<Treasure> treasure)
   {
       float minDistance = 100000000;
       int targetTreasure = 0;
       for (int i = 0; i < treasure.Count; i++)
       {
           if (!treasure[i].tagged) //don't check distance for tagged treasures
           {
               float distance = Vector3.Distance(
                        treasure[i].translation,
                        new Vector3(agentObject.Translation.X, 0, agentObject.Translation.Z));
               if (distance < minDistance)
               {
                   minDistance = distance;
                   targetTreasure = i;
               }
           }
       }
       return targetTreasure;
   }
   }
}

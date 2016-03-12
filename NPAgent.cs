/*  
    Copyright (C) 2016 G. Michael Barnes
 
    The file NPAgent.cs is part of AGMGSKv7 a port and update of AGXNASKv6 from
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
/// A non-playing character that moves.  Override the inherited Update(GameTime)
/// to implement a movement (strategy?) algorithm.
/// Distribution NPAgent moves along an "exploration" path that is created by the
/// from int[,] pathNode array.  The exploration path is traversed in a reverse path loop.
/// Paths can also be specified in text files of Vector3 values, see alternate
/// Path class constructors.
/// 
/// 1/20/2016 last changed
/// </summary>
public class NPAgent : Agent {
    public enum UpdateState { PATH_FOLLOWING, TREASURE_GOAL};
    public UpdateState currentState = UpdateState.PATH_FOLLOWING;
    private KeyboardState oldKeyboardState; 
   private NavNode nextGoal;
   private Path path;
   private int snapDistance = 20;  // this should be a function of step and stepSize
	// If using makePath(int[,]) set WayPoint (x, z) vertex positions in the following array
	private int[,] pathNode = { {505, 490}, {500, 500}, {490, 505},  // bottom, right
										 {435, 505}, {425, 500}, {420, 490},  // bottom, middle
										 {420, 450}, {425, 440}, {435, 435},  // middle, middle
                               {490, 435}, {500, 430}, {505, 420},  // middle, right
										 {505, 105}, {500,  95}, {490,  90},  // top, right
                               {110,  90}, {100,  95}, { 95, 105},  // top, left
										 { 95, 480}, {100, 490}, {110, 495},  // bottom, left
										 {495, 480} };								  // loop return

   /// <summary>
   /// Create a NPC. 
   /// AGXNASK distribution has npAgent move following a Path.
   /// </summary>
   /// <param name="theStage"> the world</param>
   /// <param name="label"> name of </param>
   /// <param name="pos"> initial position </param>
   /// <param name="orientAxis"> initial rotation axis</param>
   /// <param name="radians"> initial rotation</param>
   /// <param name="meshFile"> Direct X *.x Model in Contents directory </param>
   public NPAgent(Stage theStage, string label, Vector3 pos, Vector3 orientAxis, 
      float radians, string meshFile)
      : base(theStage, label, pos, orientAxis, radians, meshFile)
      {  // change names for on-screen display of current camera
      first.Name =  "npFirst";
      follow.Name = "npFollow";
      above.Name =  "npAbove";
      // path is built to work on specific terrain, make from int[x,z] array pathNode
      path = new Path(stage, pathNode, Path.PathType.LOOP); // continuous search path
      stage.Components.Add(path);
      nextGoal = path.NextNode;  // get first path goal
      agentObject.turnToFace(nextGoal.Translation);  // orient towards the first path goal
		// set snapDistance to be a little larger than step * stepSize
		snapDistance = (int) (1.5 * (agentObject.Step * agentObject.StepSize));
      }   

   /// <summary>
   /// Simple path following.  If within "snap distance" of a the nextGoal (a NavNode) 
   /// move to the NavNode, get a new nextGoal, turnToFace() that goal.  Otherwise 
   /// continue making steps towards the nextGoal.
   /// </summary>
   public override void Update(GameTime gameTime) {
       KeyboardState keyboardState = Keyboard.GetState();
       if ((keyboardState.IsKeyDown(Keys.N) && !oldKeyboardState.IsKeyDown(Keys.N)) && currentState == UpdateState.PATH_FOLLOWING)
          currentState = UpdateState.TREASURE_GOAL;  // toggle NPAgent update state.
       float distance;
       switch(currentState)
       { 
           case UpdateState.PATH_FOLLOWING :
               agentObject.turnToFace(nextGoal.Translation);  // adjust to face nextGoal every move
		        // See if at or close to nextGoal, distance measured in 2D xz plane
		        distance = Vector3.Distance(
			        new Vector3(nextGoal.Translation.X, 0, nextGoal.Translation.Z),
			        new Vector3(agentObject.Translation.X, 0, agentObject.Translation.Z));
		        stage.setInfo(15, stage.agentLocation(this));
              stage.setInfo(16,
			        string.Format("          nextGoal ({0:f0}, {1:f0}, {2:f0})  distance to next goal = {3,5:f2})", 
				        nextGoal.Translation.X/stage.Spacing, nextGoal.Translation.Y, nextGoal.Translation.Z/stage.Spacing, distance) );
              if (distance  <= snapDistance)  {  
                 // snap to nextGoal and orient toward the new nextGoal 
                 nextGoal = path.NextNode;
                 // agentObject.turnToFace(nextGoal.Translation);
                 }
               oldKeyboardState = keyboardState;    // Update saved state.
              base.Update(gameTime);  // Agent's Update();
               break;

           case UpdateState.TREASURE_GOAL :
               agentObject.turnToFace(new Vector3(67050,100,67950));  // adjust to face nextGoal every move
		        // See if at or close to nextGoal, distance measured in 2D xz plane
		        distance = Vector3.Distance(
			        new Vector3(67050,0,67950),
			        new Vector3(agentObject.Translation.X, 0, agentObject.Translation.Z));
                stage.setInfo(15, stage.agentLocation(this));
              //stage.setInfo(16,
              //      string.Format("          nextGoal ({0:f0}, {1:f0}, {2:f0})  distance to next goal = {3,5:f2})", 
              //          nextGoal.Translation.X/stage.Spacing, nextGoal.Translation.Y, nextGoal.Translation.Z/stage.Spacing, distance) );
              if (distance  <= 300)  {  
                 // snap to nextGoal and orient toward the new nextGoal 
                 //nextGoal = path.NextNode;
                 currentState = UpdateState.PATH_FOLLOWING;
                 // agentObject.turnToFace(nextGoal.Translation);
                 }
               oldKeyboardState = keyboardState;    // Update saved state.
              base.Update(gameTime);  // Agent's Update();
               break;
       }

      }
   } 
}

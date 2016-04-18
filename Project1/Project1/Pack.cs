/*  
    Copyright (C) 2016 G. Michael Barnes
 
    The file Pack.cs is part of AGMGSKv7 a port and update of AGXNASKv6 from
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
/// Pack represents a "flock" of MovableObject3D's Object3Ds.
/// Usually the "player" is the leader and is set in the Stage's LoadContent().
/// With no leader, determine a "virtual leader" from the flock's members.
/// Model3D's inherited List<Object3D> instance holds all members of the pack.
/// 
/// 2/1/2016 last changed
/// </summary>
public class Pack : MovableModel3D {   
   Object3D leader;
/// <summary>
/// Construct a pack with an Object3D leader
/// </summary>
/// <param name="theStage"> the scene </param>
/// <param name="label"> name of pack</param>
/// <param name="meshFile"> model of a pack instance</param>
/// <param name="xPos, zPos">  approximate position of the pack </param>
/// <param name="aLeader"> alpha dog can be used for flock center and alignment </param>
   public Pack(Stage theStage, string label, string meshFile, int nDogs, int xPos, int zPos, Object3D theLeader)
      : base(theStage, label, meshFile) {
      isCollidable = true;
		random = new Random();
      leader = theLeader;
		int spacing = stage.Spacing;
		// initial vertex offset of dogs around (xPos, zPos)
		int [,] position = { {0, 0}, {7, -4}, {-5, -2}, {-7, 4}, {5, 2} , {3,9}, {12, 0}, {5, -10}};
		for( int i = 0; i < position.GetLength(0); i++) {
			int x = xPos + position[i, 0];
			int z = zPos + position[i, 1];
			float scale = (float)(0.5 + random.NextDouble());
			addObject( new Vector3(x * spacing, stage.surfaceHeight(x, z), z * spacing),
						  new Vector3(0, 1, 0), 0.0f,
						  new Vector3(scale, scale, scale));
			}
      }

   /// <summary>
   /// Each pack member's orientation matrix will be updated.
   /// Distribution has pack of dogs moving randomly.  
   /// Supports leaderless and leader based "flocking" 
   /// </summary>      
   public override void Update(GameTime gameTime) {
      // if (leader == null) need to determine "virtual leader from members"
       if (leader == null || stage.packLevel == 0)
       {
           float angle = 0.3f;
           foreach (Object3D obj in instance)
           {
               obj.Yaw = 0.0f;
               // change direction 4 time a second  0.07 = 4/60
               if (random.NextDouble() < 0.07)
               {
                   if (random.NextDouble() < 0.5) obj.Yaw -= angle; // turn left
                   else obj.Yaw += angle; // turn right
               }
               obj.updateMovableObject();
               stage.setSurfaceHeight(obj);
           }
       }
       else
       {
           float angle = 0.017f;
           float avoidanceForce = 5.0f;
           float cohesionForce = 10.0f;
           List<Vector3> separation = new List<Vector3>();
           List<Vector3> alignment = new List<Vector3>();
           List<Vector3> cohesion = new List<Vector3>();
           List<Object3D> dogs = new List<Object3D>();
           foreach (Object3D obj in instance)
           {
               dogs.Add(obj);
           }
           //calculated the total separation force for the dogs, using the separation from the other dogs and the leader
           for (int i = 0; i < dogs.Count; i++ )
           {
               Vector3 s;
               Vector3 separationSum = new Vector3();
               Vector3 a;
               Vector3 c;
               for(int j = 0; j < dogs.Count; j++)
               {
                   if(i != j)
                   {
                       s = new Vector3(); //temp separation force vector
                       s = dogs[i].Translation - dogs[j].Translation; //vector from dog[j] to dog[i]
                       s = s * avoidanceForce/Vector3.Distance(dogs[i].Translation, dogs[j].Translation); //weight vector using distance bewteen the dogs
                       if (Vector3.Distance(dogs[i].Translation, dogs[j].Translation) > 800)
                           s = Vector3.Zero;
                       separationSum = separationSum + s;
                   } //end if
               } // end for j

               //add dogs[i] separation force
               separation.Add(separationSum);
               //calculate separation force for dog and leader
               s = new Vector3(); //temp separation force vector
               s = dogs[i].Translation - leader.Translation; //vector from leader to dog[i]
               s = s * (avoidanceForce / Vector3.Distance(dogs[i].Translation, leader.Translation)); //weight vector using distance bewteen the dogs
               separation.Add(s);

               //calculate dogs alignment vector using leader's alignment
               a = Vector3.Negate(leader.Backward);
               if (Vector3.Distance(dogs[i].Translation, leader.Translation) > 1500 && Vector3.Distance(dogs[i].Translation, leader.Translation) < 1000)
                   a = Vector3.Zero;
               alignment.Add(a);
               //a = getAlignment(dogs[i], leader.Backward, 3.0);

               //calculate cohesion force for dog and leader
               c = new Vector3(); //temp cohesion force vector
               c = leader.Translation - dogs[i].Translation; //vector from dog[i] to leader
               c = c * (Vector3.Distance(dogs[i].Translation, leader.Translation) / cohesionForce); //weight vector using distance bewteen the dogs
               if (Vector3.Distance(dogs[i].Translation, leader.Translation) < 1500)
                   c = Vector3.Zero;
               cohesion.Add(c);

           } //end for i
           int k = 0;
               foreach (Object3D obj in instance)
               {
                   if(stage.packLevel == 1) // update pack 1/3 of time ~ 0.33
                   {
                       if (random.NextDouble() < 0.33)
                           rotateToTarget(obj, separation[k] + cohesion[k] + alignment[k], angle);
                       else //random move
                       {
                           obj.Yaw = 0.0f;
                           if (random.NextDouble() < 0.5) obj.Yaw -= angle; // turn left
                           else obj.Yaw += angle; // turn right
                       }
                   }
                   else if (stage.packLevel == 2) // update pack 2/3 of time ~ 0.66
                   {
                       if (random.NextDouble() < 0.66)
                           rotateToTarget(obj, separation[k] + cohesion[k] + alignment[k], angle);
                       else //random move
                       {
                           obj.Yaw = 0.0f;
                           if (random.NextDouble() < 0.5) obj.Yaw -= angle; // turn left
                           else obj.Yaw += angle; // turn right
                       }
                   }
                   else if (stage.packLevel == 3) // update pack 3/3 of time ~ 0.99
                   {
                       if (random.NextDouble() < 0.99)
                           rotateToTarget(obj, separation[k] + cohesion[k] + alignment[k], angle);
                       else //random move
                       {
                           obj.Yaw = 0.0f;
                           if (random.NextDouble() < 0.5) obj.Yaw -= angle; // turn left
                           else obj.Yaw += angle; // turn right
                       }
                   }
                   else
                   {
                       //shouldn't get here, do nothing
                   }
                   // change direction 4 time a second  0.07 = 4/60
                   //if (random.NextDouble() < 0.07)
                   //{
                   //    if (random.NextDouble() < 0.5) obj.Yaw -= angle; // turn left
                   //    else obj.Yaw += angle; // turn right
                   //}
                   obj.updateMovableObject();
                   stage.setSurfaceHeight(obj);
                   k++;
               }
       }
      base.Update(gameTime);  // MovableMesh's Update(); 
      }

   public void rotateToTarget(Object3D dog, Vector3 toTarget, float angle)
   {
       Vector3 axis, toObj;
       double radian, aCosDot;
       // put both vector on the XZ plane of Y == 0
       toObj = new Vector3(dog.Translation.X, 0, dog.Translation.Z);
       toTarget.Normalize();
       // Dot not defined for co-linear vectors:  test toTarget and Forward
       // if vectors are identical (w/in epsilon 0.05) return, no need to turnToFace
       if (Vector3.Distance(toTarget, dog.Forward) <= 0.05) return;
       // if vectors are reversed (w/in epsilon 0.05) nudgle alittle
       if (Vector3.Distance(Vector3.Negate(toTarget), dog.Forward) <= 0.05)
       {
           toTarget.X += 0.05f;
           toTarget.Z += 0.05f;
           toTarget.Normalize();
       }
       // determine axis for rotation
       axis = Vector3.Cross(toTarget, dog.Forward);  // order of arguments maters
       axis.Normalize();
       // get cosine of rotation
       aCosDot = Math.Acos(Vector3.Dot(toTarget, dog.Forward));
       // test and adjust direction of rotation into radians
       if (aCosDot == 0) radian = Math.PI * 2;
       else if (aCosDot == Math.PI) radian = Math.PI;
       else if (axis.X + axis.Y + axis.Z < 0) radian = (float)(2 * Math.PI - aCosDot);
       else radian = -aCosDot;
       // stage.setInfo(19, string.Format("radian to rotate = {0,5:f2}, axis for rotation ({1,5:f2}, {2,5:f2}, {3,5:f2})",
       //   radian, axis.X, axis.Y, axis.Z));
       if (Double.IsNaN(radian))
       {  // validity check, this should not happen
           stage.setInfo(19, "error:  Object3D.turnToFace() radian is NaN");
           return;
       }
       else
       {  // valid radian perform transformation
           // save location, translate to origin, rotate, translate back to location
           //radian = radian / alignmentWeight;
           Vector3 objectLocation = dog.Translation;
           
           dog.Orientation *= Matrix.CreateTranslation(-1 * objectLocation);
           // all terrain rotations are really on Y
           dog.Orientation *= Matrix.CreateFromAxisAngle(axis, (float)radian/60);
           dog.Up = Vector3.Up;  // correct for flipped from negative axis of rotation
           dog.Orientation *= Matrix.CreateTranslation(objectLocation);
           return;
       }
   }


   public Object3D Leader {
      get { return leader; }
      set { leader = value; }}

   }
}

/*
 * Group member:  Therese Horey         therese.horey.57@my.csun.edu
 * Group member:  Jesus Moran-Perez     jesus.moranperez.983@my.csun.edu
 * Project 1
 * Comp 565 Spring 2016
 */
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Project1
{
    public class Treasure : Model3D {
        public Vector3 translation;
        public Vector3 axis;
        public float radians;
        public bool tagged;

public Treasure(Stage theStage, string label, string meshFile, Vector3 pos, Vector3 rotationAxis, float rotation)  : base(theStage, label, meshFile) {
    translation = pos;
    axis = rotationAxis;
    radians = rotation;
    tagged = false;

    isCollidable = false;
    int spacing = stage.Terrain.Spacing;
    Terrain terrain = stage.Terrain;
    addObject(new Vector3(translation.X, 100+terrain.surfaceHeight((int)translation.X, (int)translation.Z), translation.Z),
         axis, radians, new Vector3(100, 100, 100));
	}	



/// <summary>
/// Shared constructor intialization code.
/// </summary>

    
    }
}

/*  
    Copyright (C) 2016 G. Michael Barnes
 
    The file NavNode.cs is part of AGMGSKv7 a port and update of AGXNASKv6 from
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

namespace Project1
{

    /// <summary>
    /// A WayPoint or Marker to be used in path following or path finding.
    /// Four types of WAYPOINT:
    /// <list type="number"> WAYPOINT, a navigatable terrain vertex </list>
    /// <list type="number"> PATH, a node in a path (could be the result of A*) </list>
    /// <list type="number"> OPEN, a possible node to follow in an A*path</list>
    /// <list type="number"> CLOSED, a node that has been evaluated by A* </list>

    class NavGraph : NavNode
    {
        private NavNode root;
        private int cost;
        private double distancetoGoal, distancetoSource;
        private int spacing = 150;
        private Stage stage;
        Dictionary<String, NavNode> graph;
        public List<NavNode> closed, astar, path, open;
        
        public NavGraph()
        {
            
        }

        public NavGraph(Stage theStage)
        {
            graph = new Dictionary<String, NavNode>();
            stage = theStage;
            open = new List<NavNode>();
            closed = new List<NavNode>();
            path = new List<NavNode>();
            astar = new List<NavNode>();
          
            //stage = theStage;
            //x = anX;
            //z = aZ;
            //name = string.Format("{0}::{1}", x, z);
            //nodeType = NavGraphNodeType.VERTEX;
            //node = new NavNode(new Vector3(x * spacing, stage.Terrain.surfaceHeight(x, z), z * spacing), NavNode.NavNodeEnum.WAYPOINT);
        }

       

        // Example key for graph based on (x,z)
        private String skey(int x, int z)
        {
            return String.Format("{0}::{1}", x, z);
        }

        // Use "this" 2D indexer property to access graph like an array
        // A wrapper for Dictionary<K,V>'s Item property
        public NavNode this[int x, int z]
        {
            get
            {
                NavNode node = null;
                try
                {
                    node = graph[skey(x, z)];
                    return node;
                }
                catch (KeyNotFoundException) { return node; }
            }
            set { graph[skey(x, z)] = value; }
        }

        // Example of foreach usage
        //		KeyValuePair<K,V>  	Dictionary's internal type
        //		*.Value	Dictionary's property for stored values
        private string visit()
        {
            string output = "";
            foreach (KeyValuePair<String, NavNode> item in graph)
                output = output + item.Key;
            return output;
        }

        // This example could have been written with TryGetValue()
        private void visitNode(int x, int z)
        {
            NavNode node = this[x, z];
            Console.Write("graph[{0}, {1}] == ", x, z);
            if (node == null)
                Console.WriteLine("null");
            else
                Console.WriteLine(node.ToString());
        }

        public void buildGraph()
        {
            List<NavNode> list = new List<NavNode>();
            for (int x = 0; x < 513; x = x + 21)
                for (int z = 0; z < 513; z = z + 21)
                {

                   NavNode node = new NavNode(new Vector3(x * spacing, stage.Terrain.surfaceHeight(x, z), z * spacing), NavNode.NavNodeEnum.WAYPOINT);
                    for (int i = 0; i < stage.Collidable.Count; i++)
                    {
                        if (accessible(node.Translation, stage.Collidable[i].Translation, stage.Collidable[i].ObjectBoundingSphereRadius))
                            list.Add(node);
                    }
                    graph.Add(skey(x, z), node);
                    open.Add(node);
                   
                }

            NavNode node_treasure = new NavNode(new Vector3( 457, stage.Terrain.surfaceHeight(457, 453), 453), NavNode.NavNodeEnum.WAYPOINT);
            list.Add(node_treasure);
            graph.Add(skey(457, 453), node_treasure);
            open.Add(node_treasure);

            node_treasure = new NavNode(new Vector3(435, stage.Terrain.surfaceHeight(435, 424), 424), NavNode.NavNodeEnum.WAYPOINT);
            list.Add(node_treasure);
            graph.Add(skey(435, 424), node_treasure);
            open.Add(node_treasure);

             node_treasure = new NavNode(new Vector3(465, stage.Terrain.surfaceHeight(465, 453), 453), NavNode.NavNodeEnum.WAYPOINT);
            list.Add(node_treasure);
            graph.Add(skey(465, 453), node_treasure);
            open.Add(node_treasure);


             node_treasure = new NavNode(new Vector3(485, stage.Terrain.surfaceHeight(485, 420), 420), NavNode.NavNodeEnum.WAYPOINT);
            list.Add(node_treasure);
            graph.Add(skey(485, 420), node_treasure);
            open.Add(node_treasure);

           node_treasure = new NavNode(new Vector3(424, stage.Terrain.surfaceHeight(424, 444), 444), NavNode.NavNodeEnum.WAYPOINT);
            list.Add(node_treasure);
            graph.Add(skey(425, 444), node_treasure);
            open.Add(node_treasure);









            Path path = new Path(stage, list, Path.PathType.SINGLE);
            stage.Components.Add(path);
        }

        public bool accessible(Vector3 nodePos, Vector3 objectPos, float radius)
        {
	        bool collision = false;
	        Vector3 position1 = nodePos;
	        Vector3 position2 = objectPos;

	        double distance = Math.Pow(Math.Abs(position1.X - position2.X), 2) + Math.Pow(Math.Abs(position1.Y - position2.Y), 2) + Math.Pow(Math.Abs(position1.Z - position2.Z), 2);

	        float sumRadius = (radius*radius);


	        if (distance < sumRadius)
		        collision = true;
	        else
		        collision = false;
	        return !collision;
        }
   
        public void buildAdjacency(Dictionary<String,NavNode>  graph)
        {
            double distance = 4455.0;
            foreach(var node1 in graph)
            {
                
                   foreach(var node2 in graph)
                   {
                       if(Vector3.Distance(node1.Value.Translation,node2.Value.Translation) < distance)
                       node1.Value.adjacent.Add(node2.Value);
                   }
            }

        }



        public List<NavNode> aStarPathFinding(NavNode source, NavNode destination)
        {
            open = new List<NavNode>();
            closed = new List<NavNode>();
            path = new List<NavNode>();

            NavNode current = source;
            open.Add(current);
            open.Sort(delegate(NavNode n1, NavNode n2)
            {
                return n1.Cost.CompareTo(n2.Cost);
            });

            int count = 0;
            while (open.Count != 0)
            {
                current = open.First<NavNode>();
                open.Remove(open.First<NavNode>());

                /*Console.WriteLine("Iteration {0} : Node Retreived: {1}", count, current.Cost);
                foreach (NavNode item in open) { Console.WriteLine("Node: {0}", item.Cost); }*/
                count++;

                //if (Vector3.Distance(current.Translation, destination.Translation) == 0.0)
                if (current.Translation == destination.Translation)
                {
                    Console.WriteLine("Current: {0}", current.Translation);
                    Console.WriteLine("Destination: {0}", destination.Translation);
                    break;
                }

                closed.Add(current);
                current.Navigatable = NavNode.NavNodeEnum.CLOSED;

                foreach (NavNode adjacent in current.Adjacent)
                {
                    if (!open.Contains(adjacent) && !closed.Contains(adjacent))
                    {
                        adjacent.PathPredecessor = current;
                        adjacent.DistanceFromSource = current.DistanceFromSource +
                            Vector3.Distance(current.Translation, adjacent.Translation);
                        adjacent.DistanceToGoal =
                            Vector3.Distance(current.Translation, adjacent.Translation) +
                            Vector3.Distance(adjacent.Translation, destination.Translation);
                        adjacent.Cost = adjacent.DistanceFromSource + adjacent.DistanceToGoal;

                        open.Add(adjacent);
                        adjacent.Navigatable = NavNode.NavNodeEnum.OPEN;
                    }
                }

                open.Sort(delegate(NavNode n1, NavNode n2)
                {
                    return n1.Cost.CompareTo(n2.Cost);
                });
            }

            while (Vector3.Distance(current.Translation, source.Translation) != 0.0)
            {
                current.Navigatable = NavNode.NavNodeEnum.PATH;
                path.Add(current);
                current = current.PathPredecessor;
            }
            aStarCompleted = true;
            return path;
        }
    }







}

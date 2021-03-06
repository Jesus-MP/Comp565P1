﻿/*
 * Group member:  Therese Horey         therese.horey.57@my.csun.edu
 * Group member:  Jesus Moran-Perez     jesus.moranperez.983@my.csun.edu
 * Project 1
 * Comp 565 Spring 2016
 */
/*  
    Copyright (C) 2016 G. Michael Barnes
 
    The file Stage.cs is part of AGMGSKv7 a port and update of AGXNASKv6 from
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
using System.IO;  // needed for trace()'s fout
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
/// Stage.cs  is the environment / framework for AGXNASK.
/// 
/// Stage declares and initializes the common devices, Inspector pane,
/// and user input options.  Stage attempts to "hide" the infrastructure of AGMGSK
/// for the developer.  It's subclass Stage is where the program specific aspects are
/// declared and created.
/// 
/// AGXNASK is a starter kit for Comp 565 assignments using XNA Game Studio 4.0
/// or MonoGames 3.?.
/// 
/// See AGXNASKv7Doc.pdf file for class diagram and usage information. 
/// 
/// Property Trace has been added.  Trace will print its string "value" to the
/// executable directory (where AGMGSKv7.exe is).
///   ../bin/Windows/debug/trace.txt file for Windows MonoGames projects
///   
/// 
/// 1/20/2016   last  updated
/// </summary>
public class Stage : Game {
   // Range is the length of the cubic volume of stage's terrain.
   // Each dimension (x, y, z) of the terrain is from 0 .. range (512)
   // The terrain will be centered about the origin when created.
   // Note some recursive terrain height generation algorithms (ie SquareDiamond)
   // work best with range = (2^n) - 1 values (513 = (2^9) -1).
   protected const int range = 512;
   protected const int spacing = 150;  // x and z spaces between vertices in the terrain
   protected const int terrainSize = range * spacing;
   // Graphics device
   protected GraphicsDeviceManager graphics;
   protected GraphicsDevice display;    // graphics context
   protected BasicEffect effect;        // default effects (shaders)
   protected SpriteBatch spriteBatch;   // for Trace's displayStrings
   protected BlendState blending, notBlending;
   // stage Models
   protected Model boundingSphere3D;    // a  bounding sphere model
   protected Model wayPoint3D;          // a way point marker -- for paths.
   protected bool drawBoundingSpheres = false;
   protected bool fog = false;
   protected bool fixedStepRendering = true;     // 60 updates / second
   // Viewports and matrix for split screen display w/ Inspector.cs
   protected Viewport defaultViewport;
   protected Viewport inspectorViewport, sceneViewport;     // top and bottom "windows"
   protected Matrix sceneProjection, inspectorProjection;
   // variables required for use with Inspector
   protected const int InfoPaneSize = 5;   // number of lines / info display pane
   protected const int InfoDisplayStrings = 20;  // number of total display strings
   protected Inspector inspector;
   protected SpriteFont inspectorFont;
   // Projection values
   protected Matrix projection;
   protected float fov = (float)Math.PI / 4;
   protected float hither = 5.0f, yon = terrainSize / 5.0f, farYon = terrainSize * 1.3f;
   protected float fogStart = 4000;
   protected float fogEnd = 10000;
   protected bool yonFlag = true;
   // User event state
   protected GamePadState oldGamePadState;
   protected KeyboardState oldKeyboardState;
   // Lights
   protected Vector3 lightDirection, ambientColor, diffuseColor;
   // Cameras
   protected List<Camera> camera = new List<Camera>();  // collection of cameras
   protected Camera currentCamera, topDownCamera;
   protected int cameraIndex = 0;
   // Required entities -- all AGXNASK programs have a Player and Terrain
   protected Player player = null;
   protected NPAgent npAgent = null;
   protected List<Treasure> treasure = new List<Treasure>();
   protected List<Treasure> marked = new List<Treasure>();
   protected Terrain terrain = null;
   protected List<Object3D> collidable = null;
   // Screen display and other information variables
   protected double fpsSecond;
   protected int draws, updates;
	private StreamWriter fout = null;
	// Stage variables
	private TimeSpan time;  // if you need to know the time see Property Time
    //Bool variable to control lerping
    private bool lerping = false;
    public List<float> packing =  new List<float>{ 0.0f, 0.33f, 0.66f, 0.99f };
    public int packLevel = 0;
    
      
     
    



   public Stage() : base() {
      graphics = new GraphicsDeviceManager(this);
      Content.RootDirectory = "Content";
      graphics.SynchronizeWithVerticalRetrace = false;  // allow faster FPS
      // Directional light values
      lightDirection = Vector3.Normalize(new Vector3(-1.0f, -1.0f, -1.0f));
      ambientColor =  new Vector3(0.4f, 0.4f, 0.4f);
      diffuseColor =  new Vector3(0.2f, 0.2f, 0.2f);
      IsMouseVisible = true;  // make mouse cursor visible
      // information display variables
      fpsSecond = 0.0;
      draws = updates = 0;
      }

   // Properties

   public Vector3 AmbientLight {
      get { return ambientColor; } }

   public Model BoundingSphere3D {
      get { return boundingSphere3D; } }

   public List<Object3D> Collidable {
      get { return collidable; } }

   public Vector3 DiffuseLight {
      get { return diffuseColor; } }

   public GraphicsDevice Display {
      get { return display; } }
 
   public bool DrawBoundingSpheres {
      get { return drawBoundingSpheres; }
      set { drawBoundingSpheres = value;
            inspector.setInfo(8, String.Format("Draw bounding spheres = {0}", drawBoundingSpheres)); } }

   public float FarYon {
      get { return farYon; } }

   public bool FixedStepRendering {
      get { return fixedStepRendering; }
      set { fixedStepRendering = value;
            IsFixedTimeStep = fixedStepRendering; } }

   public bool Fog {
      get { return fog; }
      set { fog = value; } }

   public float FogStart {
      get { return fogStart; } }

   public float FogEnd {
      get { return fogEnd; } }

   public Vector3 LightDirection {
      get { return lightDirection; } }

   public Matrix Projection {
      get { return projection; } }

   public int Range {
      get { return range; } }

   public BasicEffect SceneEffect {
      get { return effect; } }

   public int Spacing {
      get { return spacing; }}

   public Terrain Terrain {
      get { return terrain; } }

   public int TerrainSize {
      get { return terrainSize; } }

	public TimeSpan Time {  // Update's GameTime
		get { return time; }}

	/// <summary>
	/// Trace = "string to be printed"
	/// will append its value string to ../bin/debug/trace.txt file
	/// </summary>
	public string Trace {
		set { fout.WriteLine(value); }}

   public Matrix View {
      get { return currentCamera.ViewMatrix; } }

   public Model WayPoint3D {
      get { return wayPoint3D; }}

   public bool YonFlag {
      get { return yonFlag; }
      set { yonFlag = value;
            if (yonFlag)  setProjection(yon);
            else setProjection(farYon); } }

   // Methods

   public bool isCollidable(Object3D obj3d) {
      if (collidable.Contains(obj3d)) return true;
      else return false;
      }   

   /// <summary>
   /// Make sure that aMovableModel3D does not move off the terain.
   /// Called from MovableModel3D.Update()
   /// The Y dimension is not constrained -- code commented out.
   /// </summary>
   /// <param name="aName"> </param>
   /// <param name="newLocation"></param>
   /// <returns>true iff newLocation is within range</returns>
   public bool withinRange(String aName, Vector3 newLocation) {
      if (newLocation.X < spacing || newLocation.X > (terrainSize - 2 * spacing) ||
         newLocation.Z < spacing || newLocation.Z > (terrainSize - 2 * spacing)) {
         // inspector.setInfo(14, String.Format("error:  {0} can't move off the terrain", aName));
         return false; }
      else 
         return true; 
      }

   public void addCamera(Camera aCamera) {
      camera.Add(aCamera);
      cameraIndex++;
      }

	public String agentLocation(Agent agent)	{
		return string.Format("{0}:   Location ({1,5:f0} = {2,3:f0}, {3,3:f0}, {4,5:f0} = {5,3:f0})  Looking at ({6,5:f2},{7,5:f2},{8,5:f2})",
		agent.Name, agent.AgentObject.Translation.X, (agent.AgentObject.Translation.X) / spacing, agent.AgentObject.Translation.Y, agent.AgentObject.Translation.Z, (agent.AgentObject.Translation.Z) / spacing,
		agent.AgentObject.Forward.X, agent.AgentObject.Forward.Y, agent.AgentObject.Forward.Z);
		}

	public void setInfo(int index, string info) {
   inspector.setInfo(index, info);
   }

   protected void setProjection(float yonValue) {
      projection = Matrix.CreatePerspectiveFieldOfView(fov,
      graphics.GraphicsDevice.Viewport.AspectRatio, hither, yonValue);
      }

   /// <summary>
   /// Changing camera view for Agents will always set YonFlag false
   /// and provide a clipped view.
	/// 'x' selects the previous camera
	/// 'c' selects the next camera
   /// </summary>
   public void setCamera(int direction) {
      cameraIndex = (cameraIndex + direction);
      if (cameraIndex == camera.Count) cameraIndex = 0;
		if (cameraIndex < 0) cameraIndex = camera.Count -1;
      currentCamera = camera[cameraIndex];
      // set the appropriate projection matrix
      YonFlag = false;
      setProjection(farYon); 
      }
 
   /// <summary>
   /// Get the height of the surface containing stage coordinates (x, z)
   /// </summary>
   public float surfaceHeight(float x, float z) {
      return terrain.surfaceHeight( (int) x/spacing, (int) z/spacing); }

	/// <summary>
	/// Sets the Object3D's height value to the corresponding surface position's height
	/// </summary>
	/// <param name="anObject3D"> has  Translation.X and Translation.Y values</param>
   public void setSurfaceHeight(Object3D anObject3D)
   {

       //Vecs 3s needed for each corner
       Vector3 cornerA, cornerB, cornerC, cornerD;

       Vector3 aPos = anObject3D.Translation;
       //height values for the corners
       float heightA, heightB, heightC, heightD;
       heightB = terrain.surfaceHeight((int)(aPos.X / spacing) + 1, (int)(aPos.Z / spacing));
       heightC = terrain.surfaceHeight((int)(aPos.X / spacing), (int)(aPos.Z / spacing) + 1);
       heightD = terrain.surfaceHeight((int)(aPos.X / spacing) + 1, (int)(aPos.Z / spacing) + 1);
       heightA = terrain.surfaceHeight((int)(aPos.X / spacing), (int)(aPos.Z / spacing));

       //Make Vector3 Objects andd add heights as y value
       cornerA = new Vector3((int)(aPos.X / spacing), heightA, (int)(aPos.Z / spacing));
       cornerB = new Vector3((int)(aPos.X / spacing) * spacing + spacing, heightB, (int)(aPos.Z / spacing) * spacing);
       cornerC = new Vector3((int)(aPos.X / spacing) * spacing, heightC, (int)(aPos.Z / spacing) * spacing + spacing);
       cornerD = new Vector3((int)(aPos.X / spacing) * spacing + spacing, heightD, (int)(aPos.Z / spacing) * spacing + spacing);

       //difference on X and on Z
       float xy;
       float zy;
       float terrainHeight;

       //Differentiate between bottom and top surface
       //If player is closer to top left corner use lerp
       //else use lerp on bottom
       //top surface
       if (Vector3.Distance(cornerA, aPos) < Vector3.Distance(cornerD, aPos))
       {
           xy = Vector3.Lerp(cornerA, cornerB, ((float)(aPos.X - cornerA.X)) / spacing).Y - cornerA.Y;
           zy = Vector3.Lerp(cornerA, cornerC, ((float)(aPos.Z - cornerA.Z)) / spacing).Y - cornerA.Y;
           aPos.Y = cornerA.Y + xy + zy;

       }
       //bottom surface
       else
       {

           xy = Vector3.Lerp(cornerD, cornerC, ((float)(cornerD.X - aPos.X)) / spacing).Y - cornerD.Y;
           zy = Vector3.Lerp(cornerD, cornerB, ((float)(cornerD.Z - aPos.Z)) / spacing).Y - cornerD.Y;
           aPos.Y = cornerD.Y + xy + zy;

       }





       if (lerping == false)
       {
           terrainHeight = terrain.surfaceHeight(
           (int)(anObject3D.Translation.X / spacing),
           (int)(anObject3D.Translation.Z / spacing));
           anObject3D.Translation = new Vector3(aPos.X, terrainHeight, aPos.Z);

       }
       else
       {
           anObject3D.Translation = new Vector3(aPos.X, aPos.Y, aPos.Z);
       }





   }

   public void setBlendingState(bool state) {
      if (state) display.BlendState = blending;
      else display.BlendState = notBlending;
      }
  
   // Overridden Game class methods. 
  
   /// <summary>
   /// Allows the game to perform any initialization it needs to before starting to run.
   /// This is where it can query for any required services and load any non-graphic
   /// related content.  Calling base.Initialize will enumerate through any components
   /// and initialize them as well.
   /// </summary>
   protected override void Initialize() {
      // TODO: Add your initialization logic here
      base.Initialize();
      }


   /// <summary>
   /// Set GraphicDevice display and rendering BasicEffect effect.  
   /// Create SpriteBatch, font, and font positions.
   /// Creates the traceViewport to display information and the sceneViewport
   /// to render the environment.
   /// Create and add all DrawableGameComponents and Cameras.
   /// First, add all required contest:  Inspector, Cameras, Terrain, Agents
   /// Second, add all optional (scene specific) content
   /// </summary>
   protected override void LoadContent() {
      display = graphics.GraphicsDevice;
      effect = new BasicEffect(display);
      // Set up Inspector display
      spriteBatch = new SpriteBatch(display);      // Create a new SpriteBatch
      inspectorFont = Content.Load<SpriteFont> ("Consolas");    // Windows XNA && MonoGames
      // viewports
      defaultViewport = GraphicsDevice.Viewport;
      inspectorViewport = defaultViewport;
      sceneViewport = defaultViewport;
      inspectorViewport.Height = InfoPaneSize * inspectorFont.LineSpacing;
      inspectorProjection = Matrix.CreatePerspectiveFieldOfView((float) Math.PI/4.0f,
         inspectorViewport.Width/inspectorViewport.Height, 1.0f, 200.0f);
      sceneViewport.Height = defaultViewport.Height - inspectorViewport.Height;
      sceneViewport.Y = inspectorViewport.Height;
      sceneProjection = Matrix.CreatePerspectiveFieldOfView((float) Math.PI/4.0f,
         sceneViewport.Width /sceneViewport.Height, 1.0f, 1000.0f);
      // create Inspector display
      Texture2D inspectorBackground = Content.Load<Texture2D>("inspectorBackground");
      inspector = new Inspector(display, inspectorViewport, inspectorFont, Color.Black, inspectorBackground);
      // create information display strings
      // help strings
      inspector.setInfo(0, "AGMGSKv7 -- Academic Graphics MonoGames/XNA Starter Kit for CSUN Comp 565 assignments.");
      inspector.setInfo(1, "Press keyboard for input (not case sensitive 'H' || 'h')");
      inspector.setInfo(2, "Inspector toggles:  'H' help or info   'M'  matrix or info   'I'  displays next info pane.");
      inspector.setInfo(3, "Arrow keys move the player in, out, left, or right.  'R' resets player to initial orientation.");
      inspector.setInfo(4, "Stage toggles:  'B' bounding spheres, 'C' || 'X' cameras, 'F' fog, 'T' updates, 'Y' Toggle Lerping 'L' ");
      // initialize empty info strings
      for (int i = 5; i < 20; i++) inspector.setInfo(i, "  ");
      // set blending for bounding sphere drawing
      blending = new BlendState();
      blending.ColorSourceBlend = Blend.SourceAlpha;
      blending.ColorDestinationBlend = Blend.InverseSourceAlpha;
      blending.ColorBlendFunction = BlendFunction.Add;
      notBlending = new BlendState();
      notBlending = display.BlendState;
      // Create and add stage components
      // You must have a TopDownCamera, BoundingSphere3D, WayPoint3D, Terrain, and Agents (player, npAgent) in your stage!
      // Place objects at a position, provide rotation axis and rotation radians.
      // All location vectors are specified relative to the center of the stage.
      // Create a top-down "Whole stage" camera view, make it first camera in collection.
      topDownCamera = new Camera(this, Camera.CameraEnum.TopDownCamera);
      camera.Add(topDownCamera);
		// Set initial camera and projection matrix
		setCamera(0);  // select the first camera
      boundingSphere3D = Content.Load<Model>("boundingSphereV3");
      wayPoint3D = Content.Load<Model>("100x50x100Marker");				// model for navigation node display
      // Create required entities:  
      collidable = new List<Object3D>();  // collection of objects to test for collisions
		terrain = new Terrain(this, "terrain", "heightTexture", "colorTexture");
      Components.Add(terrain);
      // Load Agent mesh objects, meshes do not have textures
      treasure.Add(new Treasure(this, "treasure", "crate", new Vector3(67050, 150, 67950), new Vector3(0, 1, 0), 0.0f));

      treasure.Add(new Treasure(this, "treasure", "crate", new Vector3(435*spacing, 150,424*spacing), new Vector3(0, 1, 0), 0.0f));
      treasure.Add(new Treasure(this, "treasure", "crate", new Vector3(465*spacing, 150, 453*spacing), new Vector3(0, 1, 0), 0.0f));
      treasure.Add(new Treasure(this, "treasure", "crate", new Vector3(485*spacing,150,420*spacing), new Vector3(0, 1, 0), 0.0f));
      treasure.Add(new Treasure(this, "treasure", "crate", new Vector3(425*spacing, 150,444*spacing), new Vector3(0, 1, 0), 0.0f));
      for (int i = 0; i < treasure.Count; i++)
          Components.Add(treasure[i]);
      marked.Add(new Treasure(this, "treasure", "crateOpen", new Vector3(67050, 150, 67950), new Vector3(0, 1, 0), 0.0f));
      marked.Add(new Treasure(this, "treasure", "crateOpen", new Vector3(435 * spacing, 150, 424 * spacing), new Vector3(0, 1, 0), 0.0f));
      marked.Add(new Treasure(this, "treasure", "crateOpen", new Vector3(465 * spacing, 150, 453 * spacing), new Vector3(0, 1, 0), 0.0f));
      marked.Add(new Treasure(this, "treasure", "crateOpen", new Vector3(485 * spacing, 150, 420 * spacing), new Vector3(0, 1, 0), 0.0f));
      marked.Add(new Treasure(this, "treasure", "crateOpen", new Vector3(425 * spacing, 150, 444 * spacing), new Vector3(0, 1, 0), 0.0f));
      for (int i = 0; i < marked.Count; i++)
      {
          Components.Add(marked[i]);
          marked[i].Visible = false;
      }
      player = new Player(this, "Chaser",
         new Vector3(510 * spacing, terrain.surfaceHeight(510, 507), 507 * spacing),
         new Vector3(0, 1, 0), 0.78f, "redAvatarV6", treasure, marked);  // face looking diagonally across stage
      player.IsCollidable = true; // test collisions for player
      Components.Add(player);
      npAgent = new NPAgent(this, "Evader",
         new Vector3(490 * spacing, terrain.surfaceHeight(490, 450), 450 * spacing),
         new Vector3(0, 1, 0), 0.0f, "magentaAvatarV6", treasure, marked);  // facing +Z
		npAgent.IsCollidable = false;  // npAgent does not test for collisions
      Components.Add(npAgent);
		// create file output stream for trace()
		fout = new StreamWriter("trace.txt", false);
		Trace = string.Format("{0} trace output from AGMGSKv7", DateTime.Today.ToString("MMMM dd, yyyy"));  
		//  ------ The wall and pack are required for Comp 565 projects, but not AGMGSK   ---------
		// create walls for navigation algorithms
		Wall wall = new Wall(this, "wall", "100x100x100Brick");
		Components.Add(wall);
		// create a pack for "flocking" algorithms
		// create a Pack of 6 dogs centered at (450, 500) that is leaderless
		Pack pack = new Pack(this, "dog", "dogV6", 9, 450, 430, player.AgentObject);
		Components.Add(pack);
      // ----------- OPTIONAL CONTENT HERE -----------------------
      // Load content for your project here
      // create a temple
      Model3D m3d = new Model3D(this, "temple", "templeV3");
      m3d.IsCollidable = true;  // must be set before addObject(...) and Model3D doesn't set it
      m3d.addObject(new Vector3(340 * spacing, terrain.surfaceHeight(340, 340), 340 * spacing),
         new Vector3(0, 1, 0), 0.79f); // , new Vector3(1, 4, 1));
      Components.Add(m3d);
      //Model3D treasure = new Model3D(this, "treasure", "treasure");
      //treasure.IsCollidable = true;  // must be set before addObject(...) and Model3D doesn't set it
      //treasure.addObject(new Vector3(67050,150,67950),
      //   new Vector3(0, 1, 0), 0.0f, new Vector3(3,3,3)); // , new Vector3(1, 4, 1));
      //Components.Add(treasure);
      //treasure = new Treasure(this, "treasure", "treasure", new Vector3(67050, 150, 67950), new Vector3(0, 1, 0), 0.0f);
      //Components.Add(treasure);
		// create 20 clouds
		Cloud cloud = new Cloud(this, "cloud", "cloudV3", 20);
		Components.Add(cloud);
		Trace = string.Format("Scene created with {0} collidable objects.", Collidable.Count);
      }
  
   /// <summary>
   /// UnloadContent will be called once per game and is the place to unload
   /// all content.
   /// </summary>
   protected override void UnloadContent() {
      // TODO: Unload any non ContentManager content here
		Trace = string.Format("{0} hours {1} minutes {2} seconds elapsed until system exit", time.Hours, time.Minutes, time.Seconds);
		fout.Close(); // close fout file output stream
      }

   /// <summary>
   /// Uses an Inspector to display update and display information to player.
   /// All user input that affects rendering of the stage is processed either
   /// from the gamepad or keyboard.
   /// See Player.Update(...) for handling of user events that affect the player.
   /// The current camera's place is updated after all other GameComponents have 
   /// been updated.
   /// </summary>
   /// <param name="gameTime">Provides a snapshot of timing values.</param>
   protected override void Update(GameTime gameTime) {
      // set info pane values
		time = gameTime.TotalGameTime;
      fpsSecond += gameTime.ElapsedGameTime.TotalSeconds;
      updates++;
      if (fpsSecond >= 1.0) {
         inspector.setInfo(10,
            String.Format("{0} camera    Game time {1:D2}::{2:D2}::{3:D2}    {4:D} Updates/Seconds {5:D} Draws/Seconds",
               currentCamera.Name, time.Hours, time.Minutes, time.Seconds, updates.ToString(), draws.ToString()));
         draws = updates = 0;
         fpsSecond = 0.0;
			inspector.setInfo(11, agentLocation(player) + String.Format(" Treasure: {0}", player.treasureCount));
            inspector.setInfo(12, agentLocation(npAgent) + String.Format(" Treasure: {0}", npAgent.treasureCount));
            // inspector lines 13 and 14 can be used to describe player and npAgent's status
            inspector.setInfo(13, String.Format("Packing level: {0}", packing[packLevel]));
            inspector.setMatrices("player", "npAgent", player.AgentObject.Orientation, npAgent.AgentObject.Orientation);

         }
      // Process user keyboard events that relate to the render state of the the stage
      KeyboardState keyboardState = Keyboard.GetState();
      if (keyboardState.IsKeyDown(Keys.Escape)) Exit();
      else if (keyboardState.IsKeyDown(Keys.B) && !oldKeyboardState.IsKeyDown(Keys.B))
          DrawBoundingSpheres = !DrawBoundingSpheres;
      else if (keyboardState.IsKeyDown(Keys.C) && !oldKeyboardState.IsKeyDown(Keys.C))
          setCamera(1);
      else if (keyboardState.IsKeyDown(Keys.X) && !oldKeyboardState.IsKeyDown(Keys.X))
          setCamera(-1);
      else if (keyboardState.IsKeyDown(Keys.F) && !oldKeyboardState.IsKeyDown(Keys.F))
          Fog = !Fog;
      // key event handlers needed for Inspector
      // set help display on
      else if (keyboardState.IsKeyDown(Keys.H) && !oldKeyboardState.IsKeyDown(Keys.H))
      {
          inspector.ShowHelp = !inspector.ShowHelp;
          inspector.ShowMatrices = false;
      }
      // set info display on
      else if (keyboardState.IsKeyDown(Keys.I) && !oldKeyboardState.IsKeyDown(Keys.I))
          inspector.showInfo();

    //set the lerping
      else if (keyboardState.IsKeyDown(Keys.L) && !oldKeyboardState.IsKeyDown(Keys.L))
          lerping = !lerping;
      else if (keyboardState.IsKeyDown(Keys.P) && !oldKeyboardState.IsKeyDown(Keys.P))
      {
          if (packLevel == 3)
              packLevel = 0;
          else
              packLevel++;
      }
      // set miscellaneous display on
      else if (keyboardState.IsKeyDown(Keys.M) && !oldKeyboardState.IsKeyDown(Keys.M))
      {
          inspector.ShowMatrices = !inspector.ShowMatrices;
          inspector.ShowHelp = false;
      }
      // toggle update speed between FixedStep and ! FixedStep
      else if (keyboardState.IsKeyDown(Keys.T) && !oldKeyboardState.IsKeyDown(Keys.T))
          FixedStepRendering = !FixedStepRendering;
      else if (keyboardState.IsKeyDown(Keys.Y) && !oldKeyboardState.IsKeyDown(Keys.Y))
          YonFlag = !YonFlag;  // toggle Yon clipping value.
      oldKeyboardState = keyboardState;    // Update saved state.
      base.Update(gameTime);  // update all GameComponents and DrawableGameComponents
      currentCamera.updateViewMatrix();
      }

   /// <summary>
   /// Draws information in the display viewport.
   /// Resets the GraphicsDevice's context and makes the sceneViewport active.
   /// Has Game invoke all DrawableGameComponents Draw(GameTime).
   /// </summary>
   /// <param name="gameTime">Provides a snapshot of timing values.</param>
   protected override void Draw(GameTime gameTime) {
      draws++;
      display.Viewport = defaultViewport; //sceneViewport;
      display.Clear(Color.CornflowerBlue);
      // Draw into inspectorViewport
      display.Viewport = inspectorViewport;
      spriteBatch.Begin();
      inspector.Draw(spriteBatch);
      spriteBatch.End();
      // need to restore state changed by spriteBatch
      GraphicsDevice.BlendState = BlendState.Opaque;
      GraphicsDevice.DepthStencilState = DepthStencilState.Default;
      GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
      // draw objects in stage 
      display.Viewport = sceneViewport;
      display.RasterizerState = RasterizerState.CullNone;
      base.Draw(gameTime);  // draw all GameComponents and DrawableGameComponents
      }



	/*
	  /// <summary>
	  /// The main entry point for the application.
	 /// </summary>
	  static void Main(string[] args) {
		  using (Stage stage = new Stage()){ stage.Run(); }
		  }
	 */
   }
}

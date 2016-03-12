/*  
    The file TerrainMap.cs is part of AGMGSKv7 
    Academic Graphics Starter Kit version 6 for MonoGames 3.2 or XNA 4 refresh
   
    Mike Barnes
    1/30/2016

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

/*	TerrainMap for MonoGames requirements Visual Studio and MonoDevelop:
	MonoDevelop Project | Edit References  | ALL | check System.Drawing, click OK
   Visual Studio Project | Add Refereces  | check System.Drawing, click OK
*/


using System;
using System.IO;  // needed for TerrainMap's use of Stream class in saveTerrainAsText()
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace TerrainMap {

/// <summary>
/// XN4 project that can create terrain data textures.
/// MonoGame project use, see note at end of summary.
/// 
/// Generate and save two 2D textures:  heightTexture.png and colorTexture.png.
/// File heightTexture.png stores a terrain's height values 0..255.
/// File colorTexture.png stores the terrain's vertex color values.
/// The files are saved in the execution directory.
/// 
/// Pressing 't' will toggle the display between the height and color
/// texture maps.  As distributed, the heightTexture will look all black
/// because the values range from 0 to 3.
/// 
/// The heightTexture will be mostly black since in the SK565v3 release there
/// are two height areas:  grass plain and pyramid.  The pyramid (upper left corner)'
/// will show grayscale values. 
/// Grass height values range from 0..3 -- which is black in greyscale.
/// 
/// Note:  using grayscale in a texture to represent height constrains the 
/// range of heights from 0 to 255.  Often you need to scale the values into this range
/// before saving the texture.  In your world's terrain you can then scale these 
/// values to the range you want.  This program does not scale since no values
/// become greater than 255.
/// 
/// Normally one thinks of a 2D texture as having [u, v] coordinates. 
/// In createHeightTexture() the height and in createColorTexture the color 
/// values are created.
/// The heightMap and colorMap used are [u, v] -- 2D.  They are converted to a 
/// 1D textureMap1D[u*v] when the colorTexture's values are set.
/// This is necessary because the method
///       newTexture.SetData<Color>(textureMap1D);
/// requires a 1D array, not a 2D array.
/// 
/// Program design was influenced by Riemer Grootjans example 3.7
/// Create a texture and save to file.
/// In XNA 2.0 Grame Programming Recipies:  A Problem-Solution Approach,
/// pp 176-178, Apress, 2008.
/// 
/// MonoGames can write textures using System.Drawing.Color and System.Drawing.Bitmap
/// classes.  You need to add a reference for System.Drawing in Visual Studio or MonoDevelop
///  
/// Mike Barnes
/// 1/30/2016
/// </summary>
   
public class TerrainMap : Game {
   int textureWidth = 512;  // textures should be powers of 2 for mipmapping
   int textureHeight = 512;
   GraphicsDeviceManager graphics;
   GraphicsDevice device;
   SpriteBatch spriteBatch;
   Texture2D heightTexture, colorTexture; // resulting textures 
   Color[,] colorMap, heightMap;  // values for the color and height textures
   Color[] textureMap1D;  // hold the generated values for a texture.
   Random random;
   bool showHeight = false;
   KeyboardState oldState;
   
   /// <summary>
   /// Constructor
   /// </summary>
   public TerrainMap() {
      graphics = new GraphicsDeviceManager(this);
      Window.Title = "Terrain Maps " + textureWidth + " by " + textureHeight + " to change map 't'";
      Content.RootDirectory = "Content";
      random  = new Random();
      }
      
   /// <summary>
   /// Set the window size based on the texture dimensions.
   /// </summary>
   
	protected override void Initialize() {
		// Game object exists, set its window size 
		graphics.PreferredBackBufferWidth = textureWidth;
		graphics.PreferredBackBufferHeight = textureHeight;
		graphics.ApplyChanges();
		base.Initialize();
		}

   /// <summary>
   /// Create and save two textures:  
   ///   heightTexture.png 
   ///   colorTexture.png
   /// </summary>

	protected override void LoadContent() {
		// Create a new SpriteBatch, which can be used to draw textures.
		spriteBatch = new SpriteBatch(GraphicsDevice);
		device = graphics.GraphicsDevice;
		heightTexture = createHeightTexture();
		colorTexture = createColorTexture();
		//saveTerrainAsText("terrain.dat"); // FYI: save terrain as text file included in unused method
		saveTexture(heightMap, "heightTexture.png");
		saveTexture(colorMap, "colorTexture.png");
		}
			
	/// <summary>
	/// Uses .Net System.Drawing.Bitmap and System.Drawing.Color to create
	/// png image files.
	/// </summary>
	/// <param name="map"> Color [width, height] values for texture </param>
	/// <param name="filename"> texture's nanme</param>
	private void saveTexture(Color[,] map, string filename) {
		System.Drawing.Color color;
		System.Drawing.Bitmap image =  new System.Drawing.Bitmap(textureWidth, textureHeight);
		for(int x = 0; x < textureWidth; x++) 
			for(int z = 0; z < textureHeight; z++) {
				color = System.Drawing.Color.FromArgb(Convert.ToInt32(map[x, z].R),
				Convert.ToInt32(map[x, z].G), Convert.ToInt32(map[x, z].B));
				image.SetPixel(x, z, color);
				}
		image.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
		}


    /// <summary>
    /// Save the terrain data as a text file.  This method is provided for
    /// illustration purposes.  Not used by TerrainMap
    /// </summary>
    /// <param name="filename"> terrain data's file name</param>
    private void saveTerrainAsText(string filename) {
        StreamWriter fout = new StreamWriter("terrain.dat", false);
        fout.WriteLine("Terrain data: vertex positions (x,y,z) and colors (r,g,b)");
        for(int x = 0; x < textureWidth; x++) 
	        for(int z = 0; z < textureHeight; z++)
		        fout.WriteLine("{0}  {1}  {2}  {3}  {4}  {5}",
			        x, Convert.ToInt32(heightMap[x, z].R), z, Convert.ToInt32(colorMap[x, z].R),
			        Convert.ToInt32(colorMap[x, z].G), Convert.ToInt32(colorMap[x, z].B));
        fout.Close();
        }

   /// <summary>
   /// Create a height map as a texture of byte values (0..255) 
   /// that can be viewed as a greyscale bitmap.  
   /// The scene will have a plain of grass (heights 0..3) and
   /// a pyramid (height > 5).
   /// </summary>
   /// <returns>height texture</returns>

   private Texture2D createHeightTexture() {
       
       /*
      float height;
      Vector3 colorVec3;
      heightMap = new Color[textureWidth, textureHeight];
      // first create the "plain" heights
      for (int x = 0; x < textureWidth; x++)
         for (int z = 0; z < textureHeight; z++) {
            height = ((float) random.Next(3))/255.0f; // float version of byte value 
            colorVec3 = new Vector3(height, height, height);
            heightMap[x, z] = new Color(colorVec3);  // a color where r = g = b = the hieght value
            }
        */
        
       //Brownian Motion
       float height;
       Vector3 colorVec3;
       //intialize step and radius
       int step = 3;
       int radius = 15;

       int[,] Hmap=new int[textureWidth,textureHeight];
       int nCenter = 3; //declares number of centers
       int[,] centers = new int[nCenter, 2];
       centers[0, 0] = 70;
       centers[0, 1] = 70;
       centers[1, 0] = 250;
       centers[1, 1] = 270;
       centers[2, 0] = 50;
       centers[2, 1] = 90;
      
       heightMap = new Color[textureWidth, textureHeight];
       
       int xposition, zposition,re; //position in heightMap
       



       //create the plain heights

       for (int x = 0; x < textureWidth; x++)
           for (int z = 0; z < textureHeight;z++ )
           {
               height = ((float)random.Next(3)) / 255.0f;
               colorVec3 = new Vector3(height, height, height);
               heightMap[x, z] = new Color(colorVec3);
               Hmap[x, z] = 0;

           }




       //Brownian motion 
       for (int p = 0; p <2; p++)
       {
           //increment heights for p*c*steps heightMap positions

           re = random.Next(nCenter);
           xposition = centers[re, 0];
           zposition = centers[re, 1];

           for (int i = 0; i < 10000; i++)
           {

               for (int x = xposition - radius; x <= xposition + radius; x++)
                   for (int z = zposition - radius; z <= zposition + radius; z++)
                       if (x >= 20 && x < textureWidth - 20 && z >= 20 && z < textureHeight - 20)
                           Hmap[x, z] += 1;//add 1 to all heightMap[x,z]


               //take steps on a random walk

               xposition += step * random.Next(-1, 2);
               zposition += step * random.Next(-1, 2);






               //if x or z are outside height map dimensions reset x and z randomnly to selected center [c]
               if (xposition < 0 || xposition > textureWidth || zposition < 0 || zposition >= textureHeight)
               {
                   int r = random.Next(nCenter);
                   xposition = centers[r, 0];
                   zposition = centers[r, 1];

               }

           }
       }


       
       for (int x = 0; x < textureHeight; x++)
           for (int z = 0; z < textureHeight;z++ )
           {
              
               height = Hmap[x, z] / 255.0f;//convert to grayscale 0.0 to 255.0f
               heightMap[x, z] = new Color(new Vector3(height, height, height));
           }

       //convert heightMap[,] to textureMap1D[]
       textureMap1D = new Color[textureWidth * textureHeight];
       int j = 0;
       for (int x = 0; x < textureWidth; x++)
       {
           for (int z = 0; z < textureHeight; z++)
           {
               textureMap1D[j] = heightMap[x, z];
               j++;
           }
       }

       //create the texture to return
       Texture2D newTexture = new Texture2D(device, textureWidth, textureHeight);
       newTexture.SetData<Color>(textureMap1D);
       return newTexture;





               // The pyramid created here is for illustration, remove it from your P1 TerrainMap
               // Second create the pyramid with a base of 300 by 300 and a diagonal of 424 centered at (156, 156). 
               // Have the step size of 5 and the "brick" height of each step is 9.

               /*int centerX = 156;
              int centerZ = 156;
              int pyramidSide = 300;
              int halfWidth = pyramidSide / 2;
              int pyramidDiagonal = (int) Math.Sqrt(2*Math.Pow(pyramidSide, 2));
              int brick = 9;
              int stepSize = 5;
              int [,] pyramidHeight = new int[pyramidSide, pyramidSide];
              // initialize heights
              for (int x = 0; x < pyramidSide; x++)
                 for (int z = 0; z < pyramidSide; z++) pyramidHeight[x, z] = 0;
              // create heights for pyramid
              for (int s = 0; s < pyramidDiagonal; s += stepSize) 
                 for (int x = s; x < pyramidSide - s; x++)
                    for (int z = s; z < pyramidSide - s; z ++) 
                       pyramidHeight[x, z] += brick;
                // end of pyramid create code
       

              // convert corresponding heightMap color to pyramidHeight equivalent color
              for (int x = 0; x < pyramidSide; x++)
                 for (int z = 0; z < pyramidSide; z++) {
                    height = pyramidHeight[x, z]/255.0f;  // convert to grayscale 0.0 to 255.0f
                    heightMap[centerX - halfWidth + x, centerZ - halfWidth + z] = 
                       new Color(new Vector3(height, height, height));  
                    }   
                * */




       /*
               // convert heightMap[,] to textureMap1D[]
       
       textureMap1D = new Color[textureWidth * textureHeight];
      int i = 0;
      for (int x = 0; x < textureWidth; x++)
         for (int z = 0; z < textureHeight; z++) {
            textureMap1D[i] = Hmap[x, z];
            i++;
         }    
        **/
       /*
      // create the texture to return.       
      Texture2D newTexture = new Texture2D(device, textureWidth, textureHeight); 
      newTexture.SetData<Color>(textureMap1D);
      return newTexture;
        * */
      }
        

	/// <summary>
	/// Return random int -range ... range
	/// </summary>
	/// <param name="range"></param>
	/// <returns></returns>
	private int fractalRand(int range) {
		if (random.Next(2) == 0)  // flip a coin
			return (random.Next(range));
		else
			return (-1 * (random.Next(range)));
		}


	/// <summary>
	/// Convert a height value in the range of 0 ... 255 to
	/// a Vector4 value that will be later converted to a Color.  
	/// Vector4 is used instead of color to add some random noise to the value
	/// </summary>
	/// <param name="h"></param>
	/// <returns></returns>
	private Vector4 heightToVector4(int h) {
		int r, g, b;
		if (h < 50){  // dark grass
			r = 20;  
			g = 20 ;  // 128 .. 192 ;
			b = 20;  
			}
		else if (h < 75) {  // lighter green grass
			r = 44 + random.Next(65);		// 64 .. 128 ;
			g = 32 + random.Next(33);		// 128 .. 160 ;
            b = 32;				// 0 .. 32 
			}
		else if (h < 100) { // lighter green / yellow grass
			r = 160 + random.Next(23);		// 128 .. 160 
			g = 192 + random.Next(63);		// 160 .. 192
			b = 64 + random.Next(33);		// 32 .. 64
			}
		else if (h < 125) {  // green .. brown dirt
			r = 160 + random.Next(21);		// 160 .. 180
			g = 192 - random.Next(65);		// 192 .. 128
			b = 64 - random.Next(33);		// 64 .. 32
			}
		else if (h < 150) {  // dark to lighter dirt
			r = 180 - random.Next(61);		// 180 .. 120
			g = 120 - random.Next(21);		// 120 .. 100
			b = 20; 
			}
		else if (h < 175) { // light dirt to gray
			r = 180 - random.Next(41);		// 180 .. 120
			g = 120 - random.Next(21);		// 120 .. 100
			b = 20 + random.Next(41);		// 20 .. 60
			}
		else if (h < 225)    // dark gray to light gray
			r = g = b = 128 + random.Next(98);  // 128 .. 225
		// top of mountains don't need randomization.
		else    // snow
			r = g = b = h;
		// add noise with fractalRand
		if (h <= 175) {  // not snow
			// randomize values and clamp values to 0..255
			r = Math.Abs((r + fractalRand(20)) % 255);
			g = Math.Abs((g + fractalRand(20)) % 255);
			b = Math.Abs((b + fractalRand(20)) % 255);
			}
		else if (h > 175 && h < 225)  // snow
			r = g = b = Math.Abs((r + fractalRand(20)) % 255);
		return new Vector4(r / 255.0f, g / 255.0f, b / 255.0f, 1.0f);  // must be floats
		}

   /// <summary>
   /// Create a color texture that will be used to "color" the terrain.
   /// Some comments about color that might explain some of the code in createColorTexture().
   /// Colors can be converted to vector4s.   vector4Value =  colorValue / 255.0
   /// color's (RGBA), color.ToVector4()
   /// Color.DarkGreen (R:0 G:100 B:0 A:255)    vector4 (X:0 Y:0.392 Z:0 W:1)  
   /// Color.Green     (R:0 G:128 B:0 A:255)    vector4 (X:0 Y:0.502 Z:0 W:1)  
   /// Color.OliveDrab (R:107 G:142 B:35 A:255) vector4 (X:0.420 Y:0.557 Z:0.137, W:1) 
   /// You can create colors with new Color(byte, byte, byte, byte) where byte = 0..255
   /// or, new Color(byte, byte, byte).
   /// 
   /// The Color conversion to Vector4 and back is used to add noise.
   /// You could just have Color.
   /// </summary>
   /// <returns>color texture</returns>
    
   private Texture2D createColorTexture() {
      int grassHeight = 5;
      Vector4 colorVec4 = new Vector4();
      colorMap = new Color[textureWidth, textureHeight];
      for (int x = 0; x < textureWidth; x++)
         for (int z = 0; z < textureHeight; z++) {
            if (heightMap[x, z].R < grassHeight) // make random grass
						switch (random.Next(3)) { 
							case 0 : colorVec4 = new Color(0, 0, 0, 255).ToVector4(); break;  // Color.DarkGreen
							case 1 : colorVec4 = Color.Black.ToVector4();     break;
							case 2 : colorVec4 = Color.Red.ToVector4(); break;
							}
                    
                
               // color the pyramid based on height
					else colorVec4 =  heightToVector4(heightMap[x, z].R);               
      


             // add some noise, convert to a color, and set colorMap
			colorVec4 = colorVec4 + new Vector4((float)(random.NextDouble() / 20.0));
         colorMap[x, z] = new Color(colorVec4);
         }
      // convert colorMap[,] to textureMap1D[]
      textureMap1D = new Color[textureWidth * textureHeight];
      int i = 0;
      for (int x = 0; x < textureWidth; x++)
         for (int z = 0; z < textureHeight; z++) {
            textureMap1D[i] = colorMap[x, z];
            i++;
            }
      // create the texture to return.   
      Texture2D newTexture = new Texture2D(device, textureWidth, textureHeight); 
      newTexture.SetData<Color>(textureMap1D);
      return newTexture;
      }
/*
   /// <summary>
   /// UnloadContent will be called once per game and is the place to unload
   /// all content.
   /// </summary>
   protected override void UnloadContent() {
      // TODO: Unload any non ContentManager content here
      }
*/

   /// <summary>
   /// Process user keyboard input.
   /// Pressing 'T' or 't' will toggle the display between the height and color textures
   /// </summary>

   protected override void Update(GameTime gameTime) {
      KeyboardState keyboardState = Keyboard.GetState();
      if (keyboardState.IsKeyDown(Keys.Escape)) Exit();
      else if (Keyboard.GetState().IsKeyDown(Keys.T) && !oldState.IsKeyDown(Keys.T))
         showHeight = ! showHeight;
      oldState = keyboardState;    // Update saved state.
      base.Update(gameTime);
      }
      
   /// <summary>
   /// Display the textures.
   /// </summary>
   /// <param name="gameTime"></param>
   
   protected override void Draw(GameTime gameTime) {
      device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1, 0);
      spriteBatch.Begin();
      if (showHeight) 
         spriteBatch.Draw(heightTexture, Vector2.Zero, Color.White);
      else
         spriteBatch.Draw(colorTexture, Vector2.Zero, Color.White);
      spriteBatch.End();

      base.Draw(gameTime);
      }
			
   }
   }

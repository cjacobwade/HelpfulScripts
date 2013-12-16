using UnityEngine;
using System.Collections;

//Used so anim objects can be defined on a per object basis
[System.Serializable]
public class Anim
{
	//Name of animation
	public string animName;
	
	//Frame information
	public int sheetNum, startFrame, animFrames, frameRate;
	
	//Loop?
	public bool loop;
}

//Used so this object can reference multiple spritesheets
[System.Serializable]
public class SpriteSheet
{
	public Texture img;
	public int framesPerRow, totalFrames;
}

public class SpriteAnimation : MonoBehaviour
{	
	//Sprite sheet
	Material sprite;
	
	//Frame data
	public int startAnim;
	int currentFrame = 0, rowNumber;
	float frameWidth, frameHeight;
	
	//Anim options
	public bool playOnStart;	
	bool play = false;
	
	float animTimer = 0;
	
	public SpriteSheet[] spriteSheets;
	public Anim[] animations;

	[HideInInspector]
	public SpriteSheet currentSheet;
	[HideInInspector]
	public Anim currentAnim;
	
	// Use this for initialization
	void Start ()
	{
		sprite = this.renderer.material;
		
		//Set to starting animation
		currentAnim = animations[startAnim];
		
		//
		currentSheet = spriteSheets[currentAnim.sheetNum];
		
		currentFrame = currentAnim.startFrame;
		
		//Start anim if ready
		if(playOnStart) Play();
		
		//Assign sprite, find width/height of frames, set sprite scale
		FrameData();
		
		//Set the texture offset to be friendly with our frames
		float offsetX = (frameWidth * (currentFrame % currentSheet.framesPerRow));
		float offsetY = (frameHeight*(rowNumber-1))+(currentFrame/currentSheet.framesPerRow * frameHeight);
		sprite.mainTextureOffset = new Vector2(offsetX,offsetY);
	}
	
	void FrameData()
	{			
		//Find frame width
		frameWidth = 1/(float)currentSheet.framesPerRow;
		
		//Find number of rows
		if(currentSheet.totalFrames%currentSheet.framesPerRow==0) rowNumber = currentSheet.totalFrames/currentSheet.framesPerRow;
		else rowNumber = ((int)(currentSheet.totalFrames/currentSheet.framesPerRow))+1;
		
		//Find frame height
		frameHeight = 1/(float)rowNumber;
		
		//Set the scale to be friendly with our frame height/width
		sprite.mainTextureScale = new Vector2(frameWidth,frameHeight);
	}
	
	// Update is called once per frame
	void FixedUpdate ()
	{
		Animate();
	}
	
	void Animate()
	{
		if(play)
		{
			//Homemade timer -> courtesy of David Laskey
			animTimer += Time.deltaTime;
			if(animTimer >= 60/currentAnim.frameRate*Time.deltaTime)
			{	
				//If loopable, then loop anim
				if(currentAnim.loop)
				{
					if(currentFrame == currentAnim.startFrame + currentAnim.animFrames)
						currentFrame = currentAnim.startFrame;
					else
						currentFrame++;
				}
				//Else, get to last frame and then stop
				else
				{
					if(currentFrame != currentAnim.startFrame + currentAnim.animFrames - 1)
						currentFrame++;
				}
				
				//Change to appropriate spritesheet
				ChangeSheet();
				
				if(!currentAnim.loop || currentFrame != currentAnim.startFrame + currentAnim.animFrames)
				{
					//Change texture
					float offsetX = frameWidth * (currentFrame % currentSheet.framesPerRow); //Move position.x of uv
					float offsetY = (frameHeight*(rowNumber-1))-(currentFrame/currentSheet.framesPerRow * frameHeight);//move position.y of uv
					sprite.mainTextureOffset = new Vector2(offsetX,offsetY);
				}
				
				//Set maintexture
				sprite.mainTexture = currentSheet.img;
				
				//Reset timer
				animTimer = 0;
			}
		}
	}
	
	public void Play()
	{
		play = true;
	}
	
	public void Pause()
	{
		play = false;
	}
	
	public void Stop()
	{
		//Reset animation
		currentFrame = currentAnim.startFrame;
		animTimer = 0;
		
		//Stop playing
		play = false;
	}
	
	
	public void ChangeAnim(string name)
	{
		if(currentAnim.animName != name)
		{
			//Cycle through each of the animations we've added in the inspector
			foreach(Anim anim in animations)
			{
				//If any match the name we're giving
				if(name == anim.animName)
				{					
					//Change to the corresponding animation
					currentAnim = anim;
					
					//Change to start fram
					currentFrame = currentAnim.startFrame;
					
					//Switch to starting frame of new animation
					animTimer = 61;
					break;
				}
			}
		}
	}
	
	public void ChangeSheet()
	{
		//Change sheet
		currentSheet = spriteSheets[currentAnim.sheetNum];
		
		//Find frame data for new spritesheet
		FrameData();
	}
}

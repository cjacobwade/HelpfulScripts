#ifndef SEETHROUGHUTILS_INCLUDED
#define SEETHROUGHUTILS_INCLUDED

// If you've never seen a cginclude file before, it's just a file that holds shader utilities which can then be
// dropped into another shader folder via #include "NameOfUtil.cginc"

// I use this file to define a bunch of global shader variables so it is much easier to add this SeeThroughWalls behaviour to other shaders
// Instead of having to copy/maintain code in each shader file, we keep it all here!

// $$$$$THEGOODSTUFF$$$$$$

// If you're interested in how this works, here's the part where I explain it!
// Check out SeeThroughUtils for the actual math stuff

// GOAL: don't draw (or clip, in shader terms) any pixels that would obscure our player (or a circle around them) in the eyes of the camera
// ALL WE NEED: potential pixel position, position of all targets, camera position

// Step 1: We check if pixel we're considering drawing is in front of the player. If it is, then it's possible we want to clip it.
// Step 2: We use the Dot product to check if the direction from the camera to the pixel is very close to the direction from the camera to the player. If it is then it's possible we want to clip it
// Step 3: If both of these conditions are met, we check if this material even wants to do the clipping. This step is optional. Now we clip!

// Add as many of these as you have targets!
float4 _Target0Pos;
float4 _Target1Pos;
//float4 _Target2Pos;
//float4 _Target3Pos;
//float4 _Target4Pos;
//float4 _Target5Pos;
//float4 _Target6Pos;
//float4 _Target7Pos;

float _XRayMinDot;

// This should just be part of CG so I'm leaving it in here for you lol
float inverseLerp(float min, float max, float value)
{
	return (value - min)/(max - min);
}

// This does the good math
float IsFragBlockingTarget(float4 targetPos, float4 fragPos)
{
	float3 toTarget = targetPos.xyz - _WorldSpaceCameraPos.xyz;
	float3 toFrag = fragPos.xyz - _WorldSpaceCameraPos.xyz;

	float distanceToTarget = length(toTarget);
	float distanceToFrag = length(toFrag);

	float isFragCloserThanTarget = step(distanceToFrag, distanceToTarget);
	float dotToTargetVSToFrag = dot(normalize(toTarget), normalize(toFrag));

	// If frag is farther than player, we return zero
	// Otherwise use this dot to tell how in the way the frag is of the camera
	return dotToTargetVSToFrag * isFragCloserThanTarget;
}

float IsFragBlockingAndScaleMinDot(float4 targetPos, float4 fragPos)
{
	float3 toTarget = targetPos.xyz - _WorldSpaceCameraPos.xyz;
	float appliedMinDot = _XRayMinDot; // Adjust this value based on the supplied toTarget vec to make the cut-away hole change in size based on the target's distance from the camera
	return step(appliedMinDot, IsFragBlockingTarget(targetPos, fragPos));
}

// This applies the good math for each potential target
float IsFragBlockingAnyTarget(float4 fragPos)
{
	// Add as many of these as you have targets!
	float isBlocking = IsFragBlockingAndScaleMinDot(_Target0Pos, fragPos);
	isBlocking += IsFragBlockingAndScaleMinDot(_Target1Pos, fragPos);
//	isBlocking += IsFragBlockingAndScaleMinDot(_Target2Pos, fragPos);
//	isBlocking += IsFragBlockingAndScaleMinDot(_Target3Pos, fragPos);
//	isBlocking += IsFragBlockingAndScaleMinDot(_Target4Pos, fragPos);
//	isBlocking += IsFragBlockingAndScaleMinDot(_Target5Pos, fragPos);
//	isBlocking += IsFragBlockingAndScaleMinDot(_Target6Pos, fragPos);
//	isBlocking += IsFragBlockingAndScaleMinDot(_Target7Pos, fragPos);
	return step(0.5, isBlocking);
}

#endif
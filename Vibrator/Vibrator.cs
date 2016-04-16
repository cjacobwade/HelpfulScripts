using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using InControl;
using System.Linq;

[System.Serializable]
public class Vibration
{
	public InputDevice inputDevice = null;
	public float time = 0.3f;
	public float leftStrength = 0.15f;
	public float rightStrength = 0.15f;

	public Vibration()
	{
		inputDevice = null;
		time = 0.3f;
		leftStrength = 0.15f;
		rightStrength = 0.15f;
	}

	public Vibration(InputDevice inInputDevice, Vibration inOtherVibration)
	{
		inputDevice = inInputDevice;
		time = inOtherVibration.time;
		leftStrength = inOtherVibration.leftStrength;
		rightStrength = inOtherVibration.rightStrength;
	}

	public Vibration(InputDevice inInputDevice, float inTime, float inLeftStrength, float inRightStrength)
	{
		inputDevice = inInputDevice;
		time = inTime;
		leftStrength = inLeftStrength;
		rightStrength = inRightStrength;
	}
}

public class Vibrator : Singleton<Vibrator>
{
	List<Vibration> _activeVibrations = new List<Vibration>();

	void Update()
	{
		for (int i = 0; i < _activeVibrations.Count; ++i)
		{
			_activeVibrations[i].time -= Time.deltaTime;

			if (_activeVibrations[i].time <= 0f)
			{
				_activeVibrations[i].inputDevice.StopVibration();
				_activeVibrations.RemoveAt(i--);
			}
		}
	}

	public void StopAllVibration()
	{
		for (int i = 0; i < _activeVibrations.Count; ++i)
		{
			_activeVibrations[i].inputDevice.StopVibration();
			_activeVibrations.RemoveAt(i--);
		}
	}

	public void Vibrate(InputDevice inInputDevice, float inLeftStrength, float inRightStrength, float inTime)
	{
		Vibrate(new Vibration(inInputDevice, inLeftStrength, inRightStrength, inTime));
	}

	public void Vibrate(InputDevice inInputDevice, float inStrength, float inTime)
	{
		Vibrate(new Vibration(inInputDevice, inStrength, inStrength, inTime));
	}

	public void Vibrate(Vibration inVibration)
	{
		Vibration activeVibration = _activeVibrations.Where(v => v.inputDevice == inVibration.inputDevice).FirstOrDefault();

		if (activeVibration == null)
		{
			activeVibration = inVibration;
			_activeVibrations.Add(activeVibration);
		}
		else
		{
			if (activeVibration.time < inVibration.time)
				activeVibration.time = inVibration.time;

			if (activeVibration.leftStrength < inVibration.leftStrength)
				activeVibration.leftStrength = inVibration.leftStrength;

			if (activeVibration.rightStrength < inVibration.rightStrength)
				activeVibration.rightStrength = inVibration.rightStrength;
		}

		activeVibration.inputDevice.Vibrate(inVibration.leftStrength, inVibration.rightStrength);
	}
}
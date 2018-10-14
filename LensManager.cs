using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Lens
{
	object _context;
	public object GetContext()
	{ return _context; }

	string _name;
	public string GetName()
	{ return _name; }

	protected Lens(object inContext, string name)
	{
		Debug.Assert(inContext != null, "Hold up. Invalid context passed to a lens handle.");

		_context = inContext;
		_name = string.IsNullOrEmpty(name) ? inContext.ToString() : name;
	}
}

public class Lens<T> : Lens
{
	public T value = default(T);
	public Lens(object inContext, T inValue, string name = null) : base(inContext, name)
	{
		value = inValue;
	}
}

public class LensManager<T>
{
	public LensManager(Func<List<T>, T> inEvaluateFunc)
	{
		Debug.Assert(inEvaluateFunc != null, "Lens Manager's evaluate func is null. This is not allowed");
		_evaluateFunc = inEvaluateFunc;
		EvaluateRequests();
	}

	protected T _cachedResult = default(T);
	public T GetCachedResult()
	{ return _cachedResult; }

	protected int _requestCount = 0;
	public int GetRequestCount()
	{ return _requestCount; }

	protected Func<List<T>, T> _evaluateFunc = null;
	List<Lens<T>> _activeRequests = new List<Lens<T>>();
	List<T> _evaluateValues = new List<T>();

	public Action<T> evaluateCallback = delegate{};

	public static implicit operator T(LensManager<T> inLensManager)
	{ return inLensManager._cachedResult; }

	public void EvaluateRequests()
	{
		_evaluateValues.Clear();

		for (int i = 0; i < _activeRequests.Count; i++)
		{
			if (_activeRequests[i].GetContext() == null)
			{
				_activeRequests.RemoveAt(i--);
			}
			else
			{
				_evaluateValues.Add(_activeRequests[i].value);
			}
		}

		_requestCount = _activeRequests.Count;
		_cachedResult = _evaluateFunc(_evaluateValues);

		evaluateCallback(_cachedResult);
	}

	public void AddRequest(Lens<T> handle)
	{
		if (!_activeRequests.Contains(handle))
		{
			_activeRequests.Add(handle);
			EvaluateRequests();
		}
	}

	public bool RemoveRequest(Lens<T> handle)
	{
		if (_activeRequests.Remove(handle))
		{
			EvaluateRequests();
			return true;
		}

		return false;
	}

	public bool RemoveRequestsWithContext(object inContext)
	{
		bool anyRemoved = false;
		for(int i = 0; i < _activeRequests.Count; i++)
		{
			if (_activeRequests[i].GetContext() == inContext)
			{
				_activeRequests.RemoveAt(i--);
				anyRemoved = true;
			}
		}

		if(anyRemoved)
			EvaluateRequests();

		return anyRemoved;
	}

	public void ClearRequests()
	{
		_activeRequests.Clear();
		EvaluateRequests();
	}
}

public static class LensUtils
{
	public static float Average(List<float> inRequests, float inDefault = 1f)
	{
		if (inRequests.Count > 0)
		{
			float sum = 0f;
			for (int i = 0; i < inRequests.Count; i++)
			{
				sum += inRequests[i];
			}
			return sum / inRequests.Count;
		}
		else
		{
			return inDefault;
		}
	}

	public static int Average(List<int> inRequests, int inDefault = 1)
	{
		if (inRequests.Count > 0)
		{
			int sum = 0;
			for (int i = 0; i < inRequests.Count; i++)
			{
				sum += inRequests[i];
			}
			return sum / inRequests.Count;
		}
		else
		{
			return inDefault;
		}
	}

	public static float Max(List<float> inRequests, float inDefault = 1f)
	{
		if (inRequests.Count > 0)
		{
			float max = 0f;
			for (int i = 0; i < inRequests.Count; i++)
			{
				max = Mathf.Max(max, inRequests[i]);
			}
			return max;
		}
		else
		{
			return inDefault;
		}
	}

	public static int Max(List<int> inRequests, int inDefault = 1)
	{
		if (inRequests.Count > 0)
		{
			int max = 0;
			for (int i = 0; i < inRequests.Count; i++)
			{
				max = Mathf.Max(max, inRequests[i]);
			}
			return max;
		}
		else
		{
			return inDefault;
		}
	}

	public static float Min(List<float> inRequests, float inDefault = 1f)
	{
		if (inRequests.Count > 0)
		{
			float min = 0f;
			for (int i = 0; i < inRequests.Count; i++)
			{
				min = Mathf.Min(min, inRequests[i]);
			}
			return min;
		}
		else
		{
			return inDefault;
		}
	}

	public static int Min(List<int> inRequests, int inDefault = 1)
	{
		if (inRequests.Count > 0)
		{
			int min = 0;
			for (int i = 0; i < inRequests.Count; i++)
			{
				min = Mathf.Min(min, inRequests[i]);
			}
			return min;
		}
		else
		{
			return inDefault;
		}
	}

	public static bool AllTrue(List<bool> inRequests, bool inDefault = true)
	{
		if (inRequests.Count > 0)
		{
			for (int i = 0; i < inRequests.Count; i++)
			{
				if(!inRequests[i]) return false;
			}
			return true;
		}
		else
		{
			return inDefault;
		}
	}

	public static bool AllFalse(List<bool> inRequests, bool inDefault = false)
	{
		if (inRequests.Count > 0)
		{
			for (int i = 0; i < inRequests.Count; i++)
			{
				if (inRequests[i]) return false;
			}
			return true;
		}
		else
		{
			return inDefault;
		}
	}

	public static bool AnyTrue(List<bool> inRequests, bool inDefault = false)
	{
		if (inRequests.Count > 0)
		{
			for (int i = 0; i < inRequests.Count; i++)
			{
				if (inRequests[i]) return true;
			}
			return false;
		}
		else
		{
			return inDefault;
		}
	}

	public static bool AnyFalse(List<bool> inRequests, bool inDefault = true)
	{
		if (inRequests.Count > 0)
		{
			for (int i = 0; i < inRequests.Count; i++)
			{
				if (!inRequests[i]) return true;
			}
			return false;
		}
		else
		{
			return inDefault;
		}
	}
}

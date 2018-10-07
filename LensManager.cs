using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using LinqTools;

public class LensHandle
{
	object _context;
	public object GetContext()
	{ return _context; }

	string _name;
	public string GetName()
	{ return _name; }

	public LensHandle(object inContext)
	{
		Debug.Assert(inContext != null, "Hold up. Invalid context passed to a lens handle.");

		_context = inContext;
		_name = inContext.ToString();
	}
}

public class LensHandle<T> : LensHandle
{
	public T value = default(T);
	public LensHandle(object inContext, T inValue) : base(inContext)
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
	List<LensHandle<T>> _activeRequests = new List<LensHandle<T>>();

	public Action<T> evaluateCallback = delegate{};

	public static implicit operator T(LensManager<T> inLensManager)
	{ return inLensManager._cachedResult; }

	public void EvaluateRequests()
	{
		for (int i = 0; i < _activeRequests.Count; i++)
		{
			if (_activeRequests[i].GetContext() == null)
			{
				_activeRequests.RemoveAt(i--);
			}
		}

		_requestCount = _activeRequests.Count;
		_cachedResult = _evaluateFunc(_activeRequests.Select(r => (T)r.value).ToList());

		evaluateCallback(_cachedResult);
	}

	public void AddRequest(LensHandle<T> inHandle)
	{ AddRequests(inHandle); }

	public void AddRequests(params LensHandle<T>[] inHandles)
	{ AddRequestsEnumerable(inHandles); }

	void AddRequestsEnumerable(IEnumerable<LensHandle<T>> inHandles)
	{
		Debug.Assert(inHandles != null, "Request Enumerable is null. This is not supported.");

		var enumerator = inHandles.GetEnumerator();
		while (enumerator.MoveNext())
		{
			var handle = enumerator.Current;
			if (handle != null && !_activeRequests.Contains(handle))
				_activeRequests.Add(handle);
		}

		EvaluateRequests();
	}

	public bool RemoveRequest(LensHandle<T> inHandle)
	{ return RemoveRequests(inHandle); }

	public bool RemoveRequests(params LensHandle<T>[] inHandles)
	{ return RemoveRequestsEnumerable(inHandles); }

	bool RemoveRequestsEnumerable(IEnumerable<LensHandle<T>> inHandles)
	{
		Debug.Assert(inHandles != null, "Request Enumerable is null. This is not supported.");

		bool anyRemoved = false;
		var enumerator = inHandles.GetEnumerator();
		while (enumerator.MoveNext())
		{
			var handle = enumerator.Current;
			if (_activeRequests.Contains(handle))
			{
				_activeRequests.Remove(handle);
				anyRemoved = true;
			}
		}

		EvaluateRequests();
		return anyRemoved;
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
	// AVERAGE
	public static float Average(List<float> inRequests, float inDefault = 1f)
	{ return inRequests.Count > 0 ? inRequests.Average(r => r) : inDefault; }

	public static int Average(List<int> inRequests, int inDefault = 1)
	{ return inRequests.Count > 0 ? (int)inRequests.Average(r => r) : inDefault; }

	// MAX
	public static float Max(List<float> inRequests, float inDefault = 1f)
	{ return inRequests.Count > 0 ? inRequests.Max(r => r) : inDefault; }

	public static int Max(List<int> inRequests, int inDefault = 1)
	{ return inRequests.Count > 0 ? (int)inRequests.Max(r => r) : inDefault; }

	// MIN
	public static float Min(List<float> inRequests, float inDefault = 1f)
	{ return inRequests.Count > 0 ? inRequests.Min(r => r) : inDefault; }

	public static int Min(List<int> inRequests, int inDefault = 1)
	{ return inRequests.Count > 0 ? (int)inRequests.Min(r => r) : inDefault; }

	// BOOLS
	public static bool AllTrue(List<bool> inRequests, bool inDefault = true)
	{ return inRequests.Count > 0 ? inRequests.All(r => r) : inDefault; }

	public static bool AllFalse(List<bool> inRequests, bool inDefault = false)
	{ return inRequests.Count > 0 ? inRequests.All(r => !r) : inDefault; }

	public static bool AnyTrue(List<bool> inRequests, bool inDefault = false)
	{ return inRequests.Count > 0 ? inRequests.Any(r => r) : inDefault; }

	public static bool AnyFalse(List<bool> inRequests, bool inDefault = true)
	{ return inRequests.Count > 0 ? inRequests.Any(r => !r) : inDefault; }

	// CUSTOM
	public static CursorLockMode MouseCursor(List<CursorLockMode> inRequests, CursorLockMode inDefault = CursorLockMode.Locked)
	{
		if (inRequests.Count > 0)
		{
			for(int i = 0; i < inRequests.Count; i++)
			{
				if (inRequests[i] == CursorLockMode.None)
					return CursorLockMode.None;
			}

			for (int i = 0; i < inRequests.Count; i++)
			{
				if (inRequests[i] == CursorLockMode.Confined)
					return CursorLockMode.Confined;
			}

			return CursorLockMode.Locked;
		}
		else
			return inDefault;
	}
}

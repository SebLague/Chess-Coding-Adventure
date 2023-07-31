using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

[ExecuteAlways]
public abstract class ImmediateAnimation : MonoBehaviour
{


	[Header("Animator")]
	public float animTime;
	public bool useNormTime;
	[Range(0, 1)] public float timeNorm;
	public float duration;
	public float playbackSpeed = 1;

	public delegate float EaseFunction(float t);
	public delegate void WaitFunction(float t = 1);

	protected float globalStartTime { get; private set; }
	protected float globalEndTime { get; private set; }
	float globalDelay;
	protected bool isPaused { get; private set; }

	Dictionary<Key, float> pressedKeyLookup;
	HashSet<Key> keysToListenFor;

	protected virtual void OnEnable()
	{
		if (Application.isPlaying)
		{
			animTime = 0;
		}
		pressedKeyLookup = new Dictionary<Key, float>();
		keysToListenFor = new HashSet<Key>();
	}


	protected virtual void Update()
	{
		Init();
		UpdateInput();
		UpdateAnimation();
		UpdateTime();
	}


	protected abstract void UpdateAnimation();

	void Init()
	{

		globalStartTime = 0;
		globalEndTime = 0;
		globalDelay = 0;
	}

	void UpdateInput()
	{
		if (!Application.isPlaying)
		{
			return;
		}

		Keyboard keyboard = Keyboard.current;
		foreach (Key key in keysToListenFor)
		{
			if (keyboard[key].wasPressedThisFrame)
			{
				if (pressedKeyLookup.ContainsKey(key))
				{
					pressedKeyLookup[key] = animTime;
				}
				else
				{
					pressedKeyLookup.Add(key, animTime);
				}
			}
		}
	}

	public void SetPlaybackTime(float time)
	{
		animTime = time;
	}

	void UpdateTime()
	{

		//animTime = Mathf.Lerp(globalStartTime, globalEndTime, timeNorm);

		if (Application.isPlaying)
		{
			if (!isPaused)
			{
				animTime += Time.deltaTime * playbackSpeed;
			}
			useNormTime = false;

		}
		else
		{
			if (useNormTime)
			{
				animTime = Mathf.Lerp(globalStartTime, globalEndTime, timeNorm);
			}
		}
		animTime = Mathf.Max(animTime, globalStartTime);
		timeNorm = Mathf.InverseLerp(globalStartTime, globalEndTime, animTime);
		//timeNorm = Mathf.InverseLerp(globalStartTime, globalEndTime, animTime);

		duration = globalEndTime - globalStartTime;
	}

	public void Wait(float duration)
	{
		globalDelay += duration;
	}

	float CalculateTime(float clipDuration, EaseFunction ease, WaitUntil wait)
	{
		float startTime = globalDelay;
		float endTime = startTime + clipDuration;

		float rawT = (animTime - startTime) / (endTime - startTime);
		float t = Ease(rawT, ease);

		// Update delay for subsequent clips based on wait parameter
		globalDelay += wait.GetWaitDuration(clipDuration);
		// Keep track of overall animation start/end time
		globalStartTime = Mathf.Min(globalStartTime, startTime);
		globalEndTime = Mathf.Max(globalEndTime, endTime);

		return t;
	}

	float Ease(float t, EaseFunction ease)
	{
		return (ease == null) ? Mathf.Clamp01(t) : ease.Invoke(t);
	}

	public bool Trigger()
	{
		float triggerTime = 0 + globalDelay;
		return animTime >= triggerTime;
	}

	public void InputActivatedTime(AnimFloat current, float duration, Key key, EaseFunction ease = null)
	{
		if (!Application.isPlaying)
		{
			current.UpdateValue(0);
			return;
		}

		keysToListenFor.Add(key);

		if (pressedKeyLookup.TryGetValue(key, out float lastPressedTime))
		{
			float t = Ease(animTime - lastPressedTime, ease);
			current.UpdateValue(t);
		}
	}

	public void AnimateTime(AnimFloat current, float duration, EaseFunction ease = null, WaitUntil wait = default)
	{
		AnimateValue(current, 0, 1, duration, ease, wait);
	}

	public void AnimateValue(AnimFloat current, float start, float end, float duration, EaseFunction ease = null, WaitUntil wait = default)
	{
		float t = CalculateTime(duration, ease, wait);

		if (t > 0 || current.lastModifiedFrame != Time.frameCount)
		{
			current.UpdateValue(Mathf.Lerp(start, end, t));
		}
	}

	public void PausePlayback()
	{
		isPaused = true;
	}

	public void UnPausePlayback()
	{
		isPaused = false;
	}

	public void TogglePlaybackPaused()
	{
		isPaused = !isPaused;
	}


	public float SubdivideTime(float parentTime, int num, int currentIndex, float durationFraction, EaseFunction ease = null)
	{
		float lastChildStartTime = 1 - durationFraction;
		float indexT = currentIndex / (num - 1f);
		float startTime = lastChildStartTime * indexT;
		float t = Mathf.InverseLerp(startTime, startTime + durationFraction, parentTime);
		float easeT = (ease == null) ? Mathf.Clamp01(t) : ease.Invoke(t);
		return easeT;
	}

	public struct WaitUntil
	{
		public enum WaitType
		{
			DontWait,
			UntilFinished,
			UntilFinishedPlusDelay,
			UntilFractionFinished,
			ConstantDelay
		}

		WaitType waitType;
		float waitParameter;

		public WaitUntil(WaitType waitType, float waitParameter = 0)
		{
			this.waitType = waitType;
			this.waitParameter = waitParameter;
		}

		public static WaitUntil Finished()
		{
			return new WaitUntil(WaitType.UntilFinished);
		}

		public static WaitUntil FinishedPlusDelay(float delay)
		{
			return new WaitUntil(WaitType.UntilFinishedPlusDelay, delay);
		}

		public static WaitUntil FractionFinished(float fraction)
		{
			return new WaitUntil(WaitType.UntilFractionFinished, fraction);
		}

		public static WaitUntil Delay(float delay)
		{
			return new WaitUntil(WaitType.ConstantDelay, delay);
		}

		public float GetWaitDuration(float animationDuration)
		{
			switch (waitType)
			{
				case WaitType.UntilFinished:
					return animationDuration;
				case WaitType.UntilFinishedPlusDelay:
					return animationDuration + waitParameter;
				case WaitType.UntilFractionFinished:
					return animationDuration * waitParameter;
				case WaitType.ConstantDelay:
					return waitParameter;
				default:
					return 0;
			}

		}
	}


}

public class AnimFloat
{

	public float value { get; private set; }
	public int lastModifiedFrame { get; private set; }

	public AnimFloat(float value)
	{
		this.value = value;
	}

	public AnimFloat()
	{
	}

	public void UpdateValue(float newValue)
	{
		value = newValue;
		lastModifiedFrame = Time.frameCount;
	}

	public static implicit operator float(AnimFloat f) => f.value;
	public static explicit operator AnimFloat(float f) => new AnimFloat(f);
}


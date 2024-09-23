using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Слой, создаваемый внутри аудиоменеджера. Внутри слоя смешиваются аудиосорсы.
/// </summary>
public class AudioLayer
{
	private AudioLayerType _type;
	private List<OscClip> _clips = new List<OscClip> ();
	// Клип, громкость которого будет увеличиваться до единицы. Громкость всех остальных клипов будет уменьшаться до нуля. Если поле установлено в null, затухать будут все клипы.
	private OscClip _targetClip;
	// Скорость, с которой будет происходить нарастание громкости целевого клипа. Значение '2' соответствует переходу в полсекунды.
	private float _introSpeed = 1f;
	// Скорость, с которой будет происходить затухание громкости всех нецелевых клипов.
	private float _outroSpeed = 1f;


	public AudioLayer (AudioLayerType type)
	{
		_type = type;
	}

	public void Update (float deltaTime)
	{
		int numClips = _clips.Count;
		for (int i = 0; i < numClips; i++)
		{
			OscClip curClip = _clips[i];

			// Вес целевого клипа стремится к единице.
			if (curClip == _targetClip)
			{
				if (curClip.weight < 1)
				{
					curClip.weight += _introSpeed * deltaTime;
					// Проверка на out of range производится в сеттере curClip.weight.
				}

				// Если осц-клип не был зациклен (loop == false), при завершении удаляем его со слоя.
				if (!curClip.audioSource.loop && !curClip.audioSource.isPlaying)
				{
					MonoBehaviour.Destroy (curClip.audioSource);
					_clips.RemoveAt (i);
					i--;
					numClips--;
					_targetClip = null;
				}
			}
			// Веса всех нецелевых клипов стремятся к нулю.
			// Все нецелевые клипы (и незацикленные тоже) рано или поздно будут удалены и уничтожены, поэтому здесь такую проверку не производим.
			else
			{
				curClip.weight -= _outroSpeed * deltaTime;

				// Если вес клипа становится равен нулю, останавливаем его и удаляем из слоя.
				if (Mathf.Approximately (curClip.weight, 0))
				{
					curClip.audioSource.Stop ();
					MonoBehaviour.Destroy (curClip.audioSource);

					_clips.RemoveAt (i);
					i--;
					numClips--;
				}
			}
		}
	}

	/// <summary>
	/// Вводит в слой клип clip: делает его целевым и добавляет на слой, если необходимо. Если клип уже был на слое, его громкость с этого момента начнет расти со скоростью introSpeed, а громкость всех остальных клипов - уменьшаться со скоростью outroSpeed.
	/// </summary>
	/// <param name="clipSource"></param>
	//public void IntroduceClipOld (AudioSource clipSource)
	//{
	//	OscClip usedClip = GetClipWithSource (clipSource);
	//	if (usedClip == null)
	//	{
	//		usedClip = new OscClip (clipSource);
	//	}
	//	IntroduceClipPrivate (usedClip);
	//}

	public OscClip IntroduceClip (OscAudioResource resource, float loudness = -1)
	{
		OscClip usedClip = GetClipWithResourceType (resource.type);
		if (usedClip == null)
		{
			usedClip = loudness < 0 ? new OscClip (resource) : new OscClip (resource, loudness);
			usedClip.weight = 0;
			_clips.Add (usedClip);
		}
		else
		{
			// Если громкость имеющегося клипа изменилась, перепад будет слышен сразу же. Делаю на всякий случай.
			if (loudness >= 0)
				usedClip.loudness = loudness;
		}
		if (!usedClip.audioSource.isPlaying)
			usedClip.audioSource.Play ();

		_targetClip = usedClip;

		//IntroduceClipPrivate (usedClip);

		return usedClip;
	}

	// loudness < 0 для стандартной громкости клипа.
	public OscClip IntroduceClip (OscAudioResourceEnum resourceType, float loudness = -1)
	{
		OscAudioResource resource = GlobalManager.instance.audioManager.GetResource (resourceType);
		return IntroduceClip (resource, loudness);
	}

	public OscClip IntroduceClip (OscAudioResourceEnum resourceType, bool loop, float loudness = -1)
	{
		OscAudioResource resource = GlobalManager.instance.audioManager.GetResource (resourceType);
		OscClip clip = IntroduceClip (resource, loudness);
		clip.loop = loop;
		return clip;
	}

	/// <summary>
	/// Вводит в слой клип clip: делает его целевым и добавляет на слой, если необходимо. Если клип уже был на слое, его громкость с этого момента начнет расти со скоростью introSpeed, а громкость всех остальных клипов - уменьшаться со скоростью outroSpeed.
	/// </summary>
	/// <param name="clip"></param>
	private void IntroduceClipPrivate (OscClip clip)
	{
		int index = _clips.IndexOf (clip);
		// Если клипа еще не было на слое.
		if (index < 0)
		{
			_clips.Add (clip);
			clip.weight = 0;

			if (!clip.audioSource.isPlaying)
				clip.audioSource.Play ();
		}
		// Если клип был на слое, проверяем, не завершился ли он. Если завершился, воспроизводим.
		else
		{
			if (!clip.audioSource.isPlaying)
				clip.audioSource.Play ();
		}

		_targetClip = clip;
	}

	/// <summary>
	/// Зануляет ссылку на целевой клип. Все клипы с этого момента будут затухать.
	/// </summary>
	public void FadeOut ()
	{
		_targetClip = null;
	}

	// [Deprecated].
	public OscClip GetClipWithSource (AudioSource source)
	{
		int numClips = _clips.Count;
		for (int i = 0; i < numClips; i++)
		{
			OscClip curClip = _clips[i];
			if (curClip.audioSource == source)
			{
				return curClip;
			}
		}

		return null;
	}

	public OscClip GetClipWithResourceType (OscAudioResourceEnum resourceType)
	{
		int numClips = _clips.Count;
		for (int i = 0; i < numClips; i++)
		{
			OscClip curClip = _clips[i];
			if (curClip.resource.type == resourceType)
			{
				return curClip;
			}
		}
		return null;
	}


	// ==================== PROPERTIES ====================

	public AudioLayerType type { get { return _type; } }

	public float introSpeed
	{
		get { return _introSpeed; }
		set { _introSpeed = value; }
	}

	public float outroSpeed
	{
		get { return _outroSpeed; }
		set { _outroSpeed = value; }
	}

	public int numClips { get { return _clips.Count; } }

}
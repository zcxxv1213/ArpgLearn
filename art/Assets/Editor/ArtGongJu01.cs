#region 脚本说明
/*----------------------------------------------------------------
// 脚本作用：特效制作辅助工具1.4
// 创建者：黑仔
//----------------------------------------------------------------*/
#endregion
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

public class ArtGongJu01 : EditorWindow 
{
	private bool setFbxOpen = false;

	//-------------------特效预览---------------------
	private List<Animator> animatorChilds = new List<Animator>();
	private List<float> zongJiLuTimeS = new List<float>();
	private List<int> psCounts = new List<int>();
	private List<int> meshTrians = new List<int>();

	private bool animatorHongPei = false;

	private static bool openPreview = false;
	private bool zaiRu = false;
	private GameObject dqAsset;
	private GameObject dqEffect;
	private GameObject dqEffectHierarchy;
	private GameObject dqPrefab;
	private bool isInspectorUpdate = false;
	private bool simulatePSOne = false;
	private bool lockRoot = true;

	private int shiLieHua = 0;
	/// <summary>
	/// 最大粒子数
	/// </summary>
	private int psCount = 0;
	/// <summary>
	/// 最大模型面数
	/// </summary>
	private int meshTrian = 0;

	/// <summary>
	/// 时间增量
	/// </summary>
	private double delta;

	/// <summary>
	/// 当前运行时间
	/// </summary>
	private float m_RunningTime;
	/// <summary>
	/// 滑动条时间
	/// </summary>
	private float m_HuaDongTiao;
	/// <summary>
	/// 上一次系统时间
	/// </summary>
	private double m_PreviousTime;

	/// <summary>
	/// 粒子的最长存活
	/// </summary>
	private float psTime = 0.0f;

	/// <summary>
	/// 最大时间长度	
	/// </summary>
	private float aniTime = 0.0f;

	/// <summary>
	/// 用来采样animation
	/// </summary>
	private	float ani_time = 0.0f;
	/// <summary>
	/// 播放模式下时间控制
	/// </summary>
	private float playingTime = 0.0f;
	private float runPlayingTime = 0.0f;
	private bool isPlayingTime = false;

	private Vector2 scrollPos = new Vector2();

	//-----------------------------------------------------

	//-------------------预览当前特效-----------------------
	private bool openEffectAniPlay = false;
	private bool effectAniPlayLock = false;
	private bool effectAniPlayPause = false;
	private bool effectAniPlayLoop = true;
	private bool effectHierarchyUpdate = true;
	//----------------------------------------------------

	//----------------------------------------特效发射------------------------------------------------
	private bool effectFaShe = false;
	private GameObject ziDan;
	private float ziDanLife;
	private float ziDanSpeed = 20.0f;
	//------------------------------------------------------------------------------------------------

	//----------------------------------------设置特效粒子最大粒子数----------------------------------
	private SetMaxParticles setMaxParS;
	//------------------------------------------------------------------------------------------------

	[MenuItem("HZTools/特效制作辅助1.4")]
	static void AddArtGongJu01()
	{
		openPreview = true;
		GetWindow<ArtGongJu01>("特效制作辅助1.4");
	}

	void OnEnable()
	{
		m_PreviousTime = EditorApplication.timeSinceStartup;
		EditorApplication.update += inspectorUpdate;
		isInspectorUpdate = true;
	}

	void OnGUI()
	{
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

		EditorGUILayout.Space();

		EditorGUI.BeginChangeCheck();
		EditorGUILayout.Toggle("开启资源预览（Project）", openPreview);
		if (EditorGUI.EndChangeCheck())
		{
			psCount = 0;
			meshTrian = 0;
			openPreview = !openPreview;
			if (openPreview && !isInspectorUpdate) 
			{
				OnEnable();
			}
			if (openPreview) 
			{
				m_RunningTime = 0;
				EffectUpdate();
				openEffectAniPlay = false;
				dqEffectHierarchy = null;
				effectAniPlayLock = false;
				effectAniPlayPause = false;
				effectAniPlayLoop = true;
			} else {
				if (dqEffect != null && shiLieHua > 0) 
				{
					DestroyImmediate(dqEffect);
					zaiRu = false;
				}
			}
			dqPrefab = null;
			OnSelectionChange();
		}


		EditorGUILayout.BeginHorizontal();
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.Toggle("开启当前特效预览（Hierarchy）", openEffectAniPlay);
		if (EditorGUI.EndChangeCheck()) 
		{
			lockRoot = true;
			psCount = 0;
			meshTrian = 0;
			m_RunningTime = 0;
			EffectUpdate();
			openEffectAniPlay = !openEffectAniPlay;
			if (!openEffectAniPlay) 
			{
				dqEffectHierarchy = null;
				dqPrefab = null;
				effectAniPlayLock = false;
				effectAniPlayPause = false;
				effectAniPlayLoop = true;
			}
			if (openEffectAniPlay && !isInspectorUpdate) 
			{
				OnEnable();
			}
			if (openEffectAniPlay) 
			{
				openPreview = false;
				effectAniPlayLoop = true;
				if (dqEffect != null && shiLieHua > 0) 
				{
					DestroyImmediate(dqEffect);
					zaiRu = false;
				}
			}
			OnSelectionChangeHierarchy();
		}

		EditorGUI.BeginChangeCheck();
		if (openEffectAniPlay && dqPrefab != null) 
		{
			GUILayout.Toggle(effectAniPlayLock, "锁定选择", EditorStyles.toolbarButton);
			if (EditorGUI.EndChangeCheck())
			{
				effectAniPlayLock = !effectAniPlayLock;
				if (!effectAniPlayLock) 
				{
					effectAniPlayPause = false;
					effectAniPlayLoop = true;
				}
			}
		}
		if (effectAniPlayLock && !effectAniPlayPause) 
		{
			EditorGUI.BeginChangeCheck();
			GUILayout.Toggle(effectAniPlayLoop, "循环", EditorStyles.toolbarButton);
			if (EditorGUI.EndChangeCheck())
			{
				effectAniPlayLoop = !effectAniPlayLoop;
			}
		}
		if (effectAniPlayLock && !Application.isPlaying) 
		{
			EditorGUI.BeginChangeCheck();
			GUILayout.Toggle(effectAniPlayPause, "暂停", EditorStyles.toolbarButton);
			if (EditorGUI.EndChangeCheck())
			{
				effectAniPlayPause = !effectAniPlayPause;
				effectAniPlayLoop = true;
			}
		}
		if (!effectAniPlayLoop && Application.isPlaying) 
		{
			if (GUILayout.Button("播放"))
			{
				PlayingAllAniPlay();
			}
		}
		if (!effectAniPlayLoop && !Application.isPlaying) 
		{
			if (GUILayout.Button("播放"))
			{
				m_RunningTime = 0;
				effectHierarchyUpdate = true;
				effectAniPlayPause = false;
			}
		}
		EditorGUILayout.EndHorizontal();

		if (effectAniPlayLock) 
		{
			EditorGUILayout.ObjectField("当前锁定的：", dqEffectHierarchy, typeof(GameObject), false);
		}

		if (effectAniPlayPause && !Application.isPlaying) 
		{
			m_RunningTime = m_HuaDongTiao;
			float startTime = 0.0f;
			float stopTime  = playingTime;
			m_HuaDongTiao = EditorGUILayout.Slider(m_HuaDongTiao, startTime, stopTime);
		}

		if (!effectAniPlayLoop) 
		{
			if (GUILayout.Button("只播放当前选中的粒子"))
			{
				effectAniPlayPause = false;
				PlayingOneAniPlay();
			}
		}
		EditorGUILayout.BeginHorizontal();
		if (openPreview || openEffectAniPlay) 
		{
			if (Application.isPlaying) 
			{
				EditorGUI.BeginChangeCheck();
				GUILayout.Toggle(effectFaShe, "发射特效", EditorStyles.toolbarButton);
				if (EditorGUI.EndChangeCheck())
				{
					effectFaShe = !effectFaShe;
				}
				ziDanSpeed = EditorGUILayout.FloatField("子弹速度：", ziDanSpeed);
			}
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.LabelField("当前特效的最大粒子数： " + psCount.ToString());
		EditorGUILayout.LabelField("当前特效大概的三角面数： " + ((meshTrian / 3) + (psCount * 2)).ToString());
		if (openEffectAniPlay && Application.isPlaying) 
		{
			if (GUILayout.Button("保存选中的预设"))
			{
				SavePrefab();
			}
		}
		if (openEffectAniPlay) 
		{
			if (GUILayout.Button("延迟归零")) 
			{
				PSDelayToZero();
			}
		}
		var zhuangTai = "";
		if (Application.isPlaying) 
		{
			zhuangTai = "当前是在播放状态下预览！你可以在播放模式下保存你当前预览的特效，前提是它已经保存过预设。";
		}
		else {
			zhuangTai = "当前是在编辑状态下预览！由于Unity5.0以上某些版本的bug，粒子里用了延迟时会减少粒子的生命。也就是当延迟时间大于粒子生命周期时，将看不见粒子了。在播放模式下可以正常预览。";
		}
		EditorGUILayout.HelpBox(zhuangTai, MessageType.Info);
		if (openEffectAniPlay) 
		{
			lockRoot = EditorGUILayout.Toggle("只能选择父集：", lockRoot);
		}
		EditorGUILayout.Space();
		if (Application.isPlaying && openEffectAniPlay && dqPrefab != null && setMaxParS != null) 
		{
			setMaxParS.Gui(dqPrefab, psCount);
		}

		EditorGUILayout.EndScrollView();
	}

	//------------------------特效预览------------------------------------------

	void OnDestroy()
	{
		openPreview = false;
		openEffectAniPlay = false;
		if (dqEffect != null) 
		{
			DestroyImmediate(dqEffect);
			animatorHongPei = false;
			zaiRu = false;
			effectFaShe = false;
		}
		EditorApplication.update -= inspectorUpdate;
		isInspectorUpdate = false;
	}

	void YouHuaJianCha(GameObject go)
	{
		meshTrians.Clear();
		psCounts.Clear();
		var meshs = go.GetComponentsInChildren<MeshFilter>();
		foreach (var mesh in meshs) 
		{
			if (mesh.sharedMesh != null) 
			{
				meshTrians.Add(mesh.sharedMesh.triangles.Length);
			} 
		}
		var psS = go.GetComponentsInChildren<ParticleSystem>();
		foreach (var ps in psS) 
		{
			var psConut = ps.GetParticles(new ParticleSystem.Particle[ps.maxParticles]);
			if (ps.GetComponent<ParticleSystemRenderer>().renderMode == ParticleSystemRenderMode.Mesh && ps.GetComponent<ParticleSystemRenderer>().mesh != null) 
			{
				meshTrians.Add(ps.GetComponent<ParticleSystemRenderer>().mesh.triangles.Length * psConut);
			}
			var psNum = ps.GetParticles(new ParticleSystem.Particle[ps.maxParticles]);
			psCounts.Add(psNum);
		}
		var skinMeshs = go.GetComponentsInChildren<SkinnedMeshRenderer>();
		foreach (var mesh in skinMeshs) 
		{
			if (mesh.sharedMesh != null) 
			{
				meshTrians.Add(mesh.sharedMesh.triangles.Length);
			}
		}

		if (meshTrians.Count > 0) 
		{
			var psNum = 0;
			foreach (var item in meshTrians) 
			{
				psNum = psNum + item;
			}
			if (meshTrian < psNum) 
			{
				meshTrian = psNum;
			}
		}


		if (psCounts.Count > 0) 
		{
			var psNum = 0;
			foreach (var item in psCounts) 
			{
				psNum = psNum + item;
			}
			if (psCount < psNum) 
			{
				psCount = psNum;
			}
		}
		Repaint();
	}

	void update()
	{
		if (Application.isPlaying)
		{
			return;
		}

		if (openPreview && zaiRu && dqAsset != null) 
		{
			if (animatorHongPei) 
			{
				for (int i = 0; i < animatorChilds.Count; i++) 
				{
					if (animatorChilds[i] != null) 
					{
						if (animatorChilds[i].GetCurrentAnimatorStateInfo(0).loop) 
						{
							if (zongJiLuTimeS[i] < animatorChilds[i].GetCurrentAnimatorStateInfo(0).length) 
							{
								animatorChilds[i].playbackTime = (m_RunningTime % zongJiLuTimeS[i]);
							} else {
								animatorChilds[i].playbackTime = (m_RunningTime % animatorChilds[i].GetCurrentAnimatorStateInfo(0).length);
							}
						}
						else 
						{
							if (m_RunningTime <= zongJiLuTimeS[i] && m_RunningTime <= animatorChilds[i].GetCurrentAnimatorStateInfo(0).length) 
							{
								animatorChilds[i].playbackTime = m_RunningTime;
							}
							if (m_RunningTime >= aniTime && m_RunningTime >= psTime) 
							{
								m_RunningTime = 0.0f;
							}
						}
						animatorChilds[i].Update (0);
					}
				}
			}
			if (!animatorHongPei) 
			{
				AnimaterHongPei();
			}
			PSPlayAni();
			AnimationPlay();
		}

		if (!openEffectAniPlay && !openPreview && dqEffect != null) 
		{
			DestroyImmediate(dqEffect);
			animatorHongPei = false;
			zaiRu = false;
			EditorApplication.update -= inspectorUpdate;
			isInspectorUpdate = false;
		}

		EffectUpdate();
		SimulatePSAniPlay();
	}

	void Update()
	{
		if (!Application.isPlaying && !openPreview && !openEffectAniPlay) 
		{
			if (isInspectorUpdate) 
			{
				EditorApplication.update -= inspectorUpdate;
				isInspectorUpdate = false;
			}
			return;
		}
		GetdqPrefab();

		if (dqEffect != null) 
		{
			YouHuaJianCha(dqEffect);
		}
		if (dqEffectHierarchy != null) 
		{
			YouHuaJianCha(dqEffectHierarchy);
		}

		if (!Application.isPlaying) 
		{
			GetPlayingTime();
			if (EditorApplication.isPlayingOrWillChangePlaymode) //运行播放前判断
			{
				if (dqEffect != null) 
				{
					DestroyImmediate(dqEffect);
					openPreview = false;
				}
				if (dqEffectHierarchy != null) 
				{
					dqEffectHierarchy = null;
					openEffectAniPlay = false;
					effectAniPlayLock = false;
					effectAniPlayPause = false;
				}
			}
			return;
		}


		if (setMaxParS != null && dqPrefab != null) 
		{
			setMaxParS.update(dqPrefab, psCount);
		}

		if (dqPrefab != null && !isPlayingTime && playingTime > 0) 
		{
			runPlayingTime = playingTime;
			if (!isPlayingTime && dqPrefab!= null && runPlayingTime > 0.0f) 
			{
				PlayingAllAniPlay();
			}
		}
		if (isPlayingTime && effectAniPlayLoop) 
		{
			runPlayingTime = runPlayingTime - Time.deltaTime;
			if (runPlayingTime < 0) 
			{
				isPlayingTime = false;
				playingTime = 0;
			}
		}
		GetPlayingTime();
		EffectFaSheUpdate();
	}

	void OnSelectionChange()
	{
		if (openPreview && shiLieHua > 0) 
		{
			DestroyImmediate(dqEffect);
			zaiRu = false;
		}

		if (openPreview) 
		{
			dqEffectHierarchy = null;
			dqAsset = Selection.activeGameObject;
			animatorHongPei = false;
			JiaZaiEffect();
		}

		if (openEffectAniPlay) 
		{
			OnSelectionChangeHierarchy();
		}
	}

	void JiaZaiEffect()
	{
		if (openPreview && dqAsset != null && AssetDatabase.Contains(dqAsset) && dqAsset.transform.root == dqAsset.transform) 
		{
			var go = Instantiate(dqAsset, new Vector3(0,0,0), dqAsset.transform.localRotation);
			dqEffect = go as GameObject;
			shiLieHua += 1;

			zaiRu = true;
			psCount = 0;
			meshTrian = 0;

			var animatorTranS = dqEffect.GetComponentsInChildren<Animator>();
			animatorChilds.Clear();
			zongJiLuTimeS.Clear();
			foreach (var tran in animatorTranS) 
			{
				animatorChilds.Add(tran);
				zongJiLuTimeS.Add(0.0f);
			}
			AnimaterHongPei();

			m_RunningTime = 0.0f;
			playingTime = 0.0f;
		}
	}

	void PSPlayAni()
	{
		if (Application.isPlaying)
		{
			return;
		}
			
		if (openPreview && zaiRu && dqAsset != null) 
		{
			if (dqEffect != null) 
			{
				var psS = dqEffect.GetComponentsInChildren<ParticleSystem>();
				psTime = 0.0f;
				if (psS.Length > 0) 
				{
					foreach (var ps in psS) 
					{
						if ((ps.duration + ps.startLifetime + ps.startDelay) > psTime) 
						{
							psTime = ps.duration + ps.startLifetime + ps.startDelay;
							if (aniTime < psTime) 
							{
								aniTime = psTime;
							}
						}
					}
					foreach (var ps in psS) 
					{
						if (ps.loop == false) 
						{
							if (m_RunningTime >= psTime) 
							{
								ps.Stop();
								m_RunningTime = 0;
							}
						}
						ps.Simulate(m_RunningTime, true, true);
					}
				}
			}
		}
	}

	void AnimaterHongPei()
	{
		if (Application.isPlaying)
		{
			return;
		}

		if (openPreview && zaiRu && dqAsset != null) 
		{
			aniTime = 0.0f;

			if (animatorChilds.Count > 0) 
			{

				if (!animatorHongPei && dqEffect != null) 
				{

					foreach (var ps in animatorChilds) 
					{
						if (ps.GetCurrentAnimatorStateInfo(0).length > aniTime) 
						{
							aniTime = ps.GetCurrentAnimatorStateInfo(0).length; 
						}
					}

					for (int i = 0; i < animatorChilds.Count; i++) 
					{
						if (animatorChilds[i].runtimeAnimatorController != null) 
						{
							float frameRate = 30f;
							int frameCount = (int)((animatorChilds[i].GetCurrentAnimatorStateInfo(0).length * frameRate) + 2);
							animatorChilds[i].Rebind();
							animatorChilds[i].StopPlayback();
							animatorChilds[i].recorderStartTime = 0;

							animatorChilds[i].StartRecording(frameCount);

							for (var j = 0; j < frameCount - 1; j++)
							{
								animatorChilds[i].Update(1.0f / frameRate);
							}

							animatorChilds[i].StopRecording();
							animatorChilds[i].StartPlayback();
							zongJiLuTimeS[i] = animatorChilds[i].recorderStopTime;
							if (animatorChilds[i].recorderStopTime < 0 ) 
							{
								return;
							}
						}
					}
					animatorHongPei = true;
				}
			}

		}
	}

	void AnimationPlay()
	{
		if (Application.isPlaying)
		{
			return;
		}

		if (openPreview && zaiRu && dqAsset != null) 
		{
			if (dqEffect != null) 
			{
				var psS = dqEffect.GetComponentsInChildren<Animation>();
				
				if (psS.Length > 0) 
				{
					foreach (var ps in psS) 
					{
						if (ps.clip.length > aniTime) 
						{
							aniTime = ps.clip.length; 
						}
					}
					
					foreach (var ps in psS) 
					{
						ani_time = m_RunningTime;
						
						ps.clip.SampleAnimation(ps.gameObject, ani_time);
						if (ani_time >= aniTime && m_RunningTime >= psTime) 
						{
							m_RunningTime = 0.0f;
						}
						if (ani_time >= ps.clip.length) 
						{
							ani_time = 0.0f;
						}
					}
				}
			}
		}
	}

	private void inspectorUpdate()
	{
		delta = EditorApplication.timeSinceStartup - m_PreviousTime;
		m_PreviousTime = EditorApplication.timeSinceStartup;

		if (!Application.isPlaying)
		{
			if (!effectAniPlayPause) 
			{
				m_RunningTime = m_RunningTime + (float)delta;
				m_HuaDongTiao = m_RunningTime;
			}
			update();
		}
	}
	//---------------------------------------------------------------------------------------------

	//--------------------------------Hierarchy特效预览------------------------------------------------
	void OnSelectionChangeHierarchy()
	{
		if (Application.isPlaying) 
		{
			setMaxParS = ScriptableObject.CreateInstance<SetMaxParticles>();
		}
		if (setMaxParS != null && dqPrefab != null && Application.isPlaying) 
		{
			setMaxParS.Change();
		}
		if (!effectAniPlayPause && effectAniPlayLoop) 
		{
			m_RunningTime = 0;
			EffectUpdate();
		}

		dqAsset = Selection.activeGameObject;
		if (openEffectAniPlay && dqAsset != null && !AssetDatabase.Contains(dqAsset) && !effectAniPlayLock) 
		{
			if (lockRoot) 
			{
				if (dqAsset.transform.root == dqAsset.transform) 
				{
					dqEffectHierarchy = dqAsset;
					animatorHongPei = false;
					EffectHierarchy();
				}
				return;
			} else {
				dqEffectHierarchy = dqAsset;
				animatorHongPei = false;
				EffectHierarchy();
			}
		}
	}
	
	void EffectHierarchy()
	{
		if (openEffectAniPlay && dqAsset != null && dqEffectHierarchy != null && !AssetDatabase.Contains(dqAsset)) 
		{
			var animatorTranS = dqEffectHierarchy.GetComponentsInChildren<Animator>();
			animatorChilds.Clear();
			zongJiLuTimeS.Clear();
			foreach (var tran in animatorTranS) 
			{
				animatorChilds.Add(tran);
				zongJiLuTimeS.Add(0.0f);
			}
			AnimaterHongPeiHierarchy();
			
			m_RunningTime = 0.0f;
			playingTime = 0.0f;
			psCount = 0;
			meshTrian = 0;
		}
	}
	
	void PSPlayAniHierarchy()
	{
		if (Application.isPlaying)
		{
			return;
		}
		
		if (openEffectAniPlay && dqEffectHierarchy != null) 
		{
			if (dqAsset == null && !effectAniPlayLock) 
			{
				m_RunningTime = 0;
			}

			var psS = dqEffectHierarchy.GetComponentsInChildren<ParticleSystem>();
			psTime = 0.0f;
			if (psS.Length > 0) 
			{
				foreach (var ps in psS) 
				{
					if ((ps.duration + ps.startLifetime + ps.startDelay) > psTime) 
					{
						psTime = ps.duration + ps.startLifetime + ps.startDelay; 
						if (aniTime < psTime) 
						{
							aniTime = psTime;
						}
					}
				}
				foreach (var ps in psS) 
				{
					if (ps.loop == false) 
					{
						if (effectAniPlayLoop && m_RunningTime >= psTime) 
						{
							ps.Stop();
							m_RunningTime = 0;
						}
					}
					ps.Simulate(m_RunningTime, true, true);
				}
			}
		}
	}
	
	void AnimaterHongPeiHierarchy()
	{
		if (Application.isPlaying)
		{
			return;
		}
		
		if (openEffectAniPlay  && dqAsset != null && dqEffectHierarchy != null) 
		{
			aniTime = 0.0f;
			
			if (animatorChilds.Count > 0) 
			{
				
				if (!animatorHongPei) 
				{
					
					foreach (var ps in animatorChilds) 
					{
						if (ps.GetCurrentAnimatorStateInfo(0).length > aniTime) 
						{
							aniTime = ps.GetCurrentAnimatorStateInfo(0).length; 
						}
					}
					
					for (int i = 0; i < animatorChilds.Count; i++) 
					{
						if (animatorChilds[i].runtimeAnimatorController != null) 
						{
							float frameRate = 30f;
							int frameCount = (int)((animatorChilds[i].GetCurrentAnimatorStateInfo(0).length * frameRate) + 2);
							animatorChilds[i].Rebind();
							animatorChilds[i].StopPlayback();
							animatorChilds[i].recorderStartTime = 0;
							
							animatorChilds[i].StartRecording(frameCount);
							
							for (var j = 0; j < frameCount - 1; j++)
							{
								animatorChilds[i].Update(1.0f / frameRate);
							}
							
							animatorChilds[i].StopRecording();
							animatorChilds[i].StartPlayback();
							zongJiLuTimeS[i] = animatorChilds[i].recorderStopTime;
							if (animatorChilds[i].recorderStopTime < 0 ) 
							{
								return;
							}
						}
					}
					animatorHongPei = true;
				}
			}
			
		}
	}
	
	void AnimationPlayHierarchy()
	{
		if (Application.isPlaying)
		{
			return;
		}
		

		if (openEffectAniPlay && dqEffectHierarchy != null) 
		{
			if (dqAsset == null && !effectAniPlayLock) 
			{
				m_RunningTime = 0;
			}

			var psS = dqEffectHierarchy.GetComponentsInChildren<Animation>();
			
			if (psS.Length > 0) 
			{
				foreach (var ps in psS) 
				{
					if (ps.clip.length > aniTime) 
					{
						aniTime = ps.clip.length; 
					}
				}
				
				foreach (var ps in psS) 
				{
					ani_time = m_RunningTime;
					
					ps.clip.SampleAnimation(ps.gameObject, ani_time);
					if (effectAniPlayLoop && ani_time >= aniTime && m_RunningTime >= psTime) 
					{
						m_RunningTime = 0.0f;
					}
					if (ani_time >= ps.clip.length) 
					{
						ani_time = 0.0f;
					}
				}
			}
		}
	}

	/// <summary>
	/// Hierarchy下特效预览
	/// </summary>
	void EffectUpdate()
	{
		if (Application.isPlaying)
		{
			return;
		}

		if (!effectAniPlayLoop) 
		{
			if (m_RunningTime >= aniTime) 
			{
				effectHierarchyUpdate = false;
			}
		} else {
			effectHierarchyUpdate = true;
		}

		if (openEffectAniPlay && dqEffectHierarchy != null && effectHierarchyUpdate) 
		{
			if (dqAsset == null && !effectAniPlayLock) 
			{
				m_RunningTime = 0;
			}
			if (animatorHongPei) 
			{
				for (int i = 0; i < animatorChilds.Count; i++) 
				{
					if (animatorChilds[i] != null) 
					{
						if (animatorChilds[i].GetCurrentAnimatorStateInfo(0).loop) 
						{
							if (zongJiLuTimeS[i] < animatorChilds[i].GetCurrentAnimatorStateInfo(0).length) 
							{
								animatorChilds[i].playbackTime = (m_RunningTime % zongJiLuTimeS[i]);
							} else {
								animatorChilds[i].playbackTime = (m_RunningTime % animatorChilds[i].GetCurrentAnimatorStateInfo(0).length);
							}
						}
						else 
						{
							if (m_RunningTime <= zongJiLuTimeS[i] && m_RunningTime <= animatorChilds[i].GetCurrentAnimatorStateInfo(0).length) 
							{
								animatorChilds[i].playbackTime = m_RunningTime;
							}
							if (effectAniPlayLoop && m_RunningTime >= aniTime && m_RunningTime >= psTime) 
							{
								m_RunningTime = 0.0f;
							}
						}
						animatorChilds[i].Update (0);
					}
				}
			}
			if (!animatorHongPei) 
			{
				AnimaterHongPeiHierarchy();
			}

			PSPlayAniHierarchy();
			AnimationPlayHierarchy();
		}
		
		if (!openEffectAniPlay && dqEffectHierarchy != null) 
		{
			animatorHongPei = false;
			EditorApplication.update -= inspectorUpdate;
			isInspectorUpdate = false;
		}
	}
	//------------------------------------------------------------------------------------------------

	//----------------------------------------特效发射------------------------------------------------
	void EffectFaSheUpdate()
	{
		if (!Application.isPlaying)
		{
			return;
		}

		if (effectFaShe) 
		{
			if (ziDanLife < 0) 
			{
				ziDanLife = 3.0f;
			}

			if (openPreview) 
			{
				ziDan = dqEffect;
				FaSheEffect(ziDan);
			}
			if (openEffectAniPlay) 
			{
				ziDan = dqEffectHierarchy;
				FaSheEffect(ziDan);
			}
		}

		if (!effectFaShe && ziDan != null) 
		{
			ziDanLife = 0;
			ziDan.transform.position = Vector3.zero;
			var trails = ziDan.GetComponentsInChildren<TrailRenderer>();
			foreach (var trail in trails) 
			{
				trail.Clear();
			}
			ziDan = null;
		}
	}

	void FaSheEffect(GameObject go)
	{
		if (go != null) 
		{
			go.transform.Translate(Vector3.forward * Time.deltaTime * ziDanSpeed);
			ziDanLife -= Time.deltaTime;
			if (ziDanLife <= 0) 
			{
				var trails = go.GetComponentsInChildren<TrailRenderer>();
				foreach (var trail in trails) 
				{
					trail.Clear();
				}
				go.transform.position = Vector3.zero;
			}
		}
	}
	//------------------------------------------------------------------------------------------------

	//------------------------------------播放模式下循环播放------------------------------------------
	void GetdqPrefab()
	{
		if (openPreview && dqEffect != null && dqAsset != null) 
		{
			dqPrefab = dqEffect;
		}
		if (openEffectAniPlay && dqEffectHierarchy != null) 
		{
			dqPrefab = dqEffectHierarchy;
		}
	}

	void GetPlayingTime()
	{
		if (dqPrefab != null && playingTime <= 0) 
		{
			var psS = dqPrefab.GetComponentsInChildren<ParticleSystem>();
			if (psS.Length > 0) 
			{
				foreach (var ps in psS) 
				{
					if ((ps.duration + ps.startLifetime + ps.startDelay) > playingTime) 
					{
						playingTime = ps.duration + ps.startLifetime + ps.startDelay;
					}
				}
			}

			var atrS = dqPrefab.GetComponentsInChildren<Animator>();
			if (atrS.Length > 0) 
			{
				foreach (var atr in atrS) 
				{
					if (playingTime < atr.GetCurrentAnimatorStateInfo(0).length) 
					{
						playingTime = atr.GetCurrentAnimatorStateInfo(0).length;
					}
				}
			}

			var aniS = dqPrefab.GetComponentsInChildren<Animation>();
			if (aniS.Length > 0) 
			{
				foreach (var ani in aniS) 
				{
					if (ani.clip.length > playingTime) 
					{
						playingTime = ani.clip.length; 
					}
				}
			}
			isPlayingTime = false;
			runPlayingTime = playingTime;
		}
	}

	void PlayingAllAniPlay()
	{
		if (dqPrefab != null) 
		{
			var psS = dqPrefab.GetComponentsInChildren<ParticleSystem>();
			if (psS.Length > 0) 
			{
				foreach (var ps in psS) 
				{
					if (!ps.isPlaying) 
					{
						ps.Play(true);
					}
				}
			}
			
			var atrS = dqPrefab.GetComponentsInChildren<Animator>();
			if (atrS.Length > 0) 
			{
				foreach (var atr in atrS) 
				{
					var hash = atr.GetCurrentAnimatorStateInfo(0).fullPathHash;
					if (atr.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1) 
					{
						atr.Play(hash, 0, 0);
					}
				}
			}
			
			var aniS = dqPrefab.GetComponentsInChildren<Animation>();
			if (aniS.Length > 0) 
			{
				foreach (var ani in aniS) 
				{
					if (!ani.isPlaying) 
					{
						ani.Play();
					}
				}
			}
			isPlayingTime = true;
		}
	}

	/// <summary>
	/// 播放单独的动画
	/// </summary>
	void PlayingOneAniPlay()
	{
		if (Selection.activeGameObject != null) 
		{
			var psobj = Selection.activeGameObject;
			var ps = psobj.GetComponent<ParticleSystem>();

			if (Application.isPlaying && ps != null && !ps.isPlaying) 
			{
				ps.Play(false);
			}
			if (!Application.isPlaying && ps != null) 
			{
				m_RunningTime = 0;
				psTime = 0.0f;
				if ((ps.duration + ps.startLifetime + ps.startDelay) > psTime) 
				{
					psTime = ps.duration + ps.startLifetime + ps.startDelay; 
				}
				simulatePSOne = true;
				if (ps.startDelay >= ps.startLifetime) 
				{
					Debug.Log("这个粒子的延迟大于粒子的生命，在Unity5.0~5.3.4的版本在编辑模式下可能会看不到粒子！");
				}
			}
		}
	}
	void SimulatePSAniPlay()
	{
		if (simulatePSOne && Selection.activeGameObject != null) 
		{
			var psobj = Selection.activeGameObject;
			var ps = psobj.GetComponent<ParticleSystem>();
			if (ps != null) 
			{
				ps.Simulate(m_RunningTime, false);
			}
			if (m_RunningTime >= psTime) 
			{
				simulatePSOne = false;
			}
		}
	}
	//-----------------------------------------------------------------------------------------------
	void SavePrefab()
	{
		GameObject source = PrefabUtility.GetCorrespondingObjectFromSource (dqEffectHierarchy) as GameObject;
		if(source == null) 
		{
			Debug.LogWarning("你没有选择一个在Project目录里的存在预设的特效！！！");
			return;
		}
		string prefabPath = AssetDatabase.GetAssetPath (source).ToLower ();
		if(prefabPath.EndsWith(".prefab") == false) 
		{
			Debug.LogWarning("你没有选择一个在Project目录里的存在预设的特效！！！");
			return;
		}
		PrefabUtility.ReplacePrefab (dqEffectHierarchy, source, ReplacePrefabOptions.ConnectToPrefab | ReplacePrefabOptions.ReplaceNameBased);
	}

	void PSDelayToZero()
	{
		if (dqPrefab != null) 
		{
			var psS = dqPrefab.GetComponentsInChildren<ParticleSystem>();
			if (psS.Length > 0) 
			{
				foreach (var ps in psS) 
				{
					ps.startDelay = 0;
				}
			}
		}
	}

}
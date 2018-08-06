using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections;
using System.Collections.Generic;

public class EditorModeAnimator
{
    public Animator animator { get; private set; }
    private List<string> mStateNames = new List<string>();
    private string mStateName = "";
    private AnimatorController mController;
    private Dictionary<string, AnimatorState> mStates = new Dictionary<string,AnimatorState>();
    private bool mbPlaying = false;
    private bool mRecorded = false;
    private bool mLoop = false;

    private AnimatorState mState;
    private float mRecordEndTime;
    private float mSliderValue = 0;
    public void OnGUI()
    {
        if (this.mStateNames.Count > 0)
        {
            EditorGUILayout.BeginVertical();
            int animationIndex = Mathf.Max(0, this.mStateNames.IndexOf(this.mStateName));
            animationIndex = EditorGUILayout.Popup("Animation", animationIndex, this.mStateNames.ToArray());
            if (animationIndex < this.mStateNames.Count)
            {
				if (this.mStateNames[animationIndex] != this.mStateName)
                {
					this.mSliderValue = 0;
                    this.mStateName = this.mStateNames[animationIndex];
                    this.mState = this.mStates[this.mStateName];
                    this.Record();
                }
            }

            if (this.mState != null)
            {
                this.mSliderValue = EditorGUILayout.Slider(this.mRunningTime, 0, this.mRecordEndTime);
                var zhen = this.mSliderValue * 30;
                EditorGUILayout.LabelField("帧数：" + (int)zhen);
                if (this.mbPlaying == false)
                {
                    this.mRunningTime = this.mSliderValue;
                }
            }
            EditorGUILayout.EndVertical();
        }
    }

    
    public void Play()
    {
        if (this.animator == null)
            return;

        this.mbPlaying = true;
        if (this.mRecorded == false)
        {
            this.Record();
        }
    }

    private void Record()
    {
        this.mLoop = this.mState.motion.isLooping;
		float  clipLenght = this.mRecordEndTime =  this.GetClip(this.mState.motion.name).length ;
        float frameRate = 30f;

        if(this.animator.playbackTime > 0)
        {
            this.animator.playbackTime = 0;
            this.animator.Update(0);
            //this.animator.Stop();
        }

        this.animator.StopPlayback();

        this.animator.Play(this.mState.name);
        this.animator.recorderStartTime = 0;

		List<float> frames = new List<float> ();
		float delta = 1.0f / frameRate;
		frames.Add (0);
		while(clipLenght > 0)
		{
			if (clipLenght > delta) {
				frames.Add (delta);
			} else {
				frames.Add (clipLenght);
			}
			clipLenght -= delta;
		}
			
		this.animator.StartRecording(frames.Count);

		for (int i = 0; i < frames.Count; i++) {
			this.animator.Update (frames[i]);
		}

        this.animator.StopRecording();

        this.animator.StartPlayback();
        this.mRecorded = true;
        this.mRunningTime = 0;
    }

    public void Stop()
    {
        this.mbPlaying = false;
        this.mRunningTime = 0;
        this.mSliderValue = 0;
    }
		
    private float mRunningTime = 0;

    public void Update(float deltaTime)
    {
        if (this.animator == null)
            return;
        if (this.mRecorded == false)
            return;

        if (this.mbPlaying)
        {
            this.mRunningTime += deltaTime;

            if (this.mRunningTime > this.mRecordEndTime)
            {
                this.mRunningTime = 0;
                if (this.mLoop == false)
                {
                    this.Stop();
                    return;
                }
            }

            this.animator.playbackTime = this.mRunningTime;
            this.animator.Update(0);
        }
        else
        {
            this.animator.playbackTime = this.mRunningTime;
			this.animator.Update(0);
        }

    }

    public void SetAnimator(Animator a)
    {
        if (a != this.animator)
        {
            this.mbPlaying = false;
            this.mRecorded = false;
            this.mRunningTime = 0;
            this.mSliderValue = 0;
            this.mRecordEndTime = 0;
            this.mLoop = false;
            this.mState = null;
            this.animator = a;
            
            this.mStateNames = new List<string>();
            this.mStates = new Dictionary<string, AnimatorState>();
            if (this.animator)
            {
                this.mController = this.animator.runtimeAnimatorController as AnimatorController;
                this.GetAllState();
            }
        }
    }

    private void GetAllState()
    {
        if (this.mController != null)
        {
            foreach (var layer in this.mController.layers)
            {
                if (this.mState == null)
                {
                    this.mState = layer.stateMachine.defaultState;
                    this.mStateName = this.mState.name;
					this.mSliderValue = 0;
					this.Record();
                }
                GetAnimState(layer.stateMachine);
            }
        }
    }

    private void GetAnimState(AnimatorStateMachine aSM)
    {
        foreach (var s in aSM.states)
        {
            if (s.state.motion == null)
                continue;
            var clip = GetClip(s.state.motion.name);
            if (clip != null)
            {
                this.mStates.Add(s.state.name,s.state);
                this.mStateNames.Add(s.state.name);
            }
        }

        foreach (var sms in aSM.stateMachines)
        {
            GetAnimState(sms.stateMachine);
        }
    }

    private AnimationClip GetClip(string name)
    {
        foreach (var clip in this.mController.animationClips)
        {
            if (clip.name.Equals(name))
                return clip;
        }

        return null;
    }
}

class EditorModeParticle
{
    public ParticleSystem particleSystem { get; private set; }
    private float mDuration = 0;
    private bool mLoop = false;
    private float mRunningTime = 0;
    private bool mbPlaying = false;
    public void SetParticleSystem(ParticleSystem ps)
    {
        if (ps != this.particleSystem)
        {
            this.mDuration = 0;
            this.mRunningTime = 0;
            this.mbPlaying = false;
            if (ps)
            {
                this.particleSystem = ps;
                this.mDuration = ps.main.duration;
                this.mLoop = ps.main.loop;

                if (this.mLoop == false)
                {
                    ParticleSystemCurveMode mode = ps.main.startLifetime.mode;
                    if (mode == ParticleSystemCurveMode.Constant)
                    {
                        this.mDuration += ps.main.startLifetime.constant;
                    }
                    else if (mode == ParticleSystemCurveMode.TwoConstants)
                    {
                        this.mDuration += ps.main.startLifetime.constantMax;
                    }
                    else
                    {
                        this.mDuration += 2;
                    }

                    mode = ps.main.startDelay.mode;
                    if (mode == ParticleSystemCurveMode.Constant)
                    {
                        this.mDuration += ps.main.startDelay.constant;
                    }
                    else if (mode == ParticleSystemCurveMode.TwoConstants)
                    {
                        this.mDuration += ps.main.startDelay.constantMax;
                    }
                    else
                    {
                        this.mDuration += 2;
                    }
                }
            }
            
        }
        
    }
    public void Update(float deltaTime)
    {
        if (this.particleSystem == null)
            return;
        if (this.mbPlaying)
        {
            this.mRunningTime += deltaTime;
            this.particleSystem.Simulate(deltaTime, false, false);
            if (this.mLoop == false)
            {
                if (this.mRunningTime > this.mDuration)
                {
                    this.Stop();
                }
            }
        }
       
    }

    public void Play()
    {
        this.mbPlaying = true;
        if (this.particleSystem)
        {
            this.particleSystem.Stop(false);
            this.particleSystem.Clear();
            this.particleSystem.Play(false);
        }
        
    }

    public void Stop()
    {
        this.mbPlaying = false;
        this.mRunningTime = 0;
    }
}

//[CustomEditor(typeof(AnimatorEffectTool))]
public class AnimatorEffectEditor : EditorWindow 
{
    private EditorModeAnimator mAnimator = new EditorModeAnimator();
    private GameObject mRootObject;
    private Animator mMainAnimator;
    private List<EditorModeAnimator> mAnimators = new List<EditorModeAnimator>();
    private List<EditorModeParticle> mParticles = new List<EditorModeParticle>();
    private bool mPlay = false;

    [MenuItem("ArtTools/动画特效工具")]
    public static void OpenAnimatorEffectEditor()
    {
        GetWindow<AnimatorEffectEditor>("动画特效工具");
    }

    public void OnEnable()
    {
        this.mAnimator.SetAnimator(this.mMainAnimator);
        this.ChangeRootObject(this.mRootObject);
        EditorApplication.update += this.OnUpdate;
    }

    void OnDestroy()
    {
        EditorApplication.update -= this.OnUpdate;
    }
    private float mLastTime = 0;
    void OnUpdate()
    {
        if (Application.isPlaying )
        {
            return;
        }
        float timeSinceStartup = Time.realtimeSinceStartup;
        //float timeSinceStartup = (float)EditorApplication.timeSinceStartup;//Time.realtimeSinceStartup;
        float deltaTime = timeSinceStartup - this.mLastTime;
        this.mLastTime = timeSinceStartup;

        this.mAnimator.Update(deltaTime);
        foreach (var animator in this.mAnimators)
        {
            animator.Update(deltaTime);
        }
        
        foreach (var ps in this.mParticles)
        {
            ps.Update(deltaTime);
        }
    }

    private void ChangeRootObject(GameObject rootObject)
    {
        this.mAnimators.Clear();
        this.mParticles.Clear();
        if (rootObject)
        {
            Animator[] animators = rootObject.GetComponentsInChildren<Animator>();
            if (animators != null)
            {
                foreach (var a in animators)
                {
                    if (this.mAnimator.animator != a)
                    {
                        EditorModeAnimator animator = new EditorModeAnimator();
                        animator.SetAnimator(a);
                        this.mAnimators.Add(animator);
                    }

                }
            }

            ParticleSystem[] pss = rootObject.GetComponentsInChildren<ParticleSystem>();
            if (pss != null)
            {
                foreach (var ps in pss)
                {
                    EditorModeParticle p = new EditorModeParticle();
                    p.SetParticleSystem(ps);
                    this.mParticles.Add(p);
                }
            }
        }
    }

	private void RemoveMain()
	{
		EditorModeAnimator remove = null;
		foreach (var a in this.mAnimators)
		{
			if (a.animator == this.mMainAnimator)
			{
				remove = a;
				break;
			}

		}

		if (remove != null) {
			this.mAnimators.Remove (remove);
		}
	}
    public void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        GameObject rootObject = EditorGUILayout.ObjectField("Root", this.mRootObject, typeof(GameObject), true) as GameObject;
        if (GUILayout.Button("Update", GUILayout.Width(50)))
        {
            this.mRootObject = null;
        }
        EditorGUILayout.EndHorizontal();
        if (rootObject != this.mRootObject)
        {
            this.mRootObject = rootObject;
            this.ChangeRootObject(rootObject);
        }
        EditorGUILayout.BeginHorizontal();
        this.mMainAnimator = EditorGUILayout.ObjectField("Animator", this.mMainAnimator, typeof(Animator), true) as Animator;
        if (GUILayout.Button("Update", GUILayout.Width(50)))
        {
            this.mAnimator = new EditorModeAnimator();
        }
        EditorGUILayout.EndHorizontal();

        if (this.mAnimator.animator != this.mMainAnimator)
        {
            this.RemoveMain();
        }

        this.mAnimator.SetAnimator(this.mMainAnimator);
	
        this.mAnimator.OnGUI();
        EditorGUILayout.BeginHorizontal();

        if (this.mMainAnimator)
        {
            bool toggle = GUILayout.Toggle(this.mPlay, this.mPlay ? "Stop" : "Play", EditorStyles.toolbarButton);
            if (this.mPlay != toggle)
            {
                this.mPlay = toggle;
                if (this.mPlay)
                {
                    this.mAnimator.Play();
                    foreach (var animator in this.mAnimators)
                    {
                        animator.Play();
                    }
                    foreach (var ps in this.mParticles)
                    {
                        ps.Play();
                    }
                }
                else
                {
                    this.mAnimator.Stop();
                    foreach (var animator in this.mAnimators)
                    {
                        animator.Stop();
                    }
                    foreach (var ps in this.mParticles)
                    {
                        ps.Stop();
                    }
                }
            }
        }

        EditorGUILayout.EndHorizontal();

    }
}

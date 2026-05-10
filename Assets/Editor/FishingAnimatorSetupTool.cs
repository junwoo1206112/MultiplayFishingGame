using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace MultiplayFishing.Editor
{
    public static class FishingAnimatorSetupTool
    {
        private const string ControllerPath = "Assets/Zimni/Fantasy character/animations/Fantasy_character_AnimatorController.controller";
        private const string FishingIdlePath = "Assets/Zimni/Fantasy character/animations/fishing Idle.anim";
        private const string FishingCastPath = "Assets/Zimni/Fantasy character/animations/Fishing_Cast.anim";
        private const string RodInPath = "Assets/Zimni/Fantasy character/animations/rod-in.anim";
        private const string RodOutPath = "Assets/rod-out.anim";

        [MenuItem("MultiplayFishing/Animation/Setup Fishing Player Animator")]
        public static void SetupFishingPlayerAnimator()
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null)
            {
                Debug.LogError($"[FishingAnimatorSetupTool] Animator Controller not found: {ControllerPath}");
                return;
            }

            AnimationClip fishingIdle = LoadClip(FishingIdlePath);
            AnimationClip fishingCast = LoadClip(FishingCastPath);
            AnimationClip rodIn = LoadClip(RodInPath);
            AnimationClip rodOut = LoadClip(RodOutPath);
            if (fishingIdle == null || fishingCast == null || rodIn == null || rodOut == null) return;

            Undo.RecordObject(controller, "Setup Fishing Player Animator");

            AddParameter(controller, "HasFish", AnimatorControllerParameterType.Bool);
            AddParameter(controller, "RodEquipped", AnimatorControllerParameterType.Bool);
            AddParameter(controller, "RodTakeOut", AnimatorControllerParameterType.Trigger);
            AddParameter(controller, "RodPutAway", AnimatorControllerParameterType.Trigger);
            AddParameter(controller, "FishingCast", AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            AnimatorState idleState = FindState(stateMachine, "Idle");
            AnimatorState fishingStartState = FindState(stateMachine, "Fishing  Start");
            AnimatorState liftState = FindState(stateMachine, "LIfting");
            AnimatorState carryingState = FindState(stateMachine, "Carrying");

            AnimatorState fishingIdleState = GetOrCreateState(stateMachine, "fishing Idle", fishingIdle, new Vector3(1040f, 80f, 0f));
            AnimatorState fishingCastState = GetOrCreateState(stateMachine, "Fishing_Cast", fishingCast, new Vector3(820f, 100f, 0f));
            AnimatorState rodInState = GetOrCreateState(stateMachine, "rod-in", rodIn, new Vector3(360f, -180f, 0f));
            AnimatorState rodOutState = GetOrCreateState(stateMachine, "rod-out", rodOut, new Vector3(600f, -180f, 0f));

            AddCastReleaseEvent(fishingCast);
            AddAnimationEvent(rodIn, "ShowRodEvent", 0f);
            AddAnimationEvent(rodOut, "HideRodEvent", Mathf.Max(0f, rodOut.length * 0.95f));

            AddAnyStateTriggerTransition(stateMachine, fishingCastState, "FishingCast", 0.03f);
            AddExitTransition(fishingCastState, fishingIdleState, 0.05f, 0.9f);
            AddBoolTransition(fishingIdleState, idleState, "fishing", false, 0.05f);

            AddAnyStateTriggerTransition(stateMachine, rodInState, "RodTakeOut", 0.05f);
            AddExitTransition(rodInState, idleState, 0.05f, 0.95f);
            AddAnyStateTriggerTransition(stateMachine, rodOutState, "RodPutAway", 0.05f);
            AddExitTransition(rodOutState, idleState, 0.05f, 0.95f);

            if (liftState != null)
            {
                AddAnyStateBoolTransition(stateMachine, liftState, "HasFish", true, 0.08f);
            }

            if (carryingState != null)
            {
                AddBoolTransition(carryingState, idleState, "HasFish", false, 0.08f);
            }

            if (fishingStartState != null)
            {
                AddExitTransition(fishingStartState, fishingIdleState, 0.08f, 0.95f);
            }

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[FishingAnimatorSetupTool] Fishing player animator setup completed.");
        }

        private static AnimationClip LoadClip(string path)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                Debug.LogError($"[FishingAnimatorSetupTool] Animation clip not found: {path}");
            }

            return clip;
        }

        private static void AddParameter(
            AnimatorController controller,
            string parameterName,
            AnimatorControllerParameterType parameterType)
        {
            if (controller.parameters.Any(parameter => parameter.name == parameterName)) return;

            controller.AddParameter(parameterName, parameterType);
        }

        private static AnimatorState FindState(AnimatorStateMachine stateMachine, string stateName)
        {
            foreach (ChildAnimatorState childState in stateMachine.states)
            {
                if (childState.state.name == stateName)
                {
                    return childState.state;
                }
            }

            return null;
        }

        private static AnimatorState GetOrCreateState(
            AnimatorStateMachine stateMachine,
            string stateName,
            Motion motion,
            Vector3 position)
        {
            AnimatorState state = FindState(stateMachine, stateName);
            if (state == null)
            {
                state = stateMachine.AddState(stateName, position);
            }

            state.motion = motion;
            state.writeDefaultValues = true;
            return state;
        }

        private static void AddAnyStateTriggerTransition(
            AnimatorStateMachine stateMachine,
            AnimatorState destination,
            string triggerName,
            float transitionDuration)
        {
            if (HasAnyStateTransition(stateMachine, destination, triggerName)) return;

            AnimatorStateTransition transition = stateMachine.AddAnyStateTransition(destination);
            transition.hasExitTime = false;
            transition.duration = transitionDuration;
            transition.canTransitionToSelf = false;
            transition.AddCondition(AnimatorConditionMode.If, 0f, triggerName);
        }

        private static void AddAnyStateBoolTransition(
            AnimatorStateMachine stateMachine,
            AnimatorState destination,
            string parameterName,
            bool expectedValue,
            float transitionDuration)
        {
            if (HasAnyStateTransition(stateMachine, destination, parameterName)) return;

            AnimatorStateTransition transition = stateMachine.AddAnyStateTransition(destination);
            transition.hasExitTime = false;
            transition.duration = transitionDuration;
            transition.canTransitionToSelf = false;
            transition.AddCondition(expectedValue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, parameterName);
        }

        private static void AddBoolTransition(
            AnimatorState source,
            AnimatorState destination,
            string parameterName,
            bool expectedValue,
            float transitionDuration)
        {
            if (source == null || destination == null) return;
            if (HasTransition(source, destination, parameterName)) return;

            AnimatorStateTransition transition = source.AddTransition(destination);
            transition.hasExitTime = false;
            transition.duration = transitionDuration;
            transition.canTransitionToSelf = false;
            transition.AddCondition(expectedValue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, parameterName);
        }

        private static void AddExitTransition(
            AnimatorState source,
            AnimatorState destination,
            float transitionDuration,
            float exitTime)
        {
            if (source == null || destination == null) return;
            if (HasTransition(source, destination, null)) return;

            AnimatorStateTransition transition = source.AddTransition(destination);
            transition.hasExitTime = true;
            transition.exitTime = exitTime;
            transition.duration = transitionDuration;
            transition.canTransitionToSelf = false;
        }

        private static bool HasAnyStateTransition(
            AnimatorStateMachine stateMachine,
            AnimatorState destination,
            string parameterName)
        {
            return stateMachine.anyStateTransitions.Any(transition =>
                transition.destinationState == destination &&
                transition.conditions.Any(condition => condition.parameter == parameterName));
        }

        private static bool HasTransition(
            AnimatorState source,
            AnimatorState destination,
            string parameterName)
        {
            return source.transitions.Any(transition =>
                transition.destinationState == destination &&
                (parameterName == null ||
                 transition.conditions.Any(condition => condition.parameter == parameterName)));
        }

        private static void AddCastReleaseEvent(AnimationClip clip)
        {
            AddAnimationEvent(clip, "OnCastRelease", Mathf.Max(0f, clip.length * 0.65f));
        }

        private static void AddAnimationEvent(AnimationClip clip, string functionName, float time)
        {
            AnimationEvent[] events = AnimationUtility.GetAnimationEvents(clip);
            if (events.Any(animationEvent => animationEvent.functionName == functionName)) return;

            AnimationEvent[] updatedEvents = events
                .Concat(new[]
                {
                    new AnimationEvent
                    {
                        functionName = functionName,
                        time = Mathf.Clamp(time, 0f, clip.length)
                    }
                })
                .OrderBy(animationEvent => animationEvent.time)
                .ToArray();

            AnimationUtility.SetAnimationEvents(clip, updatedEvents);
            EditorUtility.SetDirty(clip);
        }
    }
}

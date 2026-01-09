using System;
using System.Collections;
using System.Reflection;
using BYOJoystick.Bindings;
using BYOJoystick.Controls.Sync;
using UnityEngine;
using VTOLVR.Multiplayer;

namespace BYOJoystick.Controls
{
    public class CButton : IControl
    {
        private static readonly MethodInfo StartInteractionMethod = typeof(VRButton).GetMethod(
            "Vrint_OnStartInteraction",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly MethodInfo StopInteractionMethod = typeof(VRButton).GetMethod(
            "Vrint_OnStopInteraction",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly MethodInfo WhileInteractingRoutineMethod = typeof(VRInteractable).GetMethod(
            "WhileInteractingRoutine",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        protected readonly VRInteractable               Interactable;
        protected readonly InteractableSyncWrapper      SyncWrapper;
        protected readonly bool                         IsMP;
        protected readonly VRButton                     Button;
        protected          bool                         Pressed;
        protected readonly Action<VRInteractable, bool> SetInteracting;
        protected readonly Action<VRInteractable, int>  SetInteractedOnFrame;

        public CButton(VRInteractable interactable, VRButton button)
        {
            Interactable         = interactable;
            IsMP                 = VTOLMPUtils.IsMultiplayer();
            SyncWrapper          = IsMP ? InteractableSyncWrapper.Create(interactable) : null;
            Button               = button;
            Pressed              = false;
            SetInteracting       = CompiledExpressions.CreatePropertySetter<VRInteractable, bool>("interacting");
            SetInteractedOnFrame = CompiledExpressions.CreateFieldSetter<VRInteractable, int>("interactedOnFrame");
        }

        public void PostUpdate()
        {
        }

        public static void Use(CButton c, Binding binding, int state)
        {
            if (binding.GetAsBool())
            {
                if (c.Pressed || !c.Interactable.enabled || !c.Interactable.gameObject.activeInHierarchy)
                    return;

                if (c.IsMP)
                {
                    if (c.SyncWrapper != null && !c.SyncWrapper.TryInteracting(false))
                        return;
                }

                c.Pressed = true;
                c.SetInteracting(c.Interactable, true);
                c.SetInteractedOnFrame(c.Interactable, Time.frameCount);
                InvokeButtonMethod(StartInteractionMethod, c.Button);
                c.Interactable.OnInteract?.Invoke();

                if (c.Interactable.OnInteracting == null)
                    return;
                StartWhileInteractingRoutine(c.Interactable);
            }
            else
            {
                if (!c.Pressed)
                    return;

                if (c.IsMP && c.SyncWrapper != null && c.SyncWrapper.IsInteracting)
                    c.SyncWrapper.StopInteracting(false);

                c.Pressed = false;
                c.Interactable.StopInteraction();
                InvokeButtonMethod(StopInteractionMethod, c.Button);
            }
        }

        private static void InvokeButtonMethod(MethodInfo method, VRButton button)
        {
            if (method == null || button == null)
            {
                return;
            }

            method.Invoke(button, new object[] { null });
        }

        private static void StartWhileInteractingRoutine(VRInteractable interactable)
        {
            if (interactable == null || WhileInteractingRoutineMethod == null)
            {
                return;
            }

            var routine = WhileInteractingRoutineMethod.Invoke(interactable, null) as IEnumerator;
            if (routine == null)
            {
                return;
            }

            var behaviour = interactable as MonoBehaviour;
            if (behaviour == null)
            {
                return;
            }

            behaviour.StartCoroutine(routine);
        }
    }
}

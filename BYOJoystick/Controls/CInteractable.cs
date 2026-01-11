using System;
using System.Collections;
using System.Reflection;
using BYOJoystick.Bindings;
using BYOJoystick.Controls.Sync;
using UnityEngine;
using VTOLVR.Multiplayer;

namespace BYOJoystick.Controls
{
    public class CInteractable : IControl
    {
        private static readonly MethodInfo WhileInteractingRoutineMethod = typeof(VRInteractable).GetMethod(
            "WhileInteractingRoutine",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        protected readonly VRInteractable               Interactable;
        protected readonly InteractableSyncWrapper      SyncWrapper;
        protected readonly bool                         IsMP;
        protected          bool                         Pressed;
        protected readonly Action<VRInteractable, bool> SetInteracting;
        protected readonly Action<VRInteractable, int>  SetInteractedOnFrame;

        public CInteractable(VRInteractable interactable)
        {
            Interactable         = interactable;
            IsMP                 = VTOLMPUtils.IsMultiplayer();
            SyncWrapper          = IsMP ? InteractableSyncWrapper.Create(interactable) : null;
            Pressed              = false;
            SetInteracting       = CompiledExpressions.CreatePropertySetter<VRInteractable, bool>("interacting");
            SetInteractedOnFrame = CompiledExpressions.CreateFieldSetter<VRInteractable, int>("interactedOnFrame");
        }

        public void PostUpdate()
        {
        }

        public static void Use(CInteractable c, Binding binding, int state)
        {
            if (binding.GetAsBool())
            {
                if (c.Pressed || !c.Interactable.enabled)
                    return;

                if (c.IsMP)
                {
                    if (c.SyncWrapper != null && !c.SyncWrapper.TryInteracting(false))
                        return;
                }

                c.Pressed = true;
                c.SetInteracting(c.Interactable, true);
                c.SetInteractedOnFrame(c.Interactable, Time.frameCount);
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
                {
                    try
                    {
                        c.SyncWrapper.StopInteracting(false);
                    }
                    catch
                    {
                        // Ignore any errors from network sync stopping
                    }
                }

                // Clear pressed first to avoid reentrancy issues
                c.Pressed = false;
                try
                {
                    // Some VRInteractable implementations (e.g., VRLever) may throw when StopInteraction
                    // is invoked without a proper controller. Catch exceptions to avoid crashing the plugin.
                    c.Interactable.StopInteraction();
                }
                catch
                {
                    // Swallow exceptions from StopInteraction to prevent log spam and maintain stability
                }
            }
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

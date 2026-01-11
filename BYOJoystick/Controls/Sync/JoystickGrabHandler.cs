using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using VTOLVR.Multiplayer;
using BYOJoystick;

namespace BYOJoystick.Controls.Sync
{
    public class JoystickGrabHandler
    {
        public bool IsGrabbed    { get; private set; }
        public int  ControlIndex { get; }

        private readonly VRInteractable                  _interactable;
        private readonly VRJoystick                      _joystick;
        private readonly object                          _jSync;
        private readonly object                          _muvs;
        private readonly Action<ConnectedJoysticks, int> _onLocalGrabbedStick;
        private readonly Action<ConnectedJoysticks, int> _onLocalReleasedStick;
        private readonly Action<object, int>             _callOnGrabbedStick;
        private readonly Action<object, int>             _callOnReleasedStick;
        private readonly Action<object, VRInteractable> _callOnControlInteract;
        private readonly Action<object>                  _callOnControlInteracting;
        private readonly Action<object, VRInteractable> _callOnControlStopInteract;
        private          float                           _grabTimer;
        private          float                           _grabTime;

        private JoystickGrabHandler(VRJoystick joystick, object jSync, int controlIndex, object muvs, VRInteractable interactable)
        {
            _interactable         = interactable;
            _joystick             = joystick;
            _jSync                = jSync;
            _muvs                 = muvs;
            ControlIndex          = controlIndex;
            _onLocalGrabbedStick  = CompiledExpressions.CreateEventInvoker<ConnectedJoysticks>("OnLocalGrabbedStick");
            _onLocalReleasedStick = CompiledExpressions.CreateEventInvoker<ConnectedJoysticks>("OnLocalReleasedStick");
            _callOnGrabbedStick   = CreateMethodCaller(jSync, "OnGrabbedStick");
            _callOnReleasedStick  = CreateMethodCaller(jSync, "OnReleasedStick");
            _callOnControlInteract   = CreateMethodCallerForMuvs(muvs, "OnControlInteract");
            _callOnControlInteracting = CreateMethodCallerNoArg(muvs, "OnControlInteracting");
            _callOnControlStopInteract = CreateMethodCallerForMuvs(muvs, "OnControlStopInteract");
        }

        public static JoystickGrabHandler Create(VRJoystick joystick, ConnectedJoysticks jSync, MultiUserVehicleSync muvs, VRInteractable interactable)
        {
            int ctrlIdx = jSync.joysticks.IndexOf(joystick);
            if (ctrlIdx == -1)
                Plugin.Log($"Joystick {interactable.GetControlReferenceName()} not found in ConnectedJoysticks");
            else
                Plugin.Log($"Creating JoystickGrabHandler for {interactable.GetControlReferenceName()}");

            return ctrlIdx == -1 ? null : new JoystickGrabHandler(joystick, jSync, ctrlIdx, muvs, interactable);
        }

        public void GrabStick(float time)
        {
            _grabTime  = time;
            _grabTimer = 0;
            if (IsGrabbed)
                return;

            IsGrabbed = true;
            _callOnGrabbedStick?.Invoke(_jSync, ControlIndex);
            _callOnControlInteract?.Invoke(_muvs, _interactable);
            _onLocalGrabbedStick?.Invoke((ConnectedJoysticks)_jSync, ControlIndex);
            _interactable.StartCoroutine(GrabbedEnumerator());
        }

        private IEnumerator GrabbedEnumerator()
        {
            while (IsGrabbed)
            {
                _grabTimer += Time.deltaTime;
                if (_grabTimer > _grabTime || !IsGrabbed)
                {
                    ReleaseStick();
                    yield break;
                }

                _callOnControlInteracting?.Invoke(_muvs);
                yield return null;
            }
        }

        public void ReleaseStick()
        {
            IsGrabbed = false;
            _callOnReleasedStick?.Invoke(_jSync, ControlIndex);
            _callOnControlStopInteract?.Invoke(_muvs, _interactable);
            _onLocalReleasedStick?.Invoke((ConnectedJoysticks)_jSync, ControlIndex);
        }

        private static Action<object, int> CreateMethodCaller(object targetExample, string methodName)
        {
            if (targetExample == null)
                return null;
            var type = targetExample.GetType();
            var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null)
                return null;
            return (obj, i) => method.Invoke(obj, new object[] { i });
        }

        private static Action<object, VRInteractable> CreateMethodCallerForMuvs(object targetExample, string methodName)
        {
            if (targetExample == null)
                return null;
            var type = targetExample.GetType();
            var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null)
                return null;
            return (obj, interactable) => method.Invoke(obj, new object[] { interactable });
        }

        private static Action<object> CreateMethodCallerNoArg(object targetExample, string methodName)
        {
            if (targetExample == null)
                return null;
            var type = targetExample.GetType();
            var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null)
                return null;
            return obj => method.Invoke(obj, null);
        }
    }
}
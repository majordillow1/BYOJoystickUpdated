using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using VTOLVR.Multiplayer;
using BYOJoystick;

namespace BYOJoystick.Controls.Sync
{
    public class ThrottleGrabHandler
    {
        public bool IsGrabbed { get; private set; }

        private readonly VRInteractable                  _interactable;
        private readonly VRThrottle                      _throttle;
        private readonly object                          _tSync;
        private readonly int                             _ctrlIdx;
        private readonly object                          _muvs;
        private readonly Action<ConnectedThrottles, int> _onLocalGrabbedThrottle;
        private readonly Action<ConnectedThrottles, int> _onLocalReleasedThrottle;
        private readonly Action<object, int>             _callOnGrabbedThrottle;
        private readonly Action<object, int>             _callOnReleasedThrottle;
        private readonly Action<object, VRInteractable> _callOnControlInteract;
        private readonly Action<object>                  _callOnControlInteracting;
        private readonly Action<object, VRInteractable> _callOnControlStopInteract;

        private          float                           _grabTimer;
        private          float                           _grabTime;

        private ThrottleGrabHandler(VRThrottle           throttle,
                                    object               tSync,
                                    int                  ctrlIdx,
                                    object               muvs,
                                    VRInteractable       interactable)
        {
            _interactable            = interactable;
            _throttle                = throttle;
            _tSync                   = tSync;
            _ctrlIdx                 = ctrlIdx;
            _muvs                    = muvs;
            _onLocalGrabbedThrottle  = CompiledExpressions.CreateEventInvoker<ConnectedThrottles>("OnLocalGrabbedThrottle");
            _onLocalReleasedThrottle = CompiledExpressions.CreateEventInvoker<ConnectedThrottles>("OnLocalReleasedThrottle");
            _callOnGrabbedThrottle   = CreateMethodCaller(tSync, "OnGrabbedThrottle");
            _callOnReleasedThrottle  = CreateMethodCaller(tSync, "OnReleasedThrottle");
            _callOnControlInteract   = CreateMethodCallerForMuvs(muvs, "OnControlInteract");
            _callOnControlInteracting = CreateMethodCallerNoArg(muvs, "OnControlInteracting");
            _callOnControlStopInteract = CreateMethodCallerForMuvs(muvs, "OnControlStopInteract");
        }

        public static ThrottleGrabHandler Create(VRThrottle throttle, ConnectedThrottles tSync, MultiUserVehicleSync muvs, VRInteractable interactable)
        {
            if (tSync == null)
            {
                Plugin.Log("ConnectedThrottles is null");
                return null;
            }

            int ctrlIdx = tSync.throttles.IndexOf(throttle);
            if (ctrlIdx == -1)
                Plugin.Log($"Throttle {interactable.GetControlReferenceName()} not found in ConnectedThrottles");
            else
                Plugin.Log($"Creating ThrottleGrabHandler for {interactable.GetControlReferenceName()}");

            return ctrlIdx == -1 ? null : new ThrottleGrabHandler(throttle, tSync, ctrlIdx, muvs, interactable);
        }

        public void GrabThrottle(float time)
        {
            _grabTime  = time;
            _grabTimer = 0;
            if (IsGrabbed)
                return;

            IsGrabbed = true;
            _callOnGrabbedThrottle?.Invoke(_tSync, _ctrlIdx);
            _callOnControlInteract?.Invoke(_muvs, _interactable);
            _onLocalGrabbedThrottle?.Invoke((ConnectedThrottles)_tSync, _ctrlIdx);
            _interactable.StartCoroutine(GrabbedEnumerator());
        }

        private IEnumerator GrabbedEnumerator()
        {
            while (IsGrabbed)
            {
                _grabTimer += Time.deltaTime;
                if (_grabTimer > _grabTime || !IsGrabbed)
                {
                    ReleaseThrottle();
                    yield break;
                }

                _callOnControlInteracting?.Invoke(_muvs);
                yield return null;
            }
        }

        private void ReleaseThrottle()
        {
            IsGrabbed = false;
            _callOnReleasedThrottle?.Invoke(_tSync, _ctrlIdx);
            _callOnControlStopInteract?.Invoke(_muvs, _interactable);
            _onLocalReleasedThrottle?.Invoke((ConnectedThrottles)_tSync, _ctrlIdx);
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
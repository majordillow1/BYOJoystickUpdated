using System;
using System.Collections.Generic;
using BYOJoystick.Controls;
using BYOJoystick.Managers.Base;
using UnityEngine;

namespace BYOJoystick.Managers.Dynamic
{
    public class DynamicManager : Manager
    {
        private readonly string _vehicleName;
        private readonly bool   _isMulticrew;
        private readonly bool   _isSeatA;

        private readonly HashSet<string> _actionNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private DynamicManager(string vehicleName, bool isMulticrew, bool isSeatA)
        {
            _vehicleName = vehicleName;
            _isMulticrew = isMulticrew;
            _isSeatA     = isSeatA;
        }

        public static DynamicManager Create(string vehicleName, bool isMulticrew, bool isSeatA)
        {
            return new DynamicManager(vehicleName, isMulticrew, isSeatA);
        }

        public override string GameName    => _vehicleName;
        public override string ShortName   => $"AUTO_{Sanitise(_vehicleName)}_{(_isMulticrew ? (_isSeatA ? "A" : "B") : "S")}";
        public override bool   IsMulticrew => _isMulticrew;
        public override bool   IsSeatA     => _isSeatA;
        public override string VehicleName => _vehicleName;

        protected override void PreMapping()
        {
            _actionNames.Clear();
            AddJoystickControls();
            AddThrottleControls();
            AddInteractableControls();
        }

        protected override void CreateFlightControls()
        {
        }

        protected override void CreateAssistControls()
        {
        }

        protected override void CreateNavigationControls()
        {
        }

        protected override void CreateSystemsControls()
        {
        }

        protected override void CreateHUDControls()
        {
        }

        protected override void CreateNumPadControls()
        {
        }

        protected override void CreateDisplayControls()
        {
        }

        protected override void CreateRadioControls()
        {
        }

        protected override void CreateMusicControls()
        {
        }

        protected override void CreateLightsControls()
        {
        }

        protected override void CreateMiscControls()
        {
        }

        private void AddJoystickControls()
        {
            if (VehicleControlManifest == null || VehicleControlManifest.joysticks == null || VehicleControlManifest.joysticks.Length == 0)
                return;

            string controlKey = $"auto_joystick_{VehicleName}";
            FlightAxisC(MakeUniqueActionName("Pitch"), controlKey, JoystickMapper, CJoystick.SetPitch);
            FlightAxisC(MakeUniqueActionName("Yaw"), controlKey, JoystickMapper, CJoystick.SetYaw);
            FlightAxisC(MakeUniqueActionName("Roll"), controlKey, JoystickMapper, CJoystick.SetRoll);

            FlightButton(MakeUniqueActionName("Trigger"), controlKey, JoystickMapper, CJoystick.Trigger);
            FlightButton(MakeUniqueActionName("Menu Button"), controlKey, JoystickMapper, CJoystick.MenuButton);
            FlightButton(MakeUniqueActionName("Thumbstick Press"), controlKey, JoystickMapper, CJoystick.ThumbstickButton);
            FlightAxisC(MakeUniqueActionName("Thumbstick X"), controlKey, JoystickMapper, CJoystick.SetThumbstickX);
            FlightAxisC(MakeUniqueActionName("Thumbstick Y"), controlKey, JoystickMapper, CJoystick.SetThumbstickY);
        }

        private void AddThrottleControls()
        {
            var throttle = GetFirstThrottle();
            if (throttle == null)
                return;

            string controlKey = $"auto_throttle_{VehicleName}";
            FlightAxis(MakeUniqueActionName("Throttle Axis"), controlKey, ThrottleMapper, CThrottle.Set);
            FlightButton(MakeUniqueActionName("Throttle Increase"), controlKey, ThrottleMapper, CThrottle.Increase);
            FlightButton(MakeUniqueActionName("Throttle Decrease"), controlKey, ThrottleMapper, CThrottle.Decrease);
        }

        private void AddInteractableControls()
        {
            if (Interactables == null)
                return;

            foreach (var interactable in Interactables)
            {
                if (interactable == null)
                    continue;

                string referenceName = interactable.GetControlReferenceName();
                if (string.IsNullOrWhiteSpace(referenceName))
                    continue;

                if (interactable.GetComponent<VRJoystick>() != null || interactable.GetComponent<VRThrottle>() != null)
                    continue;

                var button = interactable.GetComponent<VRButton>();
                if (button != null)
                {
                    AddButtonAction(referenceName, interactable, button);
                    continue;
                }

                var lever = interactable.GetComponent<VRLever>();
                if (lever != null)
                {
                    AddLeverAction(referenceName, interactable, lever);
                    continue;
                }

                var knob = interactable.GetComponent<VRTwistKnob>();
                if (knob != null)
                {
                    AddKnobAction(referenceName, interactable, knob);
                    continue;
                }

                var knobInt = interactable.GetComponent<VRTwistKnobInt>();
                if (knobInt != null)
                {
                    AddKnobIntAction(referenceName, interactable, knobInt);
                    continue;
                }

                var door = interactable.GetComponent<VRDoor>();
                if (door != null)
                {
                    AddDoorAction(referenceName, interactable, door);
                    continue;
                }

                AddGenericInteractable(referenceName, interactable);
            }
        }

        private void AddButtonAction(string referenceName, VRInteractable interactable, VRButton button)
        {
            string controlKey = $"auto_btn_{referenceName}";
            string actionName = MakeUniqueActionName($"Button: {referenceName}");
            MiscButton(actionName, controlKey, (_, _, _, _, _) => ToControl<VRButton, CButton>(controlKey, interactable, button), CButton.Use);
        }

        private void AddLeverAction(string referenceName, VRInteractable interactable, VRLever lever)
        {
            string controlKey = $"auto_lever_{referenceName}";
            string actionName = MakeUniqueActionName($"Lever: {referenceName}");
            MiscButton(actionName, controlKey, (_, _, _, _, _) => ToControl<VRLever, CLever>(controlKey, interactable, lever), CLever.Cycle);
        }

        private void AddKnobAction(string referenceName, VRInteractable interactable, VRTwistKnob knob)
        {
            string controlKey = $"auto_knob_{referenceName}";
            MiscAxis(MakeUniqueActionName($"Knob Axis: {referenceName}"), controlKey, (_, _, _, _, _) => ToControl<VRTwistKnob, CKnob>(controlKey, interactable, knob), CKnob.Set);
            MiscButton(MakeUniqueActionName($"Knob Increase: {referenceName}"), controlKey, (_, _, _, _, _) => ToControl<VRTwistKnob, CKnob>(controlKey, interactable, knob), CKnob.Increase);
            MiscButton(MakeUniqueActionName($"Knob Decrease: {referenceName}"), controlKey, (_, _, _, _, _) => ToControl<VRTwistKnob, CKnob>(controlKey, interactable, knob), CKnob.Decrease);
        }

        private void AddKnobIntAction(string referenceName, VRInteractable interactable, VRTwistKnobInt knobInt)
        {
            string controlKey = $"auto_knobi_{referenceName}";
            MiscButton(MakeUniqueActionName($"Knob Cycle: {referenceName}"), controlKey, (_, _, _, _, _) => ToControl<VRTwistKnobInt, CKnobInt>(controlKey, interactable, knobInt), CKnobInt.Cycle);
            MiscButton(MakeUniqueActionName($"Knob Next: {referenceName}"), controlKey, (_, _, _, _, _) => ToControl<VRTwistKnobInt, CKnobInt>(controlKey, interactable, knobInt), CKnobInt.Next);
            MiscButton(MakeUniqueActionName($"Knob Prev: {referenceName}"), controlKey, (_, _, _, _, _) => ToControl<VRTwistKnobInt, CKnobInt>(controlKey, interactable, knobInt), CKnobInt.Prev);
            MiscButton(MakeUniqueActionName($"Knob Push: {referenceName}"), controlKey, (_, _, _, _, _) => ToControl<VRTwistKnobInt, CKnobInt>(controlKey, interactable, knobInt), CKnobInt.Push);
        }

        private void AddDoorAction(string referenceName, VRInteractable interactable, VRDoor door)
        {
            string controlKey = $"auto_door_{referenceName}";
            MiscButton(MakeUniqueActionName($"Door Toggle: {referenceName}"), controlKey, (_, _, _, _, _) => ToControl<VRDoor, CDoor>(controlKey, interactable, door), CDoor.Toggle);
        }

        private void AddGenericInteractable(string referenceName, VRInteractable interactable)
        {
            string controlKey = $"auto_int_{referenceName}";
            string actionName = MakeUniqueActionName($"Interact: {referenceName}");
            MiscButton(actionName, controlKey, (_, _, _, _, _) => ToControl<VRInteractable, CInteractable>(controlKey, interactable, interactable), CInteractable.Use);
        }

        private CJoystick JoystickMapper(string name, string root, bool nullable, bool checkName, int idx)
        {
            if (TryGetExistingControl<CJoystick>(name, out var existing))
                return existing;

            var joysticks = VehicleControlManifest.joysticks;
            if (joysticks == null || joysticks.Length == 0)
                return null;

            var sideStick   = joysticks[0];
            var centerStick = joysticks.Length > 1 ? joysticks[1] : null;
            var control     = new CJoystick(Vehicle, sideStick, centerStick, IsMulticrew, true);
            Controls.Add(name, control);
            return control;
        }

        private CThrottle ThrottleMapper(string name, string root, bool nullable, bool checkName, int idx)
        {
            if (TryGetExistingControl<CThrottle>(name, out var existing))
                return existing;

            var throttle = GetFirstThrottle();
            if (throttle == null)
                return null;

            var interactable = GetInteractable(throttle);
            var control      = new CThrottle(Vehicle, interactable, throttle, IsMulticrew);
            Controls.Add(name, control);
            return control;
        }

        private VRThrottle GetFirstThrottle()
        {
            if (VehicleControlManifest == null)
                return null;

            if (VehicleControlManifest.throttle != null)
                return VehicleControlManifest.throttle;

            if (VehicleControlManifest.powerLevers != null)
            {
                for (int i = 0; i < VehicleControlManifest.powerLevers.Length; i++)
                {
                    var powerLever = VehicleControlManifest.powerLevers[i];
                    if (powerLever != null)
                        return powerLever;
                }
            }

            if (VehicleControlManifest.collectives != null)
            {
                for (int i = 0; i < VehicleControlManifest.collectives.Length; i++)
                {
                    var collective = VehicleControlManifest.collectives[i];
                    if (collective != null)
                        return collective;
                }
            }

            return null;
        }

        private string MakeUniqueActionName(string candidate)
        {
            string name    = candidate;
            int    counter = 1;
            while (_actionNames.Contains(name))
            {
                name = $"{candidate} ({counter++})";
            }

            _actionNames.Add(name);
            return name;
        }

        private static string Sanitise(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "Unknown";
            var chars = value.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (!char.IsLetterOrDigit(chars[i]))
                    chars[i] = '_';
            }

            return new string(chars);
        }
    }
}

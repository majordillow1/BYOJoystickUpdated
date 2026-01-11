using System;
using System.Collections.Generic;
using System.Reflection;
using BYOJoystick.Bindings;

namespace BYOJoystick.Controls
{
    public class CMFD : IControl
    {
        protected readonly MFD       MFD;
        protected readonly CButton[] Buttons = new CButton[(int)MFD.MFDButtons.B5 + 1];

        public CMFD(MFD mfd)
        {
            MFD = mfd;
            var buttonCompsField = typeof(MFD).GetField("buttonComps", BindingFlags.NonPublic | BindingFlags.Instance);
            if (buttonCompsField == null)
                throw new InvalidOperationException("MFD.buttonComps field not found");

            var buttonCompsObj = buttonCompsField.GetValue(mfd);
            if (buttonCompsObj == null)
                throw new InvalidOperationException("MFD.buttonComps is null");

            var enumerable = buttonCompsObj as System.Collections.IEnumerable;
            if (enumerable == null)
                throw new InvalidOperationException("MFD.buttonComps is not enumerable");

            foreach (var kv in enumerable)
            {
                var kvType = kv.GetType();
                var keyProp = kvType.GetProperty("Key");
                var valProp = kvType.GetProperty("Value");
                if (keyProp == null || valProp == null)
                    continue;

                var key = keyProp.GetValue(kv);
                var val = valProp.GetValue(kv);
                if (val == null || key == null)
                    continue;

                var valType = val.GetType();
                var interactableField = valType.GetField("interactable", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (interactableField == null)
                    throw new InvalidOperationException("MFD button component 'interactable' field not found");

                var interactable = interactableField.GetValue(val) as VRInteractable;
                if (interactable == null)
                    throw new InvalidOperationException("Interactable field is not a VRInteractable");

                var button = interactable.GetComponent<VRButton>();
                if (button == null)
                    throw new InvalidOperationException($"Interactable {interactable.GetControlReferenceName()} does not have a VRButton component.");

                Buttons[(int)(MFD.MFDButtons)key] = new CButton(interactable, button);
            }
        }

        public void PostUpdate()
        {
        }

        public static void PowerToggle(CMFD c, Binding binding, int state)
        {
            if (binding.GetAsBool())
                c.MFD.powerKnob.RemoteSetState(c.MFD.powerOn ? 0 : 1);
        }

        public static void PowerOn(CMFD c, Binding binding, int state)
        {
            if (binding.GetAsBool())
                c.MFD.powerKnob.RemoteSetState(1);
        }

        public static void PowerOff(CMFD c, Binding binding, int state)
        {
            if (binding.GetAsBool())
                c.MFD.powerKnob.RemoteSetState(0);
        }

        public static void Press(CMFD c, Binding binding, int state)
        {
            CButton.Use(c.Buttons[state], binding, -1);
        }
    }
}
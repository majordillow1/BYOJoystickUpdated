using System.Reflection;
using BYOJoystick.Bindings;

namespace BYOJoystick.Controls
{
    public class CPresetButton : IControl
    {
        protected readonly MFDPortalPresetButton PortalPresetButton;
        protected readonly MFDPortalManager      MFDPortalManager;

        private static readonly MethodInfo SavePresetMethod = typeof(MFDPortalPresetButton).GetMethod("SavePreset", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly MethodInfo LoadPresetMethod = typeof(MFDPortalPresetButton).GetMethod("LoadPreset", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        public CPresetButton(MFDPortalPresetButton portalPresetButton, MFDPortalManager mfdPortalManager)
        {
            MFDPortalManager   = mfdPortalManager;
            PortalPresetButton = portalPresetButton;
        }

        public void PostUpdate()
        {
        }

        public static void Save(CPresetButton c, Binding binding, int state)
        {
            if (binding.GetAsBool())
            {
                if (SavePresetMethod != null)
                    SavePresetMethod.Invoke(c.PortalPresetButton, null);
                else
                    c.PortalPresetButton.GetType().GetMethod("SavePreset")?.Invoke(c.PortalPresetButton, null);
            }
        }

        public static void Load(CPresetButton c, Binding binding, int state)
        {
            if (binding.GetAsBool())
            {
                c.MFDPortalManager.PlayInputSound();
                if (LoadPresetMethod != null)
                    LoadPresetMethod.Invoke(c.PortalPresetButton, null);
                else
                    c.PortalPresetButton.GetType().GetMethod("LoadPreset")?.Invoke(c.PortalPresetButton, null);
            }
        }
    }
}
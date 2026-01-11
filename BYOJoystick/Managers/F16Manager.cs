using BYOJoystick.Controls;
using BYOJoystick.Managers.Base;
using VTOLVR.Multiplayer;
using MFDButtons = MFD.MFDButtons;

namespace BYOJoystick.Managers
{
    public class F16Manager : Manager
    {
        public override string GameName    => "F-16";
        public override string ShortName   => "F16";
        public override bool   IsMulticrew => false;

        // Default scene paths; adjust if your modded F-16 places controls elsewhere
        private static string Dash           => "Local/DashCanvas";
        private static string SideJoystick   => "Local/SideStickObjects";
        private static string CenterJoystick => "Local/CenterStickObjects";

        private CJoystick Joysticks(string name, string root, bool nullable, bool checkName, int idx)
        {
            return GetJoysticksByPaths(name, SideJoystick, CenterJoystick);
        }

        protected override void PreMapping()
        {
        }

        protected override void CreateFlightControls()
        {
            // Primary flight axes
            FlightAxisC("Joystick Pitch", "Joystick", Joysticks, CJoystick.SetPitch);
            FlightAxisC("Joystick Yaw", "Joystick", Joysticks, CJoystick.SetYaw);
            FlightAxisC("Joystick Roll", "Joystick", Joysticks, CJoystick.SetRoll);

            // Throttle
            FlightAxis("Throttle", "Throttle", ByManifest<VRThrottle, CThrottle>, CThrottle.Set);
            FlightButton("Throttle Increase", "Throttle", ByManifest<VRThrottle, CThrottle>, CThrottle.Increase);
            FlightButton("Throttle Decrease", "Throttle", ByManifest<VRThrottle, CThrottle>, CThrottle.Decrease);

            // Brakes/airbrakes
            FlightAxis("Brakes/Airbrakes Axis", "Throttle", ByManifest<VRThrottle, CThrottle>, CThrottle.Trigger);
            FlightButton("Brakes/Airbrakes", "Throttle", ByManifest<VRThrottle, CThrottle>, CThrottle.Trigger);

            // Flaps and gear
            FlightButton("Flaps Cycle", "Flaps", ByName<VRLever, CLever>, CLever.Cycle, s: -1, n: true);
            FlightButton("Flaps Increase", "Flaps", ByName<VRLever, CLever>, CLever.Next, s: -1, n: true);
            FlightButton("Flaps Decrease", "Flaps", ByName<VRLever, CLever>, CLever.Prev, s: -1, n: true);

            FlightButton("Landing Gear Toggle", "Landing Gear", ByName<VRLever, CLever>, CLever.Cycle, s: -1, n: true);
            FlightButton("Landing Gear Up", "Landing Gear", ByName<VRLever, CLever>, CLever.Set, s: 1, n: true);
            FlightButton("Landing Gear Down", "Landing Gear", ByName<VRLever, CLever>, CLever.Set, s: 0, n: true);

            // Parking brake
            FlightButton("Parking Brake Toggle", "Parking Brake", ByName<VRLever, CLever>, CLever.Cycle, s: -1, n: true);
            FlightButton("Parking Brake On", "Parking Brake", ByName<VRLever, CLever>, CLever.Set, s: 1, n: true);
            FlightButton("Parking Brake Off", "Parking Brake", ByName<VRLever, CLever>, CLever.Set, s: 0, n: true);

            // Weapons
            FlightButton("Fire Weapon", "Joystick", Joysticks, CJoystick.Trigger);
            FlightButton("Cycle Weapons", "Joystick", Joysticks, CJoystick.MenuButton);

            // Misc
            FlightButton("Eject", "Eject", ByType<EjectHandle, CEject>, CEject.Pull, s: -1, n: true);

            AddPostUpdateControl("Joystick");
            AddPostUpdateControl("Throttle");
        }

        protected override void CreateAssistControls()
        {
        }

        protected override void CreateNavigationControls()
        {
            // Autopilot and navigation
            NavButton("A/P Nav Mode", "Navigation Mode", ByName<VRButton, CButton>, CButton.Use, s: -1, n: true);
            NavButton("A/P Spd Hold", "Airspeed Hold", ByName<VRButton, CButton>, CButton.Use, s: -1, n: true);
            NavButton("A/P Hdg Hold", "Heading Hold", ByName<VRButton, CButton>, CButton.Use, s: -1, n: true);
            NavButton("A/P Alt Hold", "Altitude Hold", ByName<VRButton, CButton>, CButton.Use, s: -1, n: true);
            NavButton("A/P Off", "All AP Off", ByName<VRButton, CButton>, CButton.Use, s: -1, n: true);

            // AP adjustments
            NavAxisC("A/P Altitude", "Adjust AP Altitude", ByName<VRTwistKnob, CKnob>, CKnob.Set, s: -1, n: true);
            NavButton("A/P Alt Increase", "Adjust AP Altitude", ByName<VRTwistKnob, CKnob>, CKnob.Increase, s: -1, n: true);
            NavButton("A/P Alt Decrease", "Adjust AP Altitude", ByName<VRTwistKnob, CKnob>, CKnob.Decrease, s: -1, n: true);

            NavButton("A/P Heading Set Right", "Course Right", ByName<VRButton, CButton>, CButton.Use, s: -1, n: true);
            NavButton("A/P Heading Set Left", "Course Left", ByName<VRButton, CButton>, CButton.Use, s: -1, n: true);

            AddPostUpdateControl("Adjust AP Altitude");
        }

        protected override void CreateSystemsControls()
        {
            SystemsButton("Clear Cautions", "Clear Cautions", ByName<VRButton, CButton>, CButton.Use, s: -1, n: true);

            SystemsButton("Master Arm Toggle", "Master Arm", ByName<VRLever, CLever>, CLever.Cycle, s: -1, n: true);
            SystemsButton("Master Arm On", "Master Arm", ByName<VRLever, CLever>, CLever.Set, s: 1, n: true);
            SystemsButton("Master Arm Off", "Master Arm", ByName<VRLever, CLever>, CLever.Set, s: 0, n: true);

            SystemsButton("Engine Toggle", "Engine", ByName<VRLever, CLever>, CLever.Cycle, s: -1, n: true);

            // Battery switch (expected states: 0=Off, 1=On)
            SystemsButton("Battery Toggle", "Battery", ByName<VRLever, CLever>, CLever.Cycle, s: -1, n: true);
            SystemsButton("Battery On", "Battery", ByName<VRLever, CLever>, CLever.Set, s: 1, n: true);
            SystemsButton("Battery Off", "Battery", ByName<VRLever, CLever>, CLever.Set, s: 0, n: true);

            // JFS starter (middle/off, start1, start2) - map positions
            SystemsButton("JFS Off", "JFS Starter", ByName<VRLever, CLever>, CLever.Set, s: 0, n: true);
            SystemsButton("JFS Start 1", "JFS Starter", ByName<VRLever, CLever>, CLever.Set, s: 1, n: true);
            SystemsButton("JFS Start 2", "JFS Starter", ByName<VRLever, CLever>, CLever.Set, s: 2, n: true);

            SystemsButton("Radar Power Toggle", "Radar Power", ByName<VRLever, CLever>, CLever.Cycle, s: -1, n: true);
            SystemsButton("Radar Power On", "Radar Power", ByName<VRLever, CLever>, CLever.Set, s: 1, n: true);
            SystemsButton("Radar Power Off", "Radar Power", ByName<VRLever, CLever>, CLever.Set, s: 0, n: true);

            SystemsButton("TGP Power Toggle", "TGP Power", ByName<VRLever, CLever>, CLever.Cycle, s: -1, n: true);
            SystemsButton("DED Power Toggle", "DED Power", ByName<VRLever, CLever>, CLever.Cycle, s: -1, n: true);
            SystemsButton("RWR Power Toggle", "RWR Power", ByName<VRLever, CLever>, CLever.Cycle, s: -1, n: true);

            // Jettison switch: use the knob-int control if present so interaction start/stop is handled correctly
            SystemsButton("Jettison Execute", "Jettison Switch", ByName<VRTwistKnobInt, CKnobInt>, CKnobInt.Push, s: -1, n: true);
            SystemsButton("Jettison Cycle", "Jettison Switch", ByName<VRTwistKnobInt, CKnobInt>, CKnobInt.Cycle, s: -1, n: true);
            SystemsButton("Jettison Next", "Jettison Switch", ByName<VRTwistKnobInt, CKnobInt>, CKnobInt.Next, s: -1, n: true);
            SystemsButton("Jettison Prev", "Jettison Switch", ByName<VRTwistKnobInt, CKnobInt>, CKnobInt.Prev, s: -1, n: true);

            SystemsButton("Flares Toggle", "Toggle Flares", ByName<VRLever, CLever>, CLever.Cycle, s: -1, n: true);
            SystemsButton("Chaff Toggle", "Toggle Chaff", ByName<VRLever, CLever>, CLever.Cycle, s: -1, n: true);
            SystemsButton("Jammer Toggle", "Toggle Jammer", ByName<VRLever, CLever>, CLever.Cycle, s: -1, n: true);
        }

        protected override void CreateHUDControls()
        {
            HUDButton("Helmet Visor Toggle", "Toggle Visor", HelmetController, CHelmet.ToggleVisor, s: -1, n: true);
            HUDButton("Helmet NV Toggle", "Toggle NVG", HelmetController, CHelmet.ToggleNightVision, s: -1, n: true);

            HUDButton("HUD Power Toggle", "HUD Power", ByName<VRLever, CLever>, CLever.Cycle, s: -1, n: true);
            HUDButton("HUD Tint", "HUD Tint", ByName<VRTwistKnob, CKnob>, CKnob.Set, s: -1, n: true);
            HUDButton("HUD Brightness", "HUD Brightness", ByName<VRTwistKnob, CKnob>, CKnob.Set, s: -1, n: true);

            HUDButton("HMCS Power Toggle", "HMCS Power", ByName<VRLever, CLever>, CLever.Cycle, s: -1, n: true);

            // ICP: COM and Master Modes
            HUDButton("COM1", "COM1", ByName<VRButton, CButton>, CButton.Use, s: -1, n: true);
            HUDButton("COM2", "COM2", ByName<VRButton, CButton>, CButton.Use, s: -1, n: true);
            HUDButton("A/A Master Mode", "Air To Air Master Mode", ByName<VRButton, CButton>, CButton.Use, s: -1, n: true);
            HUDButton("A/G Master Mode", "Air To Ground Master Mode", ByName<VRButton, CButton>, CButton.Use, s: -1, n: true);
        }

        protected override void CreateNumPadControls()
        {
            NumPadButton("1", "1 / Swap Radio Frequency", ByName<VRButton, CButton>, CButton.Use, r: Dash);
            NumPadButton("2", "2 / Set Low Altitude Warning", ByName<VRButton, CButton>, CButton.Use, r: Dash);
            NumPadButton("3", "3 / Set ILS Frequency", ByName<VRButton, CButton>, CButton.Use, r: Dash);
            NumPadButton("4", "4 / Set AP Altitude", ByName<VRButton, CButton>, CButton.Use, r: Dash);
            NumPadButton("5", "5 / Set AP Heading", ByName<VRButton, CButton>, CButton.Use, r: Dash);
            NumPadButton("6", "6 / Set AP Speed", ByName<VRButton, CButton>, CButton.Use, r: Dash);
            NumPadButton("7", "7 / Swap MFDs", ByName<VRButton, CButton>, CButton.Use, r: Dash);
            NumPadButton("8", "8 / Set TGP Laser Code", ByName<VRButton, CButton>, CButton.Use, r: Dash);
            NumPadButton("9", "9 / Set Seeker Code", ByName<VRButton, CButton>, CButton.Use, r: Dash);
            NumPadButton("0", "0", ByName<VRButton, CButton>, CButton.Use, r: Dash);
            NumPadButton("Enter", "Enter", ByName<VRButton, CButton>, CButton.Use, r: Dash);
            NumPadButton("Clear", "Clear", ByName<VRButton, CButton>, CButton.Use, r: Dash);
        }

        protected override void CreateDisplayControls()
        {
            // MFD basic controls
            DisplayButton("MFD Left Power", "Power (MMFD Left)", ByName<VRButton, CButton>, CButton.Use, s: -1, n: true);
            DisplayButton("MFD Right Power", "Power (MMFD Right)", ByName<VRButton, CButton>, CButton.Use, s: -1, n: true);

            DisplayAxis("MFD Brightness", "MFD Brightness", ByName<VRTwistKnob, CKnob>, CKnob.Set, s: -1, n: true);
            DisplayButton("MFD Brightness Increase", "MFD Brightness", ByName<VRTwistKnob, CKnob>, CKnob.Increase, s: -1, n: true);
            DisplayButton("MFD Brightness Decrease", "MFD Brightness", ByName<VRTwistKnob, CKnob>, CKnob.Decrease, s: -1, n: true);

            // TSD GPS-S not present on F-16; omitted

            // SOI (slew + axes + directional buttons)
            DisplayButton("SOI Slew Button", "SOI", SOI, CSOI.SlewButton);
            DisplayAxisC("SOI Slew X", "SOI", SOI, CSOI.SlewX);
            DisplayAxisC("SOI Slew Y", "SOI", SOI, CSOI.SlewY);
            DisplayButton("SOI Slew Up", "SOI", SOI, CSOI.SlewUp);
            DisplayButton("SOI Slew Right", "SOI", SOI, CSOI.SlewRight);
            DisplayButton("SOI Slew Down", "SOI", SOI, CSOI.SlewDown);
            DisplayButton("SOI Slew Left", "SOI", SOI, CSOI.SlewLeft);
            DisplayButton("SOI Next", "SOI", SOI, CSOI.Next);
            DisplayButton("SOI Prev", "SOI", SOI, CSOI.Prev);
            DisplayButton("SOI Zoom In", "SOI", SOI, CSOI.ZoomIn);
            DisplayButton("SOI Zoom Out", "SOI", SOI, CSOI.ZoomOut);

            // Map MFD buttons for Left, Center, Right (indices 0,1,2)
            DisplayButton("MFD Left Toggle", "MFD Left", MFD, CMFD.PowerToggle, i: 0);
            DisplayButton("MFD Left On", "MFD Left", MFD, CMFD.PowerOn, i: 0);
            DisplayButton("MFD Left Off", "MFD Left", MFD, CMFD.PowerOff, i: 0);
            DisplayButton("MFD Left L1", "MFD Left", MFD, CMFD.Press, (int)MFDButtons.L1, i: 0);
            DisplayButton("MFD Left L2", "MFD Left", MFD, CMFD.Press, (int)MFDButtons.L2, i: 0);
            DisplayButton("MFD Left L3", "MFD Left", MFD, CMFD.Press, (int)MFDButtons.L3, i: 0);
            DisplayButton("MFD Left L4", "MFD Left", MFD, CMFD.Press, (int)MFDButtons.L4, i: 0);
            DisplayButton("MFD Left L5", "MFD Left", MFD, CMFD.Press, (int)MFDButtons.L5, i: 0);
            DisplayButton("MFD Left R1", "MFD Left", MFD, CMFD.Press, (int)MFDButtons.R1, i: 0);
            DisplayButton("MFD Left R2", "MFD Left", MFD, CMFD.Press, (int)MFDButtons.R2, i: 0);
            DisplayButton("MFD Left R3", "MFD Left", MFD, CMFD.Press, (int)MFDButtons.R3, i: 0);
            DisplayButton("MFD Left R4", "MFD Left", MFD, CMFD.Press, (int)MFDButtons.R4, i: 0);
            DisplayButton("MFD Left R5", "MFD Left", MFD, CMFD.Press, (int)MFDButtons.R5, i: 0);
            DisplayButton("MFD Left T1", "MFD Left", MFD, CMFD.Press, (int)MFDButtons.T1Home, i: 0);
            DisplayButton("MFD Left T2", "MFD Left", MFD, CMFD.Press, (int)MFDButtons.T2, i: 0);
            DisplayButton("MFD Left T3", "MFD Left", MFD, CMFD.Press, (int)MFDButtons.T3, i: 0);
            DisplayButton("MFD Left T4", "MFD Left", MFD, CMFD.Press, (int)MFDButtons.T4, i: 0);
            DisplayButton("MFD Left T5", "MFD Left", MFD, CMFD.Press, (int)MFDButtons.T5, i: 0);
            DisplayButton("MFD Left B1", "MFD Left", MFD, CMFD.Press, (int)MFDButtons.B1, i: 0);
            DisplayButton("MFD Left B2", "MFD Left", MFD, CMFD.Press, (int)MFDButtons.B2, i: 0);
            DisplayButton("MFD Left B3", "MFD Left", MFD, CMFD.Press, (int)MFDButtons.B3, i: 0);
            DisplayButton("MFD Left B4", "MFD Left", MFD, CMFD.Press, (int)MFDButtons.B4, i: 0);
            DisplayButton("MFD Left B5", "MFD Left", MFD, CMFD.Press, (int)MFDButtons.B5, i: 0);

            DisplayButton("MFD Center Toggle", "MFD Center", MFD, CMFD.PowerToggle, i: 1);
            DisplayButton("MFD Center On", "MFD Center", MFD, CMFD.PowerOn, i: 1);
            DisplayButton("MFD Center Off", "MFD Center", MFD, CMFD.PowerOff, i: 1);
            DisplayButton("MFD Center L1", "MFD Center", MFD, CMFD.Press, (int)MFDButtons.L1, i: 1);
            DisplayButton("MFD Center L2", "MFD Center", MFD, CMFD.Press, (int)MFDButtons.L2, i: 1);
            DisplayButton("MFD Center L3", "MFD Center", MFD, CMFD.Press, (int)MFDButtons.L3, i: 1);
            DisplayButton("MFD Center L4", "MFD Center", MFD, CMFD.Press, (int)MFDButtons.L4, i: 1);
            DisplayButton("MFD Center L5", "MFD Center", MFD, CMFD.Press, (int)MFDButtons.L5, i: 1);
            DisplayButton("MFD Center R1", "MFD Center", MFD, CMFD.Press, (int)MFDButtons.R1, i: 1);
            DisplayButton("MFD Center R2", "MFD Center", MFD, CMFD.Press, (int)MFDButtons.R2, i: 1);
            DisplayButton("MFD Center R3", "MFD Center", MFD, CMFD.Press, (int)MFDButtons.R3, i: 1);
            DisplayButton("MFD Center R4", "MFD Center", MFD, CMFD.Press, (int)MFDButtons.R4, i: 1);
            DisplayButton("MFD Center R5", "MFD Center", MFD, CMFD.Press, (int)MFDButtons.R5, i: 1);
            DisplayButton("MFD Center T1", "MFD Center", MFD, CMFD.Press, (int)MFDButtons.T1Home, i: 1);
            DisplayButton("MFD Center T2", "MFD Center", MFD, CMFD.Press, (int)MFDButtons.T2, i: 1);
            DisplayButton("MFD Center T3", "MFD Center", MFD, CMFD.Press, (int)MFDButtons.T3, i: 1);
            DisplayButton("MFD Center T4", "MFD Center", MFD, CMFD.Press, (int)MFDButtons.T4, i: 1);
            DisplayButton("MFD Center T5", "MFD Center", MFD, CMFD.Press, (int)MFDButtons.T5, i: 1);
            DisplayButton("MFD Center B1", "MFD Center", MFD, CMFD.Press, (int)MFDButtons.B1, i: 1);
            DisplayButton("MFD Center B2", "MFD Center", MFD, CMFD.Press, (int)MFDButtons.B2, i: 1);
            DisplayButton("MFD Center B3", "MFD Center", MFD, CMFD.Press, (int)MFDButtons.B3, i: 1);
            DisplayButton("MFD Center B4", "MFD Center", MFD, CMFD.Press, (int)MFDButtons.B4, i: 1);
            DisplayButton("MFD Center B5", "MFD Center", MFD, CMFD.Press, (int)MFDButtons.B5, i: 1);

            DisplayButton("MFD Right Toggle", "MFD Right", MFD, CMFD.PowerToggle, i: 2);
            DisplayButton("MFD Right On", "MFD Right", MFD, CMFD.PowerOn, i: 2);
            DisplayButton("MFD Right Off", "MFD Right", MFD, CMFD.PowerOff, i: 2);
            DisplayButton("MFD Right L1", "MFD Right", MFD, CMFD.Press, (int)MFDButtons.L1, i: 2);
            DisplayButton("MFD Right L2", "MFD Right", MFD, CMFD.Press, (int)MFDButtons.L2, i: 2);
            DisplayButton("MFD Right L3", "MFD Right", MFD, CMFD.Press, (int)MFDButtons.L3, i: 2);
            DisplayButton("MFD Right L4", "MFD Right", MFD, CMFD.Press, (int)MFDButtons.L4, i: 2);
            DisplayButton("MFD Right L5", "MFD Right", MFD, CMFD.Press, (int)MFDButtons.L5, i: 2);
            DisplayButton("MFD Right R1", "MFD Right", MFD, CMFD.Press, (int)MFDButtons.R1, i: 2);
            DisplayButton("MFD Right R2", "MFD Right", MFD, CMFD.Press, (int)MFDButtons.R2, i: 2);
            DisplayButton("MFD Right R3", "MFD Right", MFD, CMFD.Press, (int)MFDButtons.R3, i: 2);
            DisplayButton("MFD Right R4", "MFD Right", MFD, CMFD.Press, (int)MFDButtons.R4, i: 2);
            DisplayButton("MFD Right R5", "MFD Right", MFD, CMFD.Press, (int)MFDButtons.R5, i: 2);
            DisplayButton("MFD Right T1", "MFD Right", MFD, CMFD.Press, (int)MFDButtons.T1Home, i: 2);
            DisplayButton("MFD Right T2", "MFD Right", MFD, CMFD.Press, (int)MFDButtons.T2, i: 2);
            DisplayButton("MFD Right T3", "MFD Right", MFD, CMFD.Press, (int)MFDButtons.T3, i: 2);
            DisplayButton("MFD Right T4", "MFD Right", MFD, CMFD.Press, (int)MFDButtons.T4, i: 2);
            DisplayButton("MFD Right T5", "MFD Right", MFD, CMFD.Press, (int)MFDButtons.T5, i: 2);
            DisplayButton("MFD Right B1", "MFD Right", MFD, CMFD.Press, (int)MFDButtons.B1, i: 2);
            DisplayButton("MFD Right B2", "MFD Right", MFD, CMFD.Press, (int)MFDButtons.B2, i: 2);
            DisplayButton("MFD Right B3", "MFD Right", MFD, CMFD.Press, (int)MFDButtons.B3, i: 2);
            DisplayButton("MFD Right B4", "MFD Right", MFD, CMFD.Press, (int)MFDButtons.B4, i: 2);
            DisplayButton("MFD Right B5", "MFD Right", MFD, CMFD.Press, (int)MFDButtons.B5, i: 2);

            AddPostUpdateControl("MFD Brightness");
            AddPostUpdateControl("SOI");
            AddPostUpdateControl("MFD Left");
            AddPostUpdateControl("MFD Center");
            AddPostUpdateControl("MFD Right");
        }

        protected override void CreateRadioControls()
        {
            RadioButton("Radio Transmit", "Radio", ByType<CockpitTeamRadioManager, CRadio>, CRadio.Transmit, s: -1, n: true);

            RadioAxis("Radio Volume", "Radio Volume", ByName<VRTwistKnob, CKnob>, CKnob.Set, s: -1, n: true);
            RadioButton("Prev Song", "Prev Song", ByName<VRButton, CButton>, CButton.Use, s: -1, n: true);
            RadioButton("Next Song", "Next Song", ByName<VRButton, CButton>, CButton.Use, s: -1, n: true);
            RadioButton("Play/Pause", "Play/Pause", ByName<VRButton, CButton>, CButton.Use, s: -1, n: true);

            RadioButton("Radio Mode Cycle", "Radio Mode", ByName<VRTwistKnobInt, CKnobInt>, CKnobInt.Cycle, s: -1, n: true);
            RadioButton("Radio Channel Cycle", "Radio Channel", ByName<VRTwistKnobInt, CKnobInt>, CKnobInt.Cycle, s: -1, n: true);
        }

        protected override void CreateMusicControls()
        {
        }

        protected override void CreateLightsControls()
        {
            // Instrument lights are implemented as a discrete knob on the F-16 (knob-int)
            LightsButton("Instrument Lights Toggle", "Instrument Lights", ByName<VRTwistKnobInt, CKnobInt>, CKnobInt.Cycle, n: true);
            LightsButton("Instrument Lights On", "Instrument Lights", ByName<VRTwistKnobInt, CKnobInt>, CKnobInt.Set, 1, n: true);
            LightsButton("Instrument Lights Off", "Instrument Lights", ByName<VRTwistKnobInt, CKnobInt>, CKnobInt.Set, 0, n: true);

            LightsAxis("Instrument Brightness", "Instrument Brightness", ByName<VRTwistKnob, CKnob>, CKnob.Set, s: -1, n: true);

            // Nav and Strobe lights are knobs on this cockpit - map as knob-int
            LightsButton("Nav Lights Toggle", "Nav Lights", ByName<VRTwistKnobInt, CKnobInt>, CKnobInt.Cycle, n: true);
            LightsButton("Nav Lights On", "Nav Lights", ByName<VRTwistKnobInt, CKnobInt>, CKnobInt.Set, 1, n: true);
            LightsButton("Nav Lights Off", "Nav Lights", ByName<VRTwistKnobInt, CKnobInt>, CKnobInt.Set, 0, n: true);

            LightsButton("Strobe Lights Toggle", "Strobe Lights", ByName<VRTwistKnobInt, CKnobInt>, CKnobInt.Cycle, n: true);
            LightsButton("Strobe Lights On", "Strobe Lights", ByName<VRTwistKnobInt, CKnobInt>, CKnobInt.Set, 1, n: true);
            LightsButton("Strobe Lights Off", "Strobe Lights", ByName<VRTwistKnobInt, CKnobInt>, CKnobInt.Set, 0, n: true);

            // Landing lights: use a generic interactable fallback so mapping succeeds regardless of component type
            LightsButton("Landing Lights Toggle", "LandingLight", ByName<VRInteractable, CInteractable>, CInteractable.Use, n: true);
        }

        protected override void CreateMiscControls()
        {
            MiscButton("Canopy Toggle", "Canopy", ByName<VRLever, CLever>, CLever.Cycle, s: -1, n: true);
            MiscButton("Raise Seat", "Raise Seat", ByName<VRButton, CButton>, CButton.Use, s: -1, n: true);
            MiscButton("Lower Seat", "Lower Seat", ByName<VRButton, CButton>, CButton.Use, s: -1, n: true);

            MiscButton("Fuel Port Toggle", "Fuel Port", ByName<VRLever, CLever>, CLever.Cycle, s: -1, n: true);
            MiscButton("Fuel Dump Toggle", "Fuel Dump Switch", ByName<VRLever, CLever>, CLever.Cycle, s: -1, n: true);
            MiscButton("Fuel Dump Cover Toggle", "Switch Cover (Fuel Dump)", ByName<VRSwitchCover, CLeverCovered>, CLeverCovered.Cycle, s: -1, n: true);

            // If the aircraft has dedicated jettison mark buttons they will be mapped by name elsewhere;
            // otherwise the single 'Jettison Switch' (knob-int) is used above to cycle/execute.
        }
    }
}

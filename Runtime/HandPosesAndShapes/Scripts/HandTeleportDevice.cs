using UnityEditor;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.LowLevel;
/*
namespace VrWeb
{

    public struct HandTeleportDeviceState : IInputStateTypeInfo
    {
        // Hand Teleport Device Nuro
        public FourCC format => new FourCC( 'H', 'T', 'D', 'N');

        [InputControl( name = "selectButton", layout = "Button", bit = 0 )]
        [InputControl( name = "activateButton", layout = "Button", bit = 1 )]
        [InputControl( name = "cancelButton", layout = "Button", bit = 2 )]
        public int buttons;
    }

    [InputControlLayout( stateType= typeof( HandTeleportDeviceState ))]
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class HandTeleportDevice : InputDevice
    {
        public ButtonControl selectButton { get; private set; }
        public ButtonControl activateButton { get; private set; }
        public ButtonControl cancelButton { get; private set; }

        static HandTeleportDevice()
        {
            InputSystem.RegisterLayout < HandTeleportDevice >(
                matches: new InputDeviceMatcher().WithInterface( pattern: "HandTeleportDevice"));

            if ( !InputSystem.devices.Any(x => x is HandTeleportDevice ) )
            {
                InputSystem.AddDevice(
                    new InputDeviceDescription { interfaceName = "HandTeleportDevice", product = "Nuro Hand Teleport Device" } );
            }
            InputSystem.AddDevice < HandTeleportDevice >();
        }

        protected override void FinishSetup()
        {
            base.FinishSetup();

            selectButton = GetChildControl < ButtonControl >( path: "selectButton" );
            activateButton = GetChildControl < ButtonControl >( path: "activateButton" );
            cancelButton = GetChildControl< ButtonControl >( path: "cancelButton" );
        }
    }
}
*/
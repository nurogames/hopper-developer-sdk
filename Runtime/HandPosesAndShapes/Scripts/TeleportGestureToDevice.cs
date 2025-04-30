using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
/*
using VrWeb;

public class TeleportGestureToDevice : MonoBehaviour
{
    [SerializeField]
    GameObject m_TeleportInteractor;

    private HandTeleportDevice m_HandTeleportDevice;
    private Coroutine m_TryTeleportCoroutine = null;

    private void Start()
    {
        m_HandTeleportDevice = null;

        foreach ( InputDevice inputDevice in InputSystem.devices )
        {
            if ( inputDevice is HandTeleportDevice )
            {
                m_HandTeleportDevice = inputDevice as HandTeleportDevice;

                break;
            }
        }

        if ( m_HandTeleportDevice != null )
            InputSystem.QueueStateEvent( m_HandTeleportDevice, new HandTeleportDeviceState() { buttons = 0 } );
    }

    public void OnBeginTeleport()
    {
        if ( m_TryTeleportCoroutine == null )
        {
            if ( m_HandTeleportDevice != null )
            {
                InputSystem.QueueStateEvent( m_HandTeleportDevice, new HandTeleportDeviceState() { buttons = 1 } );
            }

            m_TryTeleportCoroutine = StartCoroutine( TeleportCoroutine() );
        }
    }

    private IEnumerator TeleportCoroutine()
    {
        m_TeleportInteractor.SetActive( true );

        yield return new WaitForSeconds( 0.3f );

        if ( m_HandTeleportDevice != null )
        {
            InputSystem.QueueStateEvent( m_HandTeleportDevice, new HandTeleportDeviceState() { buttons = 6 } );
        }

        yield return null;

        InputSystem.QueueStateEvent( m_HandTeleportDevice, new HandTeleportDeviceState() { buttons = 0 } );

        yield return null;

        m_TryTeleportCoroutine = null;
    }
}
*/
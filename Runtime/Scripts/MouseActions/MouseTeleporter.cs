using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using VRWeb.Events;
using VRWeb.Managers;
using VRWeb.Rig;

namespace VRWeb.MouseActions
{
    public class MouseTeleporter : HopperManagerMonoBehaviour < MouseTeleporter >
    {
        [SerializeField]
        private LayerMask m_TeleportLayerMask;

        [SerializeField]
        private float m_MaxTeleportDistance = 30.0f;
        
        public GameObject HoveredFloor { get; private set; }
        public RaycastHit HoveredRaycastHit => m_HoveredRaycastHit;

        private RaycastHit m_HoveredRaycastHit;
        private List < Collider > m_CollidersList = new();

        private void Awake()
        {
            RegisterManager();
        }

        public void OnEnterScene(SceneLoadedEvent.LoadStatus loadStatus)
        {
            if (loadStatus != SceneLoadedEvent.LoadStatus.finishLoad)
                return;

            m_CollidersList.Clear();
            TeleportationArea[] areas = FindObjectsByType<TeleportationArea>( 
                FindObjectsInactive.Exclude, 
                FindObjectsSortMode.None );

            if ( areas.Length == 0 )
                return;

            foreach ( TeleportationArea area in areas )
            {
                m_CollidersList.AddRange( area.colliders );
            }
        }

        private void Update()
        {
            ViewModeSwitcher vms = HopperRoot.Get < ViewModeSwitcher >();

            if ( vms == null || vms.IsInVrMode )
                return;

            Camera cam = HopperRoot.Get < MouseHandler >().Camera;

            if ( cam == null || HopperRoot.Get < MouseHandler >() == null )
                return;

            HoveredFloor = GetClosestRaycastHitTeleport(out RaycastHit raycastHit );
            if (HoveredFloor != null)
                m_HoveredRaycastHit = raycastHit;
        }

        private GameObject GetClosestRaycastHitTeleport( out RaycastHit raycastHit )
        {
            raycastHit = new RaycastHit();

            RaycastHit[] raycastHits = HopperRoot.Get < MouseHandler >().TestHit( m_TeleportLayerMask );

            if ( raycastHits.Length == 0 )
            {
                return null;
            }

            // Sort hits by distance
            System.Array.Sort( raycastHits, ( a, b ) => a.distance.CompareTo( b.distance ) );
            RaycastHit rh = raycastHits[0];

            if ( m_CollidersList.Contains( rh.collider ) && rh.distance <= m_MaxTeleportDistance )
            {
                raycastHit = raycastHits[0];
                return raycastHits[0].transform.gameObject;
            }

            return null;
        }
    }

}

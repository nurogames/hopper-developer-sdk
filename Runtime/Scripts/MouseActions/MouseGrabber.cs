#if !VRWEB_TOOLKIT_ONLY
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using VRWeb.Managers;
using VRWeb.Rig;
using WorldBuilder.Core.Components;

namespace VRWeb.MouseActions
{
	public class MouseGrabber : HopperManagerMonoBehaviour<MouseGrabber>
    {
        private enum Movement
        {
            none,
            vertical,
            horizontal
        }

        // Serialized variables
        [SerializeField] private LayerMask m_InteractableLayerMask;

        // Private const variables
        private const float ROTATION_SPEED = 4f;

        // Private variables
        private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable m_GrabbedInteractable = null;
        private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable m_HoveredInteractable = null;
        private RaycastHit m_HoveredRaycastHit;
        private Vector3 m_GrabPointToGrabbableVector = Vector3.negativeInfinity;
        private float m_DistanceRayOriginToGrabPoint;
        private bool m_IsControlKeyPressed = false;
        private bool m_RotateYKeyPressed = false;
        private bool m_RotateZKeyPressed = false;

        private bool? m_IsKinematicOriginalState = null;
        private bool[] m_IsTriggerOriginalState = null;
        private bool?[] m_ConvexOriginalState = null;

        private InputActionManager m_InputActionManager = null;

        private Rigidbody m_GrabbedRigidbody = null;

        private bool m_CanMoveToggleButtonValue;

        private Plane m_CurrentMovementPlane;

        private Movement m_CurrentMovement;

        // Getter
        public XRGrabInteractable GrabbedInteractable => m_GrabbedInteractable;
        public XRGrabInteractable HoveredInteractable => m_HoveredInteractable;

        // Awake
        private void Awake()
        {
            RegisterManager();
        }

        private void Start()
        {
            m_InputActionManager = FindAnyObjectByType<InputActionManager>();
        }

        // Update
        private void Update()
        {
            ViewModeSwitcher vms = HopperRoot.Get<ViewModeSwitcher>();

            if (vms == null || vms.IsInVrMode)
                return;

            Camera cam = HopperRoot.Get<MouseHandler>().Camera;

            if (cam == null || HopperRoot.Get < MouseHandler >() == null)
                return;

            m_HoveredInteractable = GetClosestRaycastHitGrabbable( HopperRoot.Get < MouseHandler >(), out RaycastHit raycastHit);
            m_HoveredRaycastHit = raycastHit;

            if (m_GrabbedInteractable == null || m_GrabbedRigidbody == null)
                return;

            Vector2 mousePosition = HopperRoot.Get<MouseHandler>().MousePosition.ReadValue<Vector2>();

            MoveGrabbedInteractable(mousePosition, cam);
        }

        // Public functions
        public void OnMouseButtonDown()
        {
            if (HopperRoot.Get<ViewModeSwitcher>().IsInVrMode)
                return;

            if (m_HoveredInteractable == null)
                return;

            m_GrabbedInteractable = m_HoveredInteractable;

            GrabInteractable(m_HoveredRaycastHit);

            m_CanMoveToggleButtonValue = HopperRoot.Get<ViewModeSwitcher>().CanMoveToggleButton.isOn;

            HopperRoot.Get<ViewModeSwitcher>().CanMoveToggleButton.isOn = true;
        }

        public void OnMouseButtonUp()
        {
            if (HopperRoot.Get<ViewModeSwitcher>().IsInVrMode)
                return;

            if (m_GrabbedInteractable == null)
                return;

            ReleaseGrabbedInteractable();

            m_GrabbedInteractable = null;

            RestoreCanMove();
        }

        public void RestoreCanMove()
        {
            HopperRoot.Get<ViewModeSwitcher>().CanMoveToggleButton.isOn = m_CanMoveToggleButtonValue;
        }

        public void OnControlKeyDown()
        {
            m_IsControlKeyPressed = true;
        }

        public void OnControlKeyUp()
        {
            m_IsControlKeyPressed = false;
        }

        public void OnRotateYKeyDown()
        {
            m_RotateYKeyPressed = true;
        }

        public void OnRotateYKeyUp()
        {
            m_RotateYKeyPressed = false;
        }

        public void OnRotateZKeyDown()
        {
            m_RotateZKeyPressed = true;
        }

        public void OnRotateZKeyUp()
        {
            m_RotateZKeyPressed = false;
        }

        public void ReleaseGrabbedInteractable()
        {
            MouseGrabbable mouseGrabbable = m_GrabbedInteractable.gameObject.GetComponent<MouseGrabbable>();

            if (mouseGrabbable != null)
                Destroy(mouseGrabbable);

            Rigidbody rb = m_GrabbedInteractable.GetComponent<Rigidbody>();

            if (rb != null && m_IsKinematicOriginalState != null)
                rb.isKinematic = (bool)m_IsKinematicOriginalState;

            m_IsKinematicOriginalState = null;
            m_GrabbedRigidbody = null;

            UndoDisableCollisionsWhileGrabbed();

            VisualObjectData visualObjectData = m_GrabbedInteractable.GetComponent<VisualObjectData>();

            if (visualObjectData != null)
                visualObjectData.OnSelectExited();
            else
            {
                ScreenData screenData = m_GrabbedInteractable.GetComponent<ScreenData>();
                if (screenData != null)
                    screenData.OnSelectExited();
            }

            m_GrabbedInteractable = null;

            m_GrabPointToGrabbableVector = Vector3.negativeInfinity;

            m_CurrentMovement = Movement.none;

            if (m_InputActionManager == null)
                m_InputActionManager = FindAnyObjectByType<InputActionManager>();

            m_InputActionManager?.actionAssets[0].FindAction("Zoom").Enable();
        }

        public void RotateGrabbedInteractableForward()
        {
            if (HopperRoot.Get<ViewModeSwitcher>().IsInVrMode)
                return;

            if (m_GrabbedInteractable == null)
                return;

            if (m_RotateYKeyPressed)
                m_GrabbedInteractable.transform.localRotation *= Quaternion.Euler(Vector3.up * ROTATION_SPEED);
            else if (m_RotateZKeyPressed)
                m_GrabbedInteractable.transform.localRotation *= Quaternion.Euler(Vector3.forward * ROTATION_SPEED);
            else
                m_GrabbedInteractable.transform.localRotation *= Quaternion.Euler(Vector3.right * ROTATION_SPEED);
        }

        public void RotateGrabbedInteractableBackwards()
        {
            if (HopperRoot.Get<ViewModeSwitcher>().IsInVrMode)
                return;

            if (m_GrabbedInteractable == null)
                return;

            if (m_RotateYKeyPressed)
                m_GrabbedInteractable.transform.localRotation *= Quaternion.Euler(Vector3.down * ROTATION_SPEED);
            else if (m_RotateZKeyPressed)
                m_GrabbedInteractable.transform.localRotation *= Quaternion.Euler(Vector3.back * ROTATION_SPEED);
            else
                m_GrabbedInteractable.transform.localRotation *= Quaternion.Euler(Vector3.left * ROTATION_SPEED);
        }

        // Private functions
        private XRGrabInteractable GetClosestRaycastHitGrabbable(MouseHandler mouseHandler, out RaycastHit raycastHit)
        {
            raycastHit = new RaycastHit();

            if (mouseHandler == null)
                return null;

            RaycastHit[] raycastHits = mouseHandler.TestHit(m_InteractableLayerMask);

            if (raycastHits.Length == 0)
                return null;

            // Sort hits by distance
            System.Array.Sort(raycastHits, (a, b) => a.distance.CompareTo(b.distance));

            // Find first XRGrabInteractable 
            foreach (RaycastHit hit in raycastHits)
            {
                GameObject hitObject = hit.transform.gameObject;

                if (hitObject != null && hitObject.GetComponent<XRGrabInteractable>() != null)
                {
                    raycastHit = hit;

                    return hitObject.GetComponent<XRGrabInteractable>();
                }
            }

            return null;
        }

        private void GrabInteractable(RaycastHit hit)
        {
            m_GrabPointToGrabbableVector = m_GrabbedInteractable.transform.position - hit.point;

            m_DistanceRayOriginToGrabPoint = hit.distance;

            MouseGrabbable mouseGrabbable = m_GrabbedInteractable.gameObject.AddComponent<MouseGrabbable>();
            mouseGrabbable.SetMouseGrabber(this);

            Rigidbody rb = m_GrabbedInteractable.GetComponent<Rigidbody>();

            if (rb != null)
            {
                m_IsKinematicOriginalState = rb.isKinematic;
                rb.isKinematic = true;
                m_GrabbedRigidbody = rb;
            }

            DisableCollisionsWhileGrabbed();

            if (m_InputActionManager == null)
                m_InputActionManager = FindAnyObjectByType<InputActionManager>();

            m_InputActionManager?.actionAssets[0].FindAction("Zoom").Disable();

            VisualObjectData visualObjectData = m_GrabbedInteractable.GetComponent<VisualObjectData>();

            if (visualObjectData != null)
                visualObjectData.OnSelectEntered();
            else
            {
                ScreenData screenData = m_GrabbedInteractable.GetComponent<ScreenData>();
                if (screenData != null)
                    screenData.OnSelectEntered();
            }
        }

        private void DisableCollisionsWhileGrabbed()
        {
            Collider[] colliders = m_GrabbedInteractable.gameObject.GetComponentsInChildren<Collider>();

            m_IsTriggerOriginalState = new bool[colliders.Length];
            m_ConvexOriginalState = new bool?[colliders.Length];

            for (int index = 0; index < colliders.Length; index++)
            {
                Collider c = colliders[index];

                if (c is MeshCollider meshCollider)
                {
                    m_ConvexOriginalState[index] = meshCollider.convex;
                    meshCollider.convex = true;
                }

                m_IsTriggerOriginalState[index] = c.isTrigger;
                c.isTrigger = true;
            }
        }

        private void UndoDisableCollisionsWhileGrabbed()
        {
            Collider[] colliders = m_GrabbedInteractable.gameObject.GetComponentsInChildren<Collider>();

            for (int index = 0; index < colliders.Length; index++)
            {
                Collider c = colliders[index];

                c.isTrigger = m_IsTriggerOriginalState[index];

                if (c is MeshCollider meshCollider)
                {
                    if(m_ConvexOriginalState[index] != null)
                        meshCollider.convex = (bool)m_ConvexOriginalState[index];
                }
            }

            m_IsTriggerOriginalState = null;
        }

        private void MoveGrabbedInteractable(Vector2 mousePosition, Camera camera)
        {
            Ray ray = camera.ScreenPointToRay(mousePosition);

            if (m_GrabbedRigidbody != null)
                m_GrabbedRigidbody.isKinematic = true;

            if (!m_IsControlKeyPressed)
                MoveOnHorizontalPlane(ray);
            else
                MoveOnVerticalPlane(ray);
        }

        private void MoveOnSphereAroundCamera(Ray ray)
        {
            m_GrabbedInteractable.transform.position =
                ray.GetPoint(m_DistanceRayOriginToGrabPoint) + m_GrabPointToGrabbableVector;
        }

        private void MoveOnHorizontalPlane(Ray ray)
        {
            if (m_CurrentMovement != Movement.horizontal)
            {
                float grabPointY = m_GrabbedInteractable.transform.position.y - m_GrabPointToGrabbableVector.y;
                Vector3 planeAnchorPoint = Vector3.up * grabPointY;
                m_CurrentMovementPlane = new Plane(Vector3.up, planeAnchorPoint);

                m_CurrentMovement = Movement.horizontal;
            }

            MoveOnPlane(ray);
        }

        private void MoveOnVerticalPlane(Ray ray)
        {
            if (m_CurrentMovement != Movement.vertical)
            {
                Vector3 planeAnchorPoint = ray.GetPoint(m_DistanceRayOriginToGrabPoint);
                Vector3 planeNormalVector = Vector3.ProjectOnPlane(ray.direction, Vector3.up);
                m_CurrentMovementPlane = new Plane(planeNormalVector, planeAnchorPoint);

                m_CurrentMovement = Movement.vertical;
            }

            MoveOnPlane(ray);
        }

        private void MoveOnPlane(Ray ray)
        {
            m_CurrentMovementPlane.Raycast(ray, out float enterDistance);

            if (enterDistance >= 0f)
            {
                enterDistance = Mathf.Min(100f, enterDistance);

                m_GrabbedInteractable.transform.position = ray.GetPoint(enterDistance) + m_GrabPointToGrabbableVector;

                m_DistanceRayOriginToGrabPoint = enterDistance;
            }
            else
                m_GrabbedInteractable.transform.position = ray.GetPoint(m_DistanceRayOriginToGrabPoint) + m_GrabPointToGrabbableVector;
        }
    }
}
#endif

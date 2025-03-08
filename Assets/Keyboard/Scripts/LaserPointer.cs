
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

enum TrackingTarget
{
    LeftHand,
    RightHand
}

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class LaserPointer : UdonSharpBehaviour
{
    [SerializeField] private LayerMask collideLayer;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Keyboard keyboard;
    [SerializeField] private TrackingTarget _trackingTarget;
    [SerializeField] private float _spaceOffset = 0.05f;
    [SerializeField] private float _RotationOffset = 40.0f;
    [SerializeField] private bool _debug = false;
    private LineRenderer lineRenderer;
    private bool _ontrigger = false;
    private float _lineWidth = 0.005f;
    private VRCPlayerApi _attachedPlayer;
    void Start()
    {
        _attachedPlayer = Networking.LocalPlayer;
        _spaceOffset = _trackingTarget == TrackingTarget.LeftHand ? -_spaceOffset : _spaceOffset;
        transform.localPosition = new Vector3(_spaceOffset, 0.0f, 0.0f);
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = _lineWidth;
        lineRenderer.endWidth = _lineWidth;
    }
    void Update()
    {
        if (!_debug)
        {
            if (_attachedPlayer == null)
            {
                _attachedPlayer = Networking.LocalPlayer;
                return;
            }
            var trackingDataType = _trackingTarget == TrackingTarget.LeftHand ? VRCPlayerApi.TrackingDataType.LeftHand : VRCPlayerApi.TrackingDataType.RightHand;
            var trackingData = _attachedPlayer.GetTrackingData(trackingDataType);
            transform.position = trackingData.position;
            transform.rotation = trackingData.rotation;
            transform.Rotate(0, _RotationOffset, 0);
        }

        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100.0f, collideLayer, queryTriggerInteraction: QueryTriggerInteraction.Collide))
        {
            Vector3 hitPoint = hit.point;
            Vector3[] positions = new Vector3[]
            {
                transform.position,
                hitPoint
            };
            lineRenderer.SetPositions(positions);
            lineRenderer.enabled = true;
            if (((1 << hit.collider.gameObject.layer) & layerMask.value) == 0)
            {
                return;
            }
            string _pointKeyName = hit.collider.gameObject.name;
            var inputTrigger = Input.GetAxis(_trackingTarget == TrackingTarget.LeftHand ? "Oculus_CrossPlatform_PrimaryIndexTrigger" : "Oculus_CrossPlatform_SecondaryIndexTrigger");
            if (inputTrigger > 0.75f && !_ontrigger)
            {
                keyboard.OnInteractKey(_pointKeyName);
                _ontrigger = true;
            }
            if (inputTrigger < 0.15f)
            {
                _ontrigger = false;
            }
        }
        else
        {
            Vector3[] positions = new Vector3[]
            {
                transform.position,
                transform.position + transform.forward * 100.0f
            };
            lineRenderer.SetPositions(positions);
            
        }
    }
}


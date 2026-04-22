using UnityEngine;

namespace MultiplayFishing.Gameplay
{
    /// <summary>
    /// Renders a fishing line in two segments:
    /// fixed rod path (reel -> guides -> tip) and cast path (tip -> hook).
    /// </summary>
    public class FishingLineVisual : MonoBehaviour
    {
        [Header("Renderers")]
        [SerializeField] private LineRenderer rodLineFixed;
        [SerializeField] private LineRenderer rodLineCast;

        [Header("Points")]
        [SerializeField] private Transform reelPoint;
        [SerializeField] private Transform[] guidePoints;
        [SerializeField] private Transform tipPoint;
        [SerializeField] private Transform hookPoint;

        [Header("Hook Offsets")]
        [SerializeField] private Vector3 idleHookOffset = new Vector3(0f, 0f, 0.1f);
        [SerializeField] private Vector3 castHookOffset = new Vector3(0f, 0f, 3f);

        private bool isFishingActive;
        private bool isHookControlledByRope;

        public bool HasHookPoints => tipPoint != null && hookPoint != null;
        public bool IsConfiguredForRuntime => HasHookPoints && (rodLineFixed != null || rodLineCast != null);

        private void Awake()
        {
            if (!IsConfiguredForRuntime)
            {
                enabled = false;
                return;
            }

            ApplyHookPosition();
            RefreshLines();
        }

        private void LateUpdate()
        {
            if (!IsConfiguredForRuntime)
            {
                return;
            }

            RefreshLines();
        }

        private void OnValidate()
        {
            if (!IsConfiguredForRuntime)
            {
                return;
            }

            ApplyHookPosition();
            RefreshLines();
        }

        public void SetFishingActive(bool active)
        {
            isFishingActive = active;

            if (!IsConfiguredForRuntime)
            {
                return;
            }

            ApplyHookPosition();
            RefreshLines();
        }

        public void SetHookControlledByRope(bool controlledByRope)
        {
            isHookControlledByRope = controlledByRope;
        }

        public Vector3 GetIdleHookWorldPosition()
        {
            return GetHookWorldPosition(false);
        }

        public Vector3 GetCastHookWorldPosition()
        {
            return GetHookWorldPosition(true);
        }

        public Vector3 GetHookWorldPosition(bool active)
        {
            if (tipPoint == null)
            {
                return hookPoint != null ? hookPoint.position : transform.position;
            }

            Vector3 offset = active ? castHookOffset : idleHookOffset;
            return tipPoint.TransformPoint(offset);
        }

        public void ApplyLineWidth(float width)
        {
            if (rodLineFixed != null)
            {
                rodLineFixed.widthMultiplier = width;
            }

            if (rodLineCast != null)
            {
                rodLineCast.widthMultiplier = width;
            }
        }

        private void ApplyHookPosition()
        {
            // RopeController가 애니메이션 중이면 위치 설정 안함
            if (isHookControlledByRope)
            {
                return;
            }

            if (tipPoint == null || hookPoint == null)
            {
                return;
            }

            hookPoint.position = GetHookWorldPosition(isFishingActive);
        }

        private void RefreshLines()
        {
            RefreshFixedLine();
            RefreshCastLine();
        }

        private void RefreshFixedLine()
        {
            if (rodLineFixed == null || reelPoint == null || tipPoint == null)
            {
                return;
            }

            int guideCount = guidePoints == null ? 0 : guidePoints.Length;
            int pointCount = guideCount + 2;
            rodLineFixed.positionCount = pointCount;
            rodLineFixed.SetPosition(0, reelPoint.position);

            for (int i = 0; i < guideCount; i++)
            {
                Vector3 pointPosition = guidePoints[i] != null ? guidePoints[i].position : tipPoint.position;
                rodLineFixed.SetPosition(i + 1, pointPosition);
            }

            rodLineFixed.SetPosition(pointCount - 1, tipPoint.position);
        }

        private void RefreshCastLine()
        {
            if (rodLineCast == null || tipPoint == null || hookPoint == null)
            {
                return;
            }

            rodLineCast.positionCount = 2;
            rodLineCast.SetPosition(0, tipPoint.position);
            rodLineCast.SetPosition(1, hookPoint.position);
        }
    }
}

using UnityEngine;

namespace MultiplayFishing.Gameplay
{
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

        [Header("Cast Arc")]
        [SerializeField, Min(2)] private int castArcSegments = 16;
        [SerializeField] private float castLineArcHeight = 0.45f;
        [SerializeField] private float castLineArcDistanceRatio = 0.12f;

        private bool isFishingActive;
        private bool isHookControlledByRope;
        private int lastCastArcPositionCount;
        private int lastFixedPositionCount;

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

            if (!isFishingActive)
            {
                ApplyHookPosition();
            }
            RefreshLines();
        }

        public void SetFishingActive(bool active)
        {
            isFishingActive = active;

            if (!IsConfiguredForRuntime)
            {
                return;
            }

            RefreshLines();
        }

        public void SetVisible(bool visible)
        {
            if (rodLineFixed != null)
            {
                rodLineFixed.enabled = visible;
            }

            if (rodLineCast != null)
            {
                rodLineCast.enabled = visible;
            }
        }

        public void SetFishingActiveVisualOnly(bool active)
        {
            isFishingActive = active;
            RefreshLines();
        }

        public void SetHookControlledByRope(bool controlledByRope)
        {
            isHookControlledByRope = controlledByRope;
            RefreshLines();
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
            if (pointCount != lastFixedPositionCount)
            {
                rodLineFixed.positionCount = pointCount;
                lastFixedPositionCount = pointCount;
            }
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

            if (isFishingActive || isHookControlledByRope)
            {
                RefreshCastArcLine();
                return;
            }

            RefreshStraightCastLine();
        }

        private void RefreshStraightCastLine()
        {
            if (lastCastArcPositionCount != 2)
            {
                rodLineCast.positionCount = 2;
                lastCastArcPositionCount = 2;
            }
            rodLineCast.SetPosition(0, tipPoint.position);
            rodLineCast.SetPosition(1, hookPoint.position);
        }

        private void RefreshCastArcLine()
        {
            int segmentCount = Mathf.Max(2, castArcSegments);
            int pointCount = segmentCount + 1;
            if (pointCount != lastCastArcPositionCount)
            {
                rodLineCast.positionCount = pointCount;
                lastCastArcPositionCount = pointCount;
            }

            Vector3 start = tipPoint.position;
            Vector3 end = hookPoint.position;
            Vector3 control = Vector3.Lerp(start, end, 0.5f);
            float distanceArcHeight = Vector3.Distance(start, end) * castLineArcDistanceRatio;
            control.y = Mathf.Max(start.y, end.y) + Mathf.Max(castLineArcHeight, distanceArcHeight);

            for (int i = 0; i <= segmentCount; i++)
            {
                float t = i / (float)segmentCount;
                rodLineCast.SetPosition(i, EvaluateQuadraticBezier(start, control, end, t));
            }
        }

        private static Vector3 EvaluateQuadraticBezier(Vector3 start, Vector3 control, Vector3 end, float t)
        {
            float oneMinusT = 1f - t;
            return oneMinusT * oneMinusT * start
                + 2f * oneMinusT * t * control
                + t * t * end;
        }
    }
}

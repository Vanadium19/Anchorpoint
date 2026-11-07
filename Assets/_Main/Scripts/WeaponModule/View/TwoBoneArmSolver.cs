using UnityEngine;

namespace WeaponModule
{
    public static class TwoBoneArmSolver
    {
        private const float MinLength = 1e-5f;
        private const float ReachLimit = 0.999f;

        public static void Solve(Transform upperArm, Transform lowerArm, Transform hand, Vector3 targetPosition,
            Quaternion targetRotation, float weight)
        {
            if (weight <= 0f)
                return;

            var shoulder = upperArm.position;
            var elbow = lowerArm.position;
            var wrist = hand.position;
            var upperLength = (elbow - shoulder).magnitude;
            var lowerLength = (wrist - elbow).magnitude;
            var goal = Vector3.Lerp(wrist, targetPosition, weight);
            var toGoal = goal - shoulder;
            var goalDistance = toGoal.magnitude;

            if (upperLength < MinLength || lowerLength < MinLength || goalDistance < MinLength)
                return;

            var minReach = Mathf.Abs(upperLength - lowerLength) + MinLength;
            var reach = Mathf.Clamp(goalDistance, minReach, (upperLength + lowerLength) * ReachLimit);
            var goalDirection = toGoal / goalDistance;
            var bendDirection = GetBendDirection(shoulder, elbow, wrist, goalDirection);
            var alongGoal = (upperLength * upperLength - lowerLength * lowerLength + reach * reach) / (2f * reach);
            var bendHeight = Mathf.Sqrt(Mathf.Max(0f, upperLength * upperLength - alongGoal * alongGoal));
            var solvedElbow = shoulder + goalDirection * alongGoal + bendDirection * bendHeight;

            upperArm.rotation = Quaternion.FromToRotation(elbow - shoulder, solvedElbow - shoulder) * upperArm.rotation;
            lowerArm.rotation = Quaternion.FromToRotation(hand.position - solvedElbow, goal - solvedElbow) * lowerArm.rotation;
            hand.rotation = Quaternion.Slerp(hand.rotation, targetRotation, weight);
        }

        private static Vector3 GetBendDirection(Vector3 shoulder, Vector3 elbow, Vector3 wrist, Vector3 goalDirection)
        {
            var elbowOffset = elbow - shoulder;
            var bend = elbowOffset - goalDirection * Vector3.Dot(elbowOffset, goalDirection);

            if (bend.sqrMagnitude > MinLength * MinLength)
                return bend.normalized;

            var limbDirection = (wrist - shoulder).normalized;
            bend = elbowOffset - limbDirection * Vector3.Dot(elbowOffset, limbDirection);

            if (bend.sqrMagnitude > MinLength * MinLength)
                return bend.normalized;

            return Vector3.Cross(goalDirection, Vector3.up).normalized;
        }
    }
}

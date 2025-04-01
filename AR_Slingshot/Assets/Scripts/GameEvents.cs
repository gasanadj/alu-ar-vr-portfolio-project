using System;
using UnityEngine.XR.ARFoundation;

namespace ARSlingshotGame
{
    // New centralized event system to replace direct event connections
    public static class GameEvents
    {
        // Event for ammunition updates
        public static event Action<int> OnAmmoUpdated;
        public static void AmmoUpdated(int ammoCount) => OnAmmoUpdated?.Invoke(ammoCount);

        // Event for target/enemy destruction
        public static event Action<int, int> OnTargetDestroyed;
        public static void TargetDestroyed(int id, int points) => OnTargetDestroyed?.Invoke(id, points);

        // Event for plane selection in AR
        public static event Action<ARPlane> OnPlaneSelected;
        public static void PlaneSelected(ARPlane plane) => OnPlaneSelected?.Invoke(plane);
        
        // Event for ammunition hit
        public static event Action OnAmmoHit;
        public static void AmmoHit() => OnAmmoHit?.Invoke();
    }
}
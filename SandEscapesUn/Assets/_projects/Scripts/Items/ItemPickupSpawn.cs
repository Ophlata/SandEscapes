using UnityEngine;

namespace SandEscapes.Items
{
    /// <summary>
    /// Shared spawn logic for pickups dropped or thrown into the world.
    /// </summary>
    public static class ItemPickupSpawn
    {
        public static GameObject SpawnWorldPickup(
            ItemData item,
            int amount,
            Vector3 position,
            Quaternion rotation,
            Vector3 initialVelocity,
            PrimitiveType fallbackPrimitive,
            float fallbackScale)
        {
            GameObject instance;
            if (item != null && item.WorldPickupPrefab != null)
                instance = UnityEngine.Object.Instantiate(item.WorldPickupPrefab, position, rotation);
            else
            {
                instance = GameObject.CreatePrimitive(fallbackPrimitive);
                instance.name = item != null ? $"Dropped_{item.ItemId}" : "Dropped_Item";
                instance.transform.SetPositionAndRotation(position, rotation);
                var s = instance.transform.localScale;
                instance.transform.localScale = s * fallbackScale;
            }

            var pickup = instance.GetComponent<WorldItemPickup>();
            if (pickup == null)
                pickup = instance.AddComponent<WorldItemPickup>();
            pickup.Initialize(item, amount);
            pickup.enabled = true;

            var colliders = instance.GetComponentsInChildren<Collider>(true);
            var hasSolidCollider = false;
            for (var i = 0; i < colliders.Length; i++)
            {
                var col = colliders[i];
                if (col == null)
                    continue;
                col.enabled = true;
                col.isTrigger = false;
                hasSolidCollider = true;
            }

            if (!hasSolidCollider)
            {
                var fallbackCollider = instance.AddComponent<BoxCollider>();
                fallbackCollider.isTrigger = false;
            }

            var rb = instance.GetComponent<Rigidbody>();
            if (rb == null)
                rb = instance.AddComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.detectCollisions = true;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.linearVelocity = initialVelocity;
            rb.angularVelocity = Vector3.zero;

            return instance;
        }
    }
}

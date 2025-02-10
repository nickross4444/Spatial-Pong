using UnityEngine;

public class CollisionParticleSpawner : MonoBehaviour
{
    [SerializeField] GameObject wallHitEffectPrefab; 
    [SerializeField] GameObject goalHitEffectPrefab; 

    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        Vector3 hitPoint = contact.point;
        Vector3 hitNormal = contact.normal;

        if (collision.gameObject.CompareTag("Wall"))
        {
            GameObject wallEffect = Instantiate(wallHitEffectPrefab, hitPoint, Quaternion.LookRotation(hitNormal));

            Destroy(wallEffect, 0.5f); 
        }

        if (collision.gameObject.CompareTag("Goal"))
        {
            GameObject goalEffect = Instantiate(goalHitEffectPrefab, hitPoint, Quaternion.LookRotation(hitNormal));
            
            Destroy(goalEffect, 0.5f); 
        }
    }
}


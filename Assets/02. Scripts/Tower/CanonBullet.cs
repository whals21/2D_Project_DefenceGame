using UnityEngine;
using System;

public class CanonBullet : MonoBehaviour
{
    private Vector3 targetPosition;
    private float speed;
    private float lifeTime;
    private float explosionRadius;
    private Action<Vector3> onExplode;

    private float timer;

    public void Initialize(Vector3 target, float bulletSpeed, int damage, 
                         float bulletLifeTime, float radius, Action<Vector3> explodeCallback)
    {
        targetPosition = target;
        speed = bulletSpeed;
        lifeTime = bulletLifeTime;
        explosionRadius = radius;
        onExplode = explodeCallback;
        timer = 0f;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        
        // 수명이 다하면 폭발
        if (timer >= lifeTime)
        {
            Explode();
            return;
        }

        // 목표 지점을 향해 이동
        transform.position = Vector3.MoveTowards(
            transform.position, 
            targetPosition, 
            speed * Time.deltaTime
        );

        // 목표 지점에 도달하면 폭발
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            Explode();
        }
    }

    private void Explode()
    {
        onExplode?.Invoke(transform.position);
        Destroy(gameObject);
    }
}

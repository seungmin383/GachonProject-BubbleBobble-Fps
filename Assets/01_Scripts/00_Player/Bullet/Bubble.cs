using UnityEngine;

public class Bubble : MonoBehaviour
{
    [SerializeField]
    private float _speed = 10.0f;
    [SerializeField]
    private float _lifeTime = 5.0f;

    private float _elapsedTime;

    void Update()
    {
        Move();
        UpdateLifeTime();
    }

    private void Move()
    {
        transform.position += transform.forward * _speed * Time.deltaTime;
    }

    private void UpdateLifeTime()
    {
        _elapsedTime += Time.deltaTime;

        if(_elapsedTime > _lifeTime )
        {
            _elapsedTime = 0;
            Destroy(gameObject);
        }
    }
}

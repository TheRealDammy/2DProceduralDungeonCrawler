using UnityEngine;
using System;

public class MovingObstacle : MonoBehaviour
{
    [SerializeField] private Transform m_startWaypoint;
    [SerializeField] private Transform m_endWaypoint;
    [SerializeField] private float m_speed = 5;

    private Transform m_target;
    private Rigidbody2D m_rigidbody;

    private void Start()
    {
        m_target = m_startWaypoint;
    }

    private void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, m_target.position, m_speed * Time.deltaTime);
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("MovingObstacleWayPoint"))
        {
            ChangeTarget();
        }

        void ChangeTarget()
        {
            if (m_target == m_startWaypoint)
            {
                m_target = m_endWaypoint;
            }
            else
            {
                m_target = m_startWaypoint;
            }
        }
    }
}

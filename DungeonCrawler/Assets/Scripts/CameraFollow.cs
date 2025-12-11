using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float followSpeed = 5f;


    private void FixedUpdate()
    {
        if(target == null)
        {
            return;
        }

        Vector3 newPos = target.position;
        newPos.z = -10f;
        transform.position = Vector3.Lerp(transform.position, newPos, followSpeed * Time.deltaTime);
    }
}

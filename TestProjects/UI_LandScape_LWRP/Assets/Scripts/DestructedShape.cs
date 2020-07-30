using System;
using UnityEngine;

public class DestructedShape : MonoBehaviour
{
    bool explodedState = false;
    DateTime timer = new DateTime();

    void Awake()
    {
    }

    private void Update()
    {
        if (explodedState == true)
        {
            TimeSpan dur = DateTime.Now.Subtract(timer);
            if(dur.TotalSeconds > 20f)
            {
                explodedState = false;
                this.gameObject.SetActive(false);
            }
        }

    }

    public void Explode(Vector3 normalizedDirection)
    {

        foreach ( Transform child in this.transform)
        {

            Vector3 forceDir = normalizedDirection;
            forceDir.y = forceDir.y + UnityEngine.Random.Range(0f, 2f);
            forceDir.x = forceDir.x + UnityEngine.Random.Range(-0.5f, 0.5f);
            forceDir.z = forceDir.z + UnityEngine.Random.Range(-0.5f, 0.5f);

            forceDir.Normalize();

            float forceVal = UnityEngine.Random.Range(400f, 600f);
            child.GetComponent<Rigidbody>().AddForce(forceVal * forceDir);
        }

        explodedState = true;
        timer = DateTime.Now;
    }

    public void SetMaterial(Material material)
    {
        foreach (Transform child in this.transform)
        {
            child.GetComponent<MeshRenderer>().material = material;
        }
    }

    public void Reclaim()
    {
        foreach (Transform child in this.transform)
        {
            child.GetComponent<Transform>().localPosition = new Vector3(0, 0, 0);
            child.GetComponent<Transform>().localRotation = new Quaternion(0, 0, 0, 0);
        }
    }
}

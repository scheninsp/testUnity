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

    public void Explode()
    {
        float explosionForce = UnityEngine.Random.Range(50, 100);
        Vector3 explosionPosition = this.transform.position;
        float explosionRadius = 1f;

        foreach ( Transform child in this.transform)
        {
            child.GetComponent<Rigidbody>().AddExplosionForce(explosionForce,
                explosionPosition, explosionRadius);
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

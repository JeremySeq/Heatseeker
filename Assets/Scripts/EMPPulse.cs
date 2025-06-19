using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class EMPPulse : MonoBehaviour
{
    public float maxRadius = 20f;
    public float duration = 1f;
    public int segments = 15;
    public Color pulseColor = Color.cyan;
    public bool disableMissiles = true;

    private float timer = 0f;
    private LineRenderer lr;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.loop = true;
        lr.positionCount = segments + 1;
        lr.material = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
        lr.startColor = pulseColor;
        lr.endColor = pulseColor;
        lr.useWorldSpace = false;

        UpdateRing(0.01f);
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / duration;
        float radius = Mathf.Lerp(0.1f, maxRadius, t);
        UpdateRing(radius);

        float width = Mathf.Lerp(0.5f, 0.05f, t); // taper fade (width)
        lr.startWidth = width;
        lr.endWidth = width;
        
        Color c = pulseColor;
        c.a = Mathf.Lerp(1f, 0f, t);
        lr.startColor = c;
        lr.endColor = c;


        if (timer >= duration)
            Destroy(gameObject);


        if (disableMissiles)
        {
            DisableMissiles(radius);
        }
        

    }

    private void DisableMissiles(float radius)
    {
        // detect missiles within current radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Missile"))
            {
                MissileController missile = hit.GetComponent<MissileController>();
                if (missile != null)
                {
                    missile.DisableTrackingTemporarily(1);
                }
                else
                {
                    Destroy(hit.gameObject);
                }
            }
        }
    }

    void UpdateRing(float radius)
    {
        for (int i = 0; i <= segments; i++)
        {
            float angle = 2 * Mathf.PI * i / segments;
            Vector3 pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            lr.SetPosition(i, pos);
        }
    }
    
    void OnDrawGizmos()
    {
        float t = timer / duration;
        float radius = Mathf.Lerp(0.1f, maxRadius, t);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

}
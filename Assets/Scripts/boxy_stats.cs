using UnityEngine;

public class boxy_stats : MonoBehaviour
{
    public int health;
    Rigidbody2D boxy;
    int damage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = 50;
        damage = 50;
        boxy = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        boxy.linearVelocityX -= 0.5f * Time.deltaTime;
        if(health <= 0)
            {
                this.gameObject.SetActive(false);
                Debug.Log("Boxy is dead");
            }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Dummy"))
        {
            Debug.Log("You Hit Dummy");
        }
        else if(collision.CompareTag("Projectile"))
        {
            if(health > 0)
            {
                health -= damage;
            }
        }
    }
}

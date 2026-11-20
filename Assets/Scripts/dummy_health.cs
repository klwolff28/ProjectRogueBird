using UnityEngine;

public class dummy_health : MonoBehaviour
{
    public int health;
    int damage_taken;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = 25;
        damage_taken = 25;
    }

    // Update is called once per frame
    void Update()
    {
        if(health <= 0)
            {
                this.gameObject.SetActive(false); 
                Debug.Log("Dummy is dead");
            }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Enemy"))
        {
            if(health > 0)
            {
                health -= damage_taken;
            }
          
        }
    }
}

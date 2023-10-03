using System.Collections;
using System.Collections.Generic;
using Scenes;
using UnityEngine;
using UnityEngine.Serialization;

public class Enemy : MonoBehaviour
{
    //------ Enemy properties -------
    public int health = 100; //Vida del enemigo
    public float speed = 2f; //Velocidad base del enemigo
    public int damage = 1; //Daño que hace el enemigo a la base del jugador
    public int cost = 11; //Costo de invocación del enemigo (USO INTERNO)

    public string WalkingDirection = "UP";
    public int gold = 10; //Oro que deja al morir
    
    public bool isWalking = false; // Cambia "Walk" a "isWalking"
    
    private GridManager manager;
    private LinkedList<Node> enemyPath;
    private LinkedListNode<Node> next;
    private GameObject nextGO;

    private Animator anim;

    private bool walk = true, up = true, down = true;
    
    public void death()
    {
        this.gameObject.transform.parent.GetComponent<EnemySummoner>().enemyDied();
        GameObject.FindGameObjectWithTag("Game").gameObject.GetComponent<Game>().RecieveMoney(gold);
        Destroy(this.gameObject);
    }
    
    private void Start()
    {
        manager = GameObject.FindGameObjectWithTag("GridManager").GetComponent<GridManager>();
        enemyPath = manager.GetPath();
        next = enemyPath.First;
        if (next != null)
            nextGO = next.Value.GetValue();
        
        // Genera un offset aleatorio
        offSetX = Random.Range(-0.25f, 0.25f);
        offSetY = Random.Range(-0.25f, 0.25f);

        anim = gameObject.GetComponent<Animator>();
    }
    
    private float deltaX;
    private float deltaY;
    
    private float offSetX;
    private float offSetY;
    
    private void Update()
    {
        if(health <= 0 )
            death();
        
        if (isWalking && next != null)
        {
            // Obtiene la posición objetivo con un desplazamiento aleatorio
            Vector3 targetPosition = nextGO.transform.position + new Vector3(offSetX, offSetY, 0);
            
            Vector3 currentPosition = transform.position;
    
            deltaX = targetPosition.x - currentPosition.x;
            deltaY = targetPosition.y - currentPosition.y;
    
            if (Mathf.Abs(deltaX) < speed * Time.deltaTime && Mathf.Abs(deltaY) < speed * Time.deltaTime)
            {
                transform.position = new Vector2(targetPosition.x, targetPosition.y);
                next = next.Next;
                if (next != null)
                {
                    nextGO = next.Value.GetValue();
                }
                else
                {
                    death();
                }
            }
            else
            {
                Vector3 moveDirection = new Vector3(Mathf.Sign(deltaX), Mathf.Sign(deltaY), 0);
                if (Mathf.Abs(deltaY) >= speed * Time.deltaTime)
                {
                    moveDirection.x = 0;
                }
                else
                {
                    moveDirection.y = 0;
                }
                MoveAdd((moveDirection.x * speed * Time.deltaTime), (moveDirection.y * speed * Time.deltaTime));
            }
        }
    }
    
    private void MoveAdd(float x, float y)
    {
        Vector3 pos = transform.position;
        transform.position = pos + new Vector3(x, y, 0);
        
        if (x > 0)
        {
            WalkingDirection = "RIGHT";
            if (walk)
            {
                walk = false;
                up = true;
                down = true;
                anim.SetTrigger("Walk");
            }
            
        }
        else if (x < 0)
        {
            WalkingDirection = "LEFT";
            if (walk)
            {
                walk = false;
                up = true;
                down = true;
                anim.SetTrigger("Walk");
            }
        }
        else if (y > 0)
        {
            WalkingDirection = "UP";
            if (up)
            {
                walk = true;
                up = false;
                down = true;
                anim.SetTrigger("Up");
            }
        }
        else if (y < 0)
        {
            WalkingDirection = "DOWN";
            if (down)
            {
                walk = true;
                up = true;
                down = false;
                anim.SetTrigger("Down");
            }
        }
    }

    public void ReceiveDamage(int damage)
    {
        health -= damage;
    }
}

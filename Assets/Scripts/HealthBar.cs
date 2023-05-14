using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Transform healthBar;
    private float maxHealth = 100;
    private float health = 100; // default for now
    private float adjustedHealthWidth;
    private float timeToUpdate = .4f;
    private Enemy enemyFollow;
    private Camera cam;
    private Image healthBarImage;

    // this is for 
    [SerializeField] private bool isWorldUI = true;

    // Start is called before the first frame update
    void Start()
    {
        cam = MainCam.Instance.GetComponent<Camera>();
        healthBarImage = healthBar.gameObject.GetComponent<Image>();
    }

    public void Init(Enemy enemy, float enemyHealth)
    {
        enemyFollow = enemy;
        maxHealth = enemyHealth;
        health = maxHealth;
        adjustedHealthWidth = healthBar.localScale.x;
        transform.SetParent(HealthBarUI.Instance.transform);
        timeToUpdate = .1f;
    }

    // Update is called once per frame
    void Update()
    {
        // update the position of the healthbar
        // this will only affect moving objects that call Init()
        // wait until the init is called before we show the bar

        if (enemyFollow != null)
        {
            var enemyPos = enemyFollow.transform.position;
            var newPos = new Vector2(enemyPos.x, enemyPos.y + .5f);

            transform.position = cam.WorldToScreenPoint(newPos);
        }
    }

    public void SetMaxHealth(float amount)
    {
        maxHealth = amount;
    }

    public void AddHealth(float amount)
    {
        health += amount;

        if (health > maxHealth)
            health = maxHealth;
        if (healthBar.gameObject.activeSelf)
            UpdateBar();
    }

    public void RemoveHealth(float amount)
    {
        health -= amount;

        if (health < 0)
            health = 0;
        if (healthBar.gameObject.activeSelf)
            UpdateBar();
    }

    public void SetHealth(float amount)
    {
        health = amount;

        // do our checks for both
        if (health < 0)
            health = 0;

        if (health > maxHealth)
            health = maxHealth;
        if (healthBar.gameObject.activeSelf)
            UpdateBar();
    }

    private void UpdateBar()
    {
        if (gameObject.activeInHierarchy)
            StartCoroutine(LerpBar());   
    }

    private IEnumerator LerpBar()
    {
        float elapsedTime = 0;


        Vector2 origHealth = healthBar.localScale;
        if (origHealth.x == 0 || origHealth.y == 0)
            origHealth = new Vector2(.01f, .01f);


        Vector2 targetHealth = new Vector2(health / maxHealth, 1);

        if (float.IsNaN(targetHealth.x))
            targetHealth = new Vector2(.01f, 1);

        var percent = health / maxHealth;

        // NOTE: Can add check to get player to move on walls only
        while (elapsedTime < timeToUpdate)
        {
            healthBar.localScale = Vector2.Lerp(origHealth, targetHealth, (elapsedTime / timeToUpdate));

            if (percent < .2f)
                healthBarImage.color = Color.Lerp(healthBarImage.color, Color.red, (elapsedTime / timeToUpdate));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        healthBar.localScale = targetHealth;
    }
}

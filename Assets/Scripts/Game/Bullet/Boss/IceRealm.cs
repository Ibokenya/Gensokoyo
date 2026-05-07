using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceRealm : MonoBehaviour
{
    public GameObject player;
    private SpriteRenderer spriteRenderer;
    private new Collider2D collider2D;
    
    private bool isFadingIn = false;
    private bool isFadingOut = false;
    private bool isColliderActive = false;
    private bool isWaitingForActivation = false;
    
    private const float fadeInDuration = 1f;
    private const float fadeOutDuration = 1f;
    private const float checkRadius = 1.344f;
    private readonly Vector3 centerPosition = new Vector3(-3f, 0f, 0f);
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider2D = GetComponent<Collider2D>();
        
        // 初始化状态
        if (collider2D != null)
        {
            collider2D.enabled = false;
        }
        
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 0f;
            spriteRenderer.color = color;
        }
        
        isWaitingForActivation = true;
        isFadingIn = false;
        isFadingOut = false;
        isColliderActive = false;
    }

    private void Update()
    {
        // 确保player引用不为空
        if (player == null)
        {
            return;
        }
        
        if (isWaitingForActivation)
        {
            return;
        }
        
        if (isFadingOut || isFadingIn)
        {
            return;
        }
        
        if (isColliderActive)
        {
            if (!IsPlayerInIceRealm())
            {
                StartFadeOut();
            }
        }
    }
    
    private bool IsPlayerInIceRealm()
    {
        if (player == null)
        {
            return false;
        }
        
        float distance = Vector3.Distance(player.transform.position, centerPosition);
        return distance <= checkRadius;
    }
    
    public void Activate()
    {
        // 如果正在淡出、淡入或碰撞器已激活，则不执行激活
        if (isFadingOut || isFadingIn || isColliderActive)
        {
            return;
        }
        
        isWaitingForActivation = false;
        isFadingOut = false;
        isColliderActive = false;
        
        if (collider2D != null)
        {
            collider2D.enabled = false;
        }
        
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 0f;
            spriteRenderer.color = color;
        }
        
        StartFadeIn();
    }
    
    private void StartFadeIn()
    {
        isFadingIn = true;
        StartCoroutine(FadeInCoroutine());
    }
    
    private IEnumerator FadeInCoroutine()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
            
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = alpha;
                spriteRenderer.color = color;
            }
            
            yield return null;
        }
        
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }
        
        isFadingIn = false;
        
        if (collider2D != null)
        {
            collider2D.enabled = true;
            isColliderActive = true;
        }
        Debug.Log("冰囚笼淡入");
    }
    
    public void StartFadeOut()
    {
        if (isFadingOut)
        {
            return;
        }
        
        isFadingOut = true;
        isColliderActive = false;
        
        if (collider2D != null)
        {
            collider2D.enabled = false;
        }
        
        StartCoroutine(FadeOutCoroutine());
    }
    
    private IEnumerator FadeOutCoroutine()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
            
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = alpha;
                spriteRenderer.color = color;
            }
            
            yield return null;
        }
        
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 0f;
            spriteRenderer.color = color;
        }
        
        isFadingOut = false;
        isWaitingForActivation = true;
        isColliderActive = false;
        if (collider2D != null)
        {
            collider2D.enabled = false;
        }
        Debug.Log("冰囚笼淡出");
    }
}
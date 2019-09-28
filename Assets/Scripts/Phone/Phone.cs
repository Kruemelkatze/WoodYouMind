﻿using System;
using System.Collections;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Phone : MonoBehaviour
{
    public SpriteRenderer coloredSprite;
    public SpriteRenderer handBack;
    public SpriteRenderer handFront;
    public SpriteRenderer indicator;
    public Animation flash;

    public Vector3 targetPosition;
    private Vector3 velocity = Vector3.zero;
    public const float smoothTime = 0.3F;
    private const float StartOffset = 6;

    public float timeToPhoto = 5;
    public float timeToPhotoLeft = 5;
    public bool photoShot = false;
    
    
    private Screen _screen;

    public bool isHorizontal;

    public float blinkTimer;
    public float flashTime = 0.2f;
    
    void Start()
    {
        _screen = GetComponentInChildren<Screen>();
        
        var camBounds = GetCameraBounds();
        // Get nearest edge of targetPosition
        var diffTop = Math.Abs(camBounds.y + camBounds.height - targetPosition.y);
        var diffBottom = Math.Abs(targetPosition.y - camBounds.y);
        var diffToLeft = Math.Abs(targetPosition.x - camBounds.x);
        var diffToRight = Math.Abs(camBounds.x + camBounds.width - targetPosition.x);

        var min = Math.Min(Math.Min(diffTop, diffBottom), Math.Min(diffToLeft, diffToRight)); // Thanks, Unity
        var tolerance = 0.05f;
        if (Math.Abs(min - diffTop) < tolerance)
        {
            // Top
            transform.Rotate(Vector3.forward, 180);
            _screen.transform.Rotate(Vector3.forward, -180);
        }
        else if (Math.Abs(min - diffBottom) < tolerance)
        {
            // Bottom
            // Everything is fine
        }
        else if (Math.Abs(min - diffToLeft) < tolerance)
        {
            // Left
            transform.Rotate(Vector3.forward, -90);
            _screen.transform.Rotate(Vector3.forward, 90);
            // Flip screen size
            _screen.transform.localScale = FlipV3(_screen.transform.localScale);
            isHorizontal = true;
        }
        else
        {
            // Right
            transform.Rotate(Vector3.forward, 90);
            _screen.transform.Rotate(Vector3.forward, -90);
            // Flip screen size
            _screen.transform.localScale = FlipV3(_screen.transform.localScale);
            isHorizontal = true;
        }
        
        transform.Translate(Vector3.down * StartOffset);
        
        timeToPhotoLeft = timeToPhoto;
        StartCoroutine(ToggleIndicator());
    }

    private Vector3 FlipV3(Vector3 v)
    {
        return new Vector3(v.y, v.x, v.z);
    }


    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        timeToPhotoLeft = Math.Max(0, timeToPhotoLeft - Time.deltaTime);

        if (!photoShot && Math.Abs(timeToPhotoLeft) < 0.05)
        {
            ShootPhoto();
        }
    }

    IEnumerator ToggleIndicator()
    {
        while (!photoShot)
        {
            var color = indicator.color;
            
            color.a = 255;
            indicator.color = color;
            yield return new WaitForSeconds(blinkTimer);


            var waitTime = timeToPhotoLeft / timeToPhoto;
            if (waitTime < 0.15)
            {
                waitTime = timeToPhotoLeft;
                color.a = 255;
            }
            else
            {
                color.a = 0;
            }
            
            indicator.color = color;
            
            Debug.Log(waitTime);
            yield return new WaitForSeconds(waitTime);

            if (photoShot)
            {
                color.a = 0;
                indicator.color = color;
            }
        }
        
        Debug.Log("Stopped toggling indicator");
    }

    public void ShootPhoto()
    {
        Debug.Log("Shot Photo");
        photoShot = true;

        flash.Play();
    }

    Rect GetCameraBounds()
    {
        var camHeight = 2 * Camera.main.orthographicSize;
        var camWidth = camHeight * Camera.main.aspect;

        var camPos = Camera.main.transform.position;
        return new Rect(
            camPos.x - camWidth / 2,
            camPos.y - camHeight / 2,
            camPos.x + camWidth,
            camPos.y + camHeight
        );
    }

    public void SetColor(Color color)
    {
        if (coloredSprite != null)
        {
            coloredSprite.color = color;
        }
    }

    public void Despawn()
    {
        targetPosition = targetPosition - (transform.rotation * Vector3.up * 1.5f *StartOffset);
        Destroy(gameObject, smoothTime + 0.01f);
    }

    public void ResetPhoto()
    {
        timeToPhotoLeft = timeToPhoto;
        photoShot = false;
        StartCoroutine(ToggleIndicator());
    }
}
using System;
using System.Collections;
using UnityEngine;

public class WindExternalVelocityTrigger : MonoBehaviour
{
    private WindVelocityController _windVelocityController;

    private GameObject _player;

    private Material _material;

    private Rigidbody2D _playerRB;

    private bool _easeInCoroutineRunning;
    private bool _easeOutCoroutineRunning;

    private int _externalInfluence = Shader.PropertyToID("_ExternalInfluence");

    private float _startingXVelocity;
    private float _velocityLastFrame;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _playerRB = _player.GetComponent<Rigidbody2D>();
        _material = GetComponent<Renderer>().material;
        _windVelocityController = GetComponent<WindVelocityController>();

        _material = GetComponent<SpriteRenderer>().material;
        _startingXVelocity = _material.GetFloat(_externalInfluence);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if(collision.gameObject == _player)
        {
            if (!_easeInCoroutineRunning && Mathf.Abs(_playerRB.linearVelocity.x) > Mathf.Abs(_windVelocityController.VelocityThreshold))
            {
                float XVelocity = _playerRB.linearVelocity.x * _windVelocityController.ExternalInfluenceStrenght;
                StartCoroutine(EaseIn(XVelocity));
            }
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {

        if (collision.gameObject == _player)
        {
            if (Mathf.Abs(_velocityLastFrame) > Mathf.Abs(_windVelocityController.VelocityThreshold) && MathF.Abs(_playerRB.linearVelocityX) < Mathf.Abs(_windVelocityController.VelocityThreshold))
            {
                StartCoroutine(EaseOut());

            }
            else if (Mathf.Abs(_velocityLastFrame) > Mathf.Abs(_windVelocityController.VelocityThreshold) && MathF.Abs(_playerRB.linearVelocityX) < Mathf.Abs(_windVelocityController.VelocityThreshold))
            {
                float XVelocity = _playerRB.linearVelocity.x * _windVelocityController.ExternalInfluenceStrenght;
                StartCoroutine(EaseIn(XVelocity));
            }
            else if (!_easeInCoroutineRunning && !_easeOutCoroutineRunning && MathF.Abs(_playerRB.linearVelocityX) < Mathf.Abs(_windVelocityController.VelocityThreshold))
            {
                _windVelocityController.InfluenceWind(_material, _playerRB.linearVelocityX * _windVelocityController.ExternalInfluenceStrenght);
            }

            _velocityLastFrame = _playerRB.linearVelocityX;
        }

    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject == _player)
        {
            StartCoroutine(EaseOut());
        }

    }
    private IEnumerator EaseIn(float XVelocity)
    {
        _easeInCoroutineRunning = true;

        float elapsedTime = 0f;
        while(elapsedTime < _windVelocityController.EaseInTime)
        {
            elapsedTime += Time.deltaTime;
            float lerpedAmount = Mathf.Lerp(_startingXVelocity, XVelocity, elapsedTime / _windVelocityController.EaseInTime);
            _windVelocityController.InfluenceWind(_material, lerpedAmount);
            yield return null;
        }

        _easeInCoroutineRunning = false;
    }

    private IEnumerator EaseOut()
    {
        _easeOutCoroutineRunning = true;
        float currentXInfluence = _material.GetFloat(_externalInfluence);

        float elapsedTime = 0f;
        while (elapsedTime < _windVelocityController.EaseOutTime)
        {
            elapsedTime += Time.deltaTime;
            float lerpedAmount = Mathf.Lerp(currentXInfluence, _startingXVelocity, elapsedTime / _windVelocityController.EaseOutTime);
            _windVelocityController.InfluenceWind(_material, lerpedAmount);
            yield return null;
        }

        _easeOutCoroutineRunning = false;
    }

}

﻿using Crowbar.Ship;
using UnityEngine;
using System.Collections;
using Mirror;

namespace Crowbar.Enemy
{
    public class MoveShark : MoveEnemy
    {
        public float timeWaitMax = 5f;
        public float timeWaitMin = 0f;
        public float timeMoveMax = 8f;
        public float timeMoveMin = 1f;
        public float offsetAngleMax = 60f;
        public float offsetAngleMin = -60f;
        public float timeToDestroy = 20f;

        public UnderwaterShip target;
        public Shark shark;

        public bool isMove;

        public void Leave()
        {
            StopAllCoroutines();

            transform.localRotation = Quaternion.Euler(0, 0, 270f);
            isMove = true;

            StartCoroutine(WaitDestroy());
        }

        public void StartMove()
        {
            float timeMove = Random.Range(timeMoveMin, timeMoveMax);
            float timeWait = Random.Range(timeWaitMin, timeWaitMax);

            SetAngle();
            StopAllCoroutines();
            StartCoroutine(MoveTimer(timeMove));
            StartCoroutine(WaitTimer(timeMove + timeWait));
        }

        public void SetAngle()
        {
            Vector3 direction = target.transform.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            angle += Random.Range(offsetAngleMin, offsetAngleMax);

            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        public IEnumerator WaitTimer(float time)
        {
            yield return new WaitForSeconds(time);

            StartMove();
        }

        public IEnumerator WaitDestroy()
        {
            yield return new WaitForSeconds(timeToDestroy);

            NetworkServer.Destroy(gameObject);
        }

        public IEnumerator MoveTimer(float time)
        {
            isMove = true;

            yield return new WaitForSeconds(time);

            isMove = false;
        }

        private void Start()
        {
            if (isServer)
            {
                target = FindObjectOfType<UnderwaterShip>();

                StartMove();
            }
        }

        private void Update()
        {
            if (!shark.isDied && isMove && isServer)
            {
                transform.position += transform.right * speed * Time.deltaTime;
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (isServer)
            {
                Water water = collision.GetComponent<Water>();

                if (water != null && water.waterParent == null)
                    Leave();
            }
        }
    }
}
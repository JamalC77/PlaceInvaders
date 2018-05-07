﻿using DigitalRuby.LightningBolt;
using GameplayNs;
using System.Collections;
using UnityEngine;

namespace WeaponNs
{
  
    public class AnimatedGun : MonoBehaviour
    {

        public Transform Muzzle;
        [Range(0,1)]
        public float DamageAmount = 0.5f;

        public bool IsAutomaticFire = true;

        [Tooltip("Time between shots")]
        public float PauseBetweenShots = 0.5f;


        [Tooltip("Time between shots")]
        public float ShotDuration = 0.5f;

        public GameObject Blast;
        
        [Tooltip("Third party component used by this script")]
        public LightningBoltScript lightning;
        /// <summary>
        /// required by LightningBoltScript 
        /// </summary>
        LineRenderer lineRenderer;
        PhotonView _thisPhotonView;
        public PhotonView photonView
        {
            get
            {
                if (_thisPhotonView == null)
                    _thisPhotonView = GetComponent<PhotonView>();
                return _thisPhotonView;
            }
        }
        

        public void DoHitShot(RaycastHit hit, float maxDistance)
        {
            MakeDamage(hit, DamageAmount);
            AnimateShot();
            Debug.DrawLine(Muzzle.position, hit.point, Color.yellow, 0.5f);
        }

        public void DoMissedShot(Vector3 targetPosition)
        {
            AnimateShot();
            Debug.DrawLine(Muzzle.position, targetPosition, Color.blue, 0.5f);
        }

        void AnimateShot()
        {
            StartCoroutine(AnimateShotCoroutine());
        }

        void MakeDamage (RaycastHit hit, float DamageAmount)
        {
            // Debug.Log("Sending message to " + hit.transform.gameObject.name);
            if (
                    (PhotonNetwork.offlineMode || !PhotonNetwork.connected)
                    || photonView.isMine
                )
                hit.transform.SendMessage(GameController.EnemyDamageReceiverName,DamageAmount, SendMessageOptions.DontRequireReceiver);
        }

        bool IsMakingSecondaryDamageNow = false;
        IEnumerator AnimateShotCoroutine()
        {
            yield return null;
            lightning.enabled = true;
            yield return null;
            yield return new WaitForEndOfFrame();
            IsMakingSecondaryDamageNow = true;
            lineRenderer.enabled = true;
            audioSource.Play();
            yield return new WaitForSeconds(ShotDuration);
            //HideHit();

            IsMakingSecondaryDamageNow = false;
            lineRenderer.enabled = false;
            lightning.enabled = false;

        }


        // Use this for initialization
        void Start()
        {
            Init();
        }

        AudioSource audioSource;
        private void Init()
        {
            if(lightning == null)
                lightning = GetComponentInChildren<LightningBoltScript>();

            lineRenderer = lightning.GetComponent<LineRenderer>();
            lineRenderer.enabled = false;
            lightning.enabled = false;
            audioSource = GetComponent<AudioSource>();

        }

        AimingData AimData;
        public void SetAimData(AimingData newAimData)
        {
            AimData = newAimData;
        }

        /*
        void HideHit()
        {
            Blast.Stop();
        }
        */

        GameObject activeBlast;
        void ShowHit(Vector3 point)
        {
            if (activeBlast == null)
            {
                if (PhotonNetwork.offlineMode || !PhotonNetwork.connected)
                    activeBlast = Instantiate<GameObject>(Blast, GameController.WorldRootObject.transform);
                else
                    activeBlast = PhotonNetwork.Instantiate(Blast.name, point, Quaternion.identity, 0);
                activeBlast.transform.parent = GameController.WorldRootObject.transform;


            }
            else
                activeBlast.SendMessage("RestartSelfDestruction", SendMessageOptions.DontRequireReceiver);
            activeBlast.transform.SetPositionAndRotation(point, Quaternion.identity);
            /*
          Blast.transform.position = point;
          if (!Blast.gameObject.activeSelf)
                Blast.gameObject.SetActive(true);
            Blast.Play();
            */
        }

        // Update is called once per frame
        void Update()
        {
            if (IsMakingSecondaryDamageNow)
            {
                // Ray ray = new Ray(Muzzle.position, )
                if (AimData.IsHit
                    && AimData.HitOfRay.transform != null
                    && AimData.HitOfRay.transform.CompareTag(GameController.EnemyTag))
                {
                    ShowHit(AimData.HitOfRay.point);

                    MakeDamage(AimData.HitOfRay, DamageAmount * Time.deltaTime);
                }
               
            }
        }
    }
}

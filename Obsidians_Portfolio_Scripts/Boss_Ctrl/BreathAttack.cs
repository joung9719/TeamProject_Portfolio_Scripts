using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

public class BreathAttack : MonoBehaviour
{
    [SerializeField] int BreathDamge = 100;
    [SerializeField] Collider BreathCollider;
    public ParticleSystem BreathParticle;
    [SerializeField] LayerMask breathRayMask;
    [SerializeField] float breathTime = 3f;
    bool _isBreathing;
    bool _hasUseBreath;

    void Awake()
    {
       if(BreathCollider!=null)
       BreathCollider.enabled=false;
       if(BreathParticle!=null)
       BreathParticle.Stop();
    }
    public void StartBreath()
    {
      if(BreathCollider!=null)
      BreathCollider.enabled=true;
      if(BreathParticle!=null)
      BreathParticle.Play();

    }
   
    public void EndBreath()
    {
         if(BreathCollider!=null)
       BreathCollider.enabled=false;
       if(BreathParticle!=null)
       BreathParticle.Stop();
    }

    private void OnTriggerEnter(Collider other)
    {
      if(PhotonNetwork.connected&&!PhotonNetwork.isMasterClient)
      return;
      if(!other.CompareTag("Player"))
      return;
      Vector3 origin=transform.position;
      Vector3 target=other.bounds.center;
      Vector3 dir=target-origin;
      float dist=dir.magnitude;
      if(dist<=0.001f)
      return;
      dir/=dist;

      if(Physics.Raycast(origin,dir,out RaycastHit hit,dist,breathRayMask,QueryTriggerInteraction.Collide))
        {
            if(hit.collider!=other)
            return;
        }

      IDamageable player=other.GetComponentInParent<IDamageable>();
      if(player==null)
      return;

      player.ApplyDamage(BreathDamge,other.transform.position,Vector3.up);
    }
}

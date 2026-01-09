using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class camFollowingPlayer : MonoBehaviour
{
    public Transform player;   // 따라갈 플레이어
    public Vector3 offset = new Vector3(0, 2, -5); // 플레이어 뒤쪽 위로 살짝
    public float followSpeed = 5f; // 이동 속도
    public float rotationSpeed = 5f; // 회전 속도

    void Awake()
    {
        // player = GetComponent<camFollowingPlayer>().player;//플레이어의 위치를 참조로 받아옴
    }

    void Start()
    {
        // player = GameObject.FindGameObjectWithTag("CameraFollow").GetComponent<Transform>();
    }
    
    private Transform FindAlivePlayer()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("CameraFollow");

        foreach (var o in objs)
        {
            var pv = o.GetComponentInParent<PhotonView>();
            if (pv == null) continue;

            // 내 캐릭터가 살아있으면 위치로.
            if (pv.isMine)
            {
                var cp = pv.owner.CustomProperties;
                if (cp == null || !cp.ContainsKey("DEAD") || (bool)cp["DEAD"] == false)
                    return o.transform;
            }
        }

        // 내 캐릭터가 죽으면: 다른 생존 플레이어 찾기
        foreach (var o in objs)
        {
            var pv = o.GetComponentInParent<PhotonView>();
            if (pv == null) continue;

            var cp = pv.owner.CustomProperties;
            if (cp != null && cp.ContainsKey("DEAD") && (bool)cp["DEAD"] == true)
                continue;

            return o.transform; // 생존자 1명
        }

        return null;
    }

    private void FixedUpdate()
    {
        if (player == null) player = FindAlivePlayer();

        if (player == null) return;

        // 1. 플레이어 방향 기준으로 오프셋 적용
        Vector3 desiredPosition = player.position + player.TransformDirection(offset);

        // 2. 부드럽게 이동
        transform.position = Vector3.Slerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // 3. 플레이어 바라보기
        Quaternion desiredRotation = Quaternion.LookRotation(player.position - transform.position);

        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
    }
   
}

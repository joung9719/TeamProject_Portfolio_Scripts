using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scCubeRotation : MonoBehaviour
{
    public float Cube_rotation_Speed = 50f;//큐브 돌아가는 속도

    public float Cube_Rotation = 90f;//큐브 돌아가는 각도
    private Quaternion StartRotation;//큐브 시작 위치
    private Quaternion EndRotation;//큐브 멈춤 위치

    public scGateOpen gate;
    public int gateIndex = 0;

    private bool isCube_rotation;//큐브가 돌았는지 안돌았는지 확인
    void Awake()
    {
        StartRotation = transform.rotation;
        EndRotation = transform.rotation;
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (isCube_rotation)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, EndRotation, Cube_rotation_Speed * Time.deltaTime);

           
        }
        if (Quaternion.Angle(transform.rotation, EndRotation) < 0.1f)
        {
            transform.rotation = EndRotation;
            isCube_rotation = false;
        }
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (!coll.CompareTag("Player"))
            return;

        // 이미 돌고 있으면 무시
        if (isCube_rotation)
            return;

        if (!PhotonNetwork.connected || !PhotonNetwork.inRoom)
        {
            LocalRotateAndOpen();
            return;
        }

        // 퍼즐을 누가 풀었는지는 중요하지 않고,
        // 어떤 클라에서든 충돌이 감지되면 마스터가 한 번만 RPC 쏘는 구조
        if (!PhotonNetwork.isMasterClient)
            return;

        LocalRotateAndOpen();
    }

    // ★ 로컬 전용 처리 (오프라인/테스트용)
    private void LocalRotateAndOpen()
    {
        if (isCube_rotation)
            return;

        StartRotation = transform.rotation;
        EndRotation = StartRotation * Quaternion.Euler(0, 0, Cube_Rotation);
        isCube_rotation = true;

        if (gate != null)
        {
            gate.SetTrigger(gateIndex, true);
        }
    }
}

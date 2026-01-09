using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro 사용

public class scBattleHint : MonoBehaviour
{
    [Header("키 설정")]
    public KeyCode HintKey = KeyCode.F;
    
    [Header("힌트 설정")]
    [TextArea(3, 5)]
    public string hintMessage = "여기에 힌트 내용을 입력하세요!";
    public float displayDuration = 3f; // 힌트가 표시되는 시간 (초)
    
    [Header("UI 설정")]
    public GameObject hintUI; // 힌트 UI 오브젝트
    public TextMeshProUGUI hintText; // 힌트 텍스트 컴포넌트
    public Vector3 hintOffset = new Vector3(0, 2f, 0); // 오브젝트로부터 떨어진 거리

    [Header("플래이어 범위 설정")]
    public string playerTag="Player";
    bool _isPlayerIn=false;
    
    [Header("페이드 효과 설정 (선택)")]
    public bool useFadeEffect = true; // 페이드 효과 사용 여부
    public float fadeSpeed = 2f; // 페이드 속도
    
    private Camera mainCamera;
    private bool isHintActive = false; // 힌트가 현재 활성화되어 있는지
    private CanvasGroup canvasGroup; // 페이드 효과용
    private Coroutine hideCoroutine; // 코루틴 참조 저장

    void Start()
    {
        // 메인 카메라 찾기
        mainCamera = Camera.main;
        
        // CanvasGroup 컴포넌트 가져오기 (페이드 효과용)
        if (hintUI != null && useFadeEffect)
        {
            canvasGroup = hintUI.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                // 없으면 자동으로 추가
                canvasGroup = hintUI.AddComponent<CanvasGroup>();
            }
        }
        
        // 시작할 때 힌트 숨기기
        if (hintUI != null)
        {
            hintUI.SetActive(false);
        }
        else
        {
            Debug.LogWarning("힌트 UI가 연결되지 않았습니다! Inspector에서 연결해주세요.");
        }
    }

    void Update()
    {
        // F키를 눌렀을 때
        if (_isPlayerIn &&Input.GetKeyDown(HintKey))
        {
            // 힌트가 이미 표시 중이 아닐 때만 실행
            if (!isHintActive)
            {
                ShowHint();
            }
        }
        
        // 힌트가 활성화되어 있으면 카메라를 향하도록 회전
        if (isHintActive && hintUI != null&&mainCamera!=null)
        {
            RotateTowardsCamera();
        }
    }

    // 힌트 표시 함수
    void ShowHint()
    {
        if (hintUI == null || hintText == null)
        {
            Debug.LogError("힌트 UI 또는 텍스트가 설정되지 않았습니다!");
            return;
        }
        
        // 힌트 활성화
        isHintActive = true;
        hintUI.SetActive(true);
        
        // 텍스트 내용 설정
        hintText.text = hintMessage;
        
        // 힌트 위치 설정 (오브젝트 위치 + 오프셋)
        hintUI.transform.position = transform.position + hintOffset;
        
        // 페이드 효과 사용 시
        if (useFadeEffect && canvasGroup != null)
        {
            // 이전 코루틴이 실행 중이면 중지
            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
            }
            
            // 페이드 인 후 일정 시간 뒤 페이드 아웃
            hideCoroutine = StartCoroutine(FadeInAndHide());
        }
        else
        {
            // 페이드 효과 없이 일정 시간 후 숨기기
            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
            }
            hideCoroutine = StartCoroutine(HideAfterDelay());
        }
    }

    // 일정 시간 후 힌트 숨기기 (페이드 없음)
    IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        HideHint();
    }

    // 페이드 인 후 일정 시간 대기하고 페이드 아웃
    IEnumerator FadeInAndHide()
    {
        // 페이드 인
        canvasGroup.alpha = 0f;
        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }
        canvasGroup.alpha = 1f;
        
        // 일정 시간 대기
        yield return new WaitForSeconds(displayDuration);
        
        // 페이드 아웃
        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
        canvasGroup.alpha = 0f;
        
        HideHint();
    }

    // 힌트 숨기기 함수
    void HideHint()
    {
        if (hintUI != null)
        {
            hintUI.SetActive(false);
        }
        isHintActive = false;
    }

    // 힌트가 항상 카메라를 향하도록 회전
    void RotateTowardsCamera()
    {
        if (mainCamera != null)
        {
            // 카메라 방향을 바라보도록 설정
            hintUI.transform.rotation = Quaternion.LookRotation(
                hintUI.transform.position - mainCamera.transform.position
            );
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            _isPlayerIn=true;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            _isPlayerIn=false;
            HideHint();
        }
    }

    // Scene 뷰에서 힌트 위치 표시 (디버깅용)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + hintOffset, 0.1f);
        
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + hintOffset);
    }
}
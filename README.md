# TeamProject_Portfolio_Scripts

Unity TPS 팀 프로젝트 **Obidians**에서 제가 담당한 기능/코드를 정리한 레포입니다

## 담당 기능

**전체 스테이지 구조**
-Tutorial->FieldBattle(Wave1,Wave2)->Puzzle->Boss흐름 형식

**플레이어 컨트롤**
-이동/조작,Raycast기반 사격(권총/라이플),피격 처리

**적FSM**
-일반 몹/중간보스/최종 보스
-전환 기준:거리/시야/타이머

**보스 2페이지**
-비행->착지->브레스 특수 패턴
-전환 꼬임 문제를 우선순위/플레그/쿨타임으로 안정화

## 폴더 구조
-"Boss_Ctrl":보스 페이즈/패턴
-"Enemy_Ctrl":적 FSM
-"Player_Ctrl":플레이어 이동/사격/피격/카메라
-"Puzzle":퍼즐(힌트,게이트 오픈)

## 주요코드

**Player**
-"PlayerCtrl":이동/조작핵심
-"PlayetShot":무기 발사 로직
-"camFollowingPlayer":카메라 추적

**Enemy/Boss**
-"EnemyCtrl":적/보스FSM 제어
-"EnemyState":상태 기반 구조
-"FlyStart","FlyEnd","BreathStart"등(Boss 패턴)

**Puzzle**
-"BattleHint":오벨리스크 힌트
-"CubeRotation":마방진 큐브 회전
-"GataOpen":정답 시 게이트 오픈

## 사용 기술
-Unity,C#
-Photon(팀 프로젝트)

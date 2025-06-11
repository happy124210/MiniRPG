# 🎮 Unity RPG Game Project
모바일 세로형 RPG 게임

## 📖 프로젝트 개요
Unity 엔진을 사용하여 개발한 모바일 세로형 RPG 게임입니다. 플레이어는 자동으로 전진하며 적과 전투하고, 아이템을 수집하여 캐릭터를 강화시키는 게임입니다.

## 🎯 주요 특징

자동 전투 시스템: 플레이어가 직접 조작하지 않아도 AI가 자동으로 이동하고 전투
아이템 & 장비 시스템: 다양한 무기와 방어구로 캐릭터 능력치 강화
스테이지 기반 진행: 각 스테이지마다 다른 적과 보상

## 🛠️ 기술 스택

Engine: Unity 2022.3 LTS
Language: C#
Architecture: FSM(State Machine), ScriptableObject 기반 데이터 관리
UI Framework: UGUI
Target Platform: Mobile


## 🏗️ 시스템 아키텍처
### 📊 ScriptableObject 데이터 관리
게임의 모든 데이터를 ScriptableObject로 관리하여 코드와 데이터를 분리했습니다.

```
// 적 데이터
[CreateAssetMenu(fileName = "NewStageData", menuName = "Stage/StageData")]
public class StageData : ScriptableObject
{
    [Header("맵 정보")]
    public string stageName;
    public int stageID;
    public GameObject stagePrefab;
    
    [Header("스폰 몬스터 정보")]
    public EnemyData[] enemyPool; // 스폰할 적 데이터들
    public int requiredKillCount; // 보스 소환까지 필요한 처치 수
    
    [Header("보상")]
    public ItemData rewardItem; 
    public int rewardExp;
    public int rewardGold;
    public int rewardGem;
}
```

주요 데이터 클래스:

EnemyData: 적 스탯 및 행동 패턴
ItemData: 아이템 정보 및 스탯 모디파이어
StageData: 스테이지 정보 및 스폰 설정
StatProfile: 캐릭터 기본 능력치

### 🤖 FSM 기반 플레이어 AI
플레이어의 행동을 상태 기계로 관리하여 명확한 행동 패턴을 구현했습니다.

```
public interface IState
{
    void Enter();
    void Update();
    void Exit();
}
```

상태 종류:

PlayerIdleState: 대기 상태
PlayerMoveState: 이동 상태 (적 탐지)
PlayerAttackState: 전투 상태 (코루틴 기반 연속 공격)
PlayerDeadState: 사망 상태

### 🎒 인벤토리 & 장비 시스템
이벤트 기반 인벤토리 시스템으로 아이템 관리와 UI 업데이트를 자동화했습니다.

```
// 스탯 모디파이어 시스템
[System.Serializable]
public class StatModifier
{
    public StatType type;
    public int value;
}
```

주요 기능:

동적 스탯 계산 (기본값 + 장비 보너스)
같은 타입 장비 자동 교체
실시간 UI 업데이트

### 🏟️ 스테이지 & 스폰 시스템
플레이어 중심의 동적 적 스폰 시스템을 적용했습니다

```
// 플레이어 앞쪽에 적 스폰
Vector3 spawnCenter = playerPos + playerForward * spawnDistanceAhead;
Vector3 candidatePos = spawnCenter + rightVector * randomOffset;
```

특징:

플레이어 진행 방향 기준 스폰
장애물 감지 및 회피
동시 최대 적 수 제한으로 성능 관리
스테이지별 완료 조건 (적 처치 수 기반)


### 🎭 UI 매니저 시스템
계층화된 UI 관리로 복잡한 화면 전환을 체계적으로 처리합니다.

```
UI 계층 구조:
├── Screen (전체 화면): Main, Start, Option
├── Popup (화면 위 팝업): Inventory, Shop
└── Modal (최상위): Confirm, Loading
```

특징:

ESC 키 우선순위 처리 (Modal → Popup → Screen)
IGUI 인터페이스로 일관된 생명주기 관리
Scene별 조건부 UI 활성화


### 🎯 적 AI
플레이어를 추적하고 공격하는 기본적인 AI를 구현했습니다.

행동 패턴:

플레이어 추적 (NavMesh 없이 직선 이동)
공격 범위 내 진입 시 공격
쿨다운 기반 공격 주기


## 📁 프로젝트 구조

```
📦 Assets/
├── 📂 Scripts/
│   ├── 📂 Entities/      
│   │   ├── 📂 Enemy/      # 적 관련 스크립트
│   │   ├── 📂 Player/     # 플레이어 관련 스크립트
│   │   ├── 📂 Stat/       # 적 관련 스크립트
│   ├── 📂 UIs/            # UI 관련 스크립트
│   ├── 📂 Managers/       # 게임 매니저들
│   ├── 📂 Data/           # ScriptableObject 데이터 클래스들
│   └── 📂 Utils/          # 유틸리티 스크립트들
├── 📂 Resources/
│   ├── 📂 Items/          # 아이템 데이터들
│   └── 📂 Stages/         # 스테이지 데이터들
└── 📂 Scenes/
    ├── MainMenu.unity     # 메인 화면
    ├── StageSelect.unity  # 스테이지 선택 화면
    └── GamePlay.unity     # 게임플레이 화면
```

## 🚀 핵심 기능
1. 자동 플레이 시스템

FSM 기반 AI로 플레이어가 자동으로 전진하며 전투
적 감지 → 공격 → 이동 패턴

2. ScriptableObject

게임 데이터를 ScriptableObject로 관리
코드 수정 없이 기획 데이터 변경 가능

3. 이벤트 기반 아키텍처

시스템 간 느슨한 결합으로 확장성 확보
UI 자동 업데이트 및 반응성 구현


## 🎯 향후 개발 계획
Phase 1: 게임플레이 확장

 다양한 스킬 시스템
 보스전 특별 패턴
 스테이지 다양화

Phase 2: 콘텐츠 추가

 캐릭터 레벨업 시스템
 아이템 강화 시스템
 업적 시스템

Phase 3: 메타 게임

 세이브/로드 시스템
 일일 미션


👥 개발 팀

개발자: 고윤아


📄 라이선스
이 프로젝트는 개인 학습 목적으로 제작되었습니다.

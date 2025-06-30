# 길드마스터 구현 가이드

## 📋 프로젝트 개요

길드마스터는 **4부대 vs 4부대 대규모 전투**와 **3D 아이소메트릭 길드 건설**을 특징으로 하는 턴제 자동 방치형 싱글플레이어 게임입니다.

### 핵심 사양
- **전투 시스템**: 각 부대 9명 × 4부대 = 36명 vs 36명 (총 72명)
- **화면 모드**: 1920x1080 가로 모드 전용
- **플랫폼**: PC(Unity), 모바일 (iOS/Android)
- **데이터**: CSV 기반 (Unity Editor에서 생성 가능)

## 🎮 구현된 주요 시스템

### 1. 대규모 전투 시스템
- **4부대 시스템**: 각 길드는 4개 부대 운영
- **6x3 그리드**: 각 부대는 6x3 그리드에 9명 배치
- **턴제 진행**: 부대별로 순서대로 행동
- **전술 시스템**: 6가지 전술 (균형, 공격적, 방어적, 측면공격, 집중공격, 전격전)
- **시너지 시스템**: 직업 조합에 따른 보너스

### 2. 길드 건설 시스템
- **3D 아이소메트릭 뷰**: 45도 각도의 3D 건물 배치
- **20x20 그리드**: 자유로운 건물 배치
- **10가지 건물 타입**: 길드홀, 훈련소, 병영, 연구소, 신전 등
- **드래그 앤 드롭**: 직관적인 건물 배치
- **건설 큐**: 여러 건물 동시 건설 가능

### 3. JRPG 직업 시스템 (7개 직업)
1. **전사 (Warrior)**: 높은 HP와 물리 공격
2. **기사 (Knight)**: 최고의 방어력과 보호 능력  
3. **마법사 (Wizard)**: 강력한 마법 공격
4. **성직자 (Priest)**: 치유와 버프 전문
5. **암살자 (Assassin)**: 크리티컬과 속도
6. **궁수 (Ranger)**: 원거리 물리 공격
7. **현자 (Sage)**: 마법과 물리 복합

### 4. 자원 관리 시스템
- **5가지 자원**: 골드, 목재, 석재, 마나스톤, 평판
- **자동 생산**: 건물별 시간당 자원 생산
- **자원 한계**: 창고 레벨에 따른 저장 한계
- **거래 시스템**: 자원 교환 및 시장 가치

### 5. UI/UX (가로 모드 전용)
- **왼쪽 패널 (35%)**: 3D 길드 뷰
- **중앙 패널 (35%)**: 주요 콘텐츠 버튼
- **오른쪽 패널 (30%)**: 4부대 편성
- **상단 바**: 자원 현황 및 길드 정보
- **하단 바**: 빠른 접근 메뉴

## 📁 프로젝트 구조

```
Assets/
├── Scripts/
│   ├── Battle/             # 전투 시스템
│   │   ├── Squad.cs        # 부대 관리 (9명)
│   │   ├── Unit.cs         # 개별 유닛
│   │   └── SquadBattleSystem.cs  # 4vs4 전투
│   ├── Core/               # 핵심 시스템
│   │   ├── GameManager.cs  # 게임 매니저
│   │   ├── BattleManager.cs # 전투 관리
│   │   └── ResourceManager.cs # 자원 관리
│   ├── Guild/              # 길드 시스템
│   │   ├── GuildBuildingSystem.cs # 건물 시스템
│   │   └── BuildingData.cs # 건물 데이터
│   ├── UI/                 # UI 시스템
│   │   └── HorizontalUILayout.cs # 가로 모드 UI
│   └── Editor/             # 에디터 도구
│       └── CSVDataGenerator.cs # CSV 생성기
└── CSV/                    # 게임 데이터
    ├── character_data.csv  # 60명 캐릭터
    ├── building_data.csv   # 건물 정보
    ├── building_effects.csv # 건물 효과
    ├── building_production.csv # 생산량
    └── skill_data.csv      # 스킬 데이터
```

## 🚀 게임 실행 방법

### 1. Unity 에디터에서 실행
1. Unity 2021.3 이상으로 프로젝트 열기
2. `Main.cs`가 있는 씬 로드
3. Play 버튼 클릭

### 2. CSV 데이터 생성
1. Unity 메뉴바 → `GuildMaster` → `Generate CSV Data`
2. 원하는 데이터 생성 버튼 클릭
3. `Assets/CSV/` 폴더에 파일 생성됨

### 3. 디버그 모드 (F1 키)
- **1-5 키**: 자원 추가 (골드, 목재, 석재, 마나, 평판)
- **A 키**: 랜덤 모험가 모집
- **B 키**: 테스트 전투 시작  
- **S 키**: 게임 속도 조절 (1x → 2x → 4x)

## 📊 CSV 데이터 구조

### character_data.csv
```csv
ID,Name,JobClass,Level,Rarity,HP,MP,Attack,Defense,MagicPower,Speed,CritRate,CritDamage,Accuracy,Evasion,Skill1,Skill2,Skill3,Description
char_001,철검의 라이언,Warrior,1,Common,120,30,15,10,5,8,0.10,1.5,0.95,0.05,101,102,103,"검술에 능한 신참 전사..."
```

### building_data.csv
```csv
buildingId,buildingName,buildingType,category,sizeX,sizeY,gold,wood,stone,mana,buildTime,requiredLevel,maxLevel,description
guild_hall,길드 홀,GuildHall,Core,3,3,0,0,0,0,0,1,10,"길드의 중심 건물..."
```

### skill_data.csv
```csv
skillId,skillName,description,skillType,targetType,damageMultiplier,healAmount,buffType,buffAmount,buffDuration,manaCost,cooldown,range,areaOfEffect
101,기본 공격,"대상에게 물리 피해를 입힙니다.",Attack,Enemy,1.0,0,None,0,0,0,0,1,0
```

## 🔧 주요 클래스 설명

### Squad.cs
- **MAX_UNITS = 9**: 각 부대는 9명으로 구성
- **6x3 그리드**: 전략적 배치 가능
- **Formation**: 5가지 진형 (표준, 방어, 공격, 원거리, 치유)

### BattleManager.cs  
- **SQUADS_PER_SIDE = 4**: 각 진영 4개 부대
- **TOTAL_UNITS_PER_SIDE = 36**: 총 36명
- **턴제 진행**: 부대 단위로 순차 행동

### GuildBuildingSystem.cs
- **20x20 그리드**: 건물 배치 공간
- **드래그 앤 드롭**: 마우스로 건물 배치
- **건설 큐**: 여러 건물 동시 진행

### HorizontalUILayout.cs
- **화면 분할**: 좌(35%), 중앙(35%), 우(30%)
- **반응형 UI**: 해상도에 따라 자동 조정
- **가로 모드 고정**: 세로 모드 자동 전환

## 📝 추가 구현 팁

### 1. 새로운 캐릭터 추가
```csharp
// CSVDataGenerator.cs에서
sb.AppendLine("char_061,새로운영웅,Warrior,5,Rare,300,50,35,25,15,12,0.18,1.8,0.96,0.08,101,106,107,\"설명\"");
```

### 2. 새로운 건물 추가
```csharp
// building_data.csv에 추가
"new_building,새건물,NewType,Special,2,2,1000,500,300,100,180,5,5,\"새로운 건물 설명\""
```

### 3. 스킬 추가
```csharp
// skill_data.csv에 추가
"720,새스킬,\"스킬 설명\",Attack,Enemy,2.0,0,None,0,0,50,10,3,0"
```

## 🐛 문제 해결

### CSV 파일이 로드되지 않을 때
1. `Assets/CSV/` 폴더 확인
2. 파일 인코딩이 UTF-8인지 확인
3. DataManager의 LoadAllData() 호출 확인

### 전투가 시작되지 않을 때
1. 각 부대에 최소 1명 이상 배치 확인
2. BattleManager 인스턴스 생성 확인
3. 유닛의 HP가 0이 아닌지 확인

### UI가 제대로 표시되지 않을 때
1. Canvas의 Render Mode가 Screen Space인지 확인
2. 해상도가 16:9 비율인지 확인
3. HorizontalUILayout 컴포넌트 연결 확인

## 📌 중요 참고사항

1. **싱글플레이어 전용**: 멀티플레이어 기능 없음
2. **자동 저장**: 5분마다 자동 저장
3. **최대 부대 수**: 4개 고정
4. **부대당 인원**: 9명 고정
5. **가로 모드**: 세로 모드 미지원

## 🎯 다음 단계 개발 제안

1. **Phase 2 기능**
   - AI 길드 대항전
   - NPC 교역 시스템
   - 던전 탐험

2. **콘텐츠 확장**
   - 신규 직업 추가
   - 이벤트 던전
   - 시즌 패스

3. **편의 기능**
   - 일괄 전투 스킵
   - 자동 편성
   - 전투 리플레이

---

이 가이드를 참고하여 길드마스터 게임을 성공적으로 실행하고 확장해 나가시기 바랍니다!
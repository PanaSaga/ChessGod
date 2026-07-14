# CLAUDE.md

이 파일은 Claude Code가 세션 시작 시 자동으로 읽는 프로젝트 컨텍스트 파일입니다.
담당 범위는 **UI 제작 + 인게임 연결**이며, 인게임 체스 로직 자체는 별도 담당(시스템/기획)입니다.
이 문서는 실제 저장소(`c:\PJ\ChessGod\ChessGod`) 코드를 직접 확인하며 작성했고,
과거 별도 폴더(Rfilezip)에서 설계만 하고 실제로는 반영되지 않았던 문서를 대체합니다.

**씬/프리팹 배치 작업은 `EDITOR_SETUP_GUIDE.md`(프로젝트 루트) 참고.** mcp-unity 연결
문제(Unity 창이 포커스를 잃으면 요청이 전부 타임아웃되는 Windows 전용 알려진 버그,
GitHub이슈 #147/#150)를 해결한 뒤로 씬 생성/GameObject/컴포넌트 구조는 도구로 자동
배치 완료. 다만 이 도구는 씬 내부 오브젝트 참조 필드(에셋 참조 아닌 것)는 설정 불가
확인됨 — 그 부분(슬라이더→컨트롤러 연결 등)만 인스펙터에서 직접 드래그 필요, 가이드
문서에 남은 목록 정리해둠.

---

## 프로젝트 개요

- **엔진:** Unity 6 / **정식 게임명(가제):** 체크메이트 러너
- **플랫폼:** Itch.io (WebGL) / **해상도 기준:** 1920×1080
- **조작:** 방향키 이동, 스페이스바로 턴 즉시 종료
- **한 줄 소개:** 체스 세계관의 킹(사실은 프로모션한 폰=주인공 광대)이 되어, 적 체스말(흑)의 공격 범위를 피하며 아군 체스말(백, 아이템 역할)을 주워 프로모션하고 스테이지를 통과하는 게임
- **핵심 순환:** 스테이지 클리어 → 점수 획득 → 업적 달성 → 스토리 컷씬 해금 → (반복)
- **핵심 용어:** 턴(10초 단위 스폰 주기) / 스테이지 레벨(턴의 10의 자리) / 라이프(기본 3) / 프로모션(백의 말 획득 시 20초간 해당 말로 변신)

## 확정된 업적 7종 (참고용 표 — 조건 판정 로직은 미구현)

| id | 업적명 | 조건 | 목표치 |
|---|---|---|---|
| ach_checkmate | 체크메이트 | 흑의 킹 제거 누적 10회 | 10 |
| ach_fork | 포크 | 한 번의 공격으로 체스말 2개 이상 잡기 누적 10회 | 10 |
| ach_upset | 하극상 | 나이트로 퀸 처치 누적 10회 | 10 |
| ach_brilliant | 탁월수 | 킹이 아닌 상태로 적의 퀸과 동귀어진 누적 5회 | 5 |
| ach_stalemate | 스테일 메이트 | 보드 위에 흑의 체스말 10개 이상일 때 생존 누적 3회 | 3 |
| ach_genocide | 제노사이드 | 2스테이지 이후 보드 위의 체스말을 전부 제거 누적 3회 | 3 |
| ach_rating_master | 레이팅 마스터 | 점수 2000점 이상 달성 | 2000 |

`ach_rating_master`는 다른 업적과 달리 "누적 증가"가 아니라 "현재 점수를 그대로 진행도로" 취급해야 함 — `IncrementAchievementProgress` 헬퍼(1씩 증가 전용)를 그대로 쓰면 안 됨, 별도 처리 필요.

---

## 실제 저장소 상태 (2026-07-14 확인 기준)

### 이미 완성되어 있는 것 (인게임, 절대 직접 수정하지 않음)
`Assets/Scripts/InGame/` 전체 — `GameManager`, `SpawnManager`, `ControlManager`, `BoardManager`, `BoardViewManager`, `PlayerPiece`, `ChessPiece` 계열, `InGameUIManager`(세션 값 HUD, 이미 완성도 높음). 씬은 `Assets/Scenes/InGame.unity` 하나뿐, Build Settings에 씬 등록도 안 되어 있음.

### 이번 세션에 새로 만든 것 (UI/연결 매니저)
- `Assets/Scripts/Contracts/IGameDataProvider.cs` — 계약 인터페이스
- `Assets/Scripts/Core/` — `GlobalManager`, `BootLoader`, `DataManager`, `GameDataProvider`
- `Assets/Scripts/EventChannels/` — `GameEvent`, `GameEventListener`, `AchievementUnlockedEvent`, `AchievementEventListener`
- `Assets/Scripts/Achievements/` — `GameSessionObserver`, `AchievementTracker`, `AchievementPopulator`, `AchievementSlotUI`, `AchievementToastManager`

이 스크립트들은 작성만 되어 있고 **씬 배치/필드 연결은 아직 안 됨** (Unity 에디터 연결이 안 된 상태로 작업함).

---

## 핵심 설계 원칙 (이번 세션에 확정)

1. **인게임 완성본은 절대 직접 수정하지 않는다.** `GameManager.cs`를 포함한 `Assets/Scripts/InGame/` 전체가 대상. 업적 판정에 필요한 신호는 `GameSessionObserver`가 매 프레임 `GameManager.Instance`의 public 상태를 diff해서 재구성한다 (이벤트가 없어서 직접 만들 수 없기 때문).
2. **UI는 완성된 인게임 브랜치 위에서 이어서 만든다.** 별도 팀이 계약 파일로만 연결되는 구조가 아니라, 하나의 브랜치 계보 위에서 진행.
3. **씬은 분리한다.** `InGame.unity`는 GameScene 역할로 그대로 두고, `BootScene`/`MainScene`/`GalleryScene`을 새로 만든다. (근거: 완성된 씬도 안 건드린다는 원칙의 연장 + WebGL 메모리 절약 + 이미 짜여있던 `BootLoader`/`SceneLoader` 설계 재사용 가능)
4. **저장은 JSON 구조 + PlayerPrefs 저장소.** `File.WriteAllText`가 아니라 `PlayerPrefs.SetString`으로 JSON 블롭을 저장 (WebGL의 IndexedDB 동기화 문제 회피). 볼륨 등 단순 값은 PlayerPrefs 개별 키 그대로 사용.
5. **업적 아이콘은 업적별 고유.** 7개 각각 별도 리소스 필요 (등급 재사용 아님).
6. **판단은 게임 로직, 표시는 UI.** 업적 달성 여부 "판단"은 `AchievementTracker`, "표시"는 `AchievementPopulator`/`AchievementSlotUI` — 단, 실제 판정 조건은 콘텐츠가 완전히 확정되기 전까지 TODO로 비워둔다.
7. **모호한 결정은 질문한다.** 씬 구조, 저장 방식, 아이콘 정책처럼 여러 갈래가 가능한 지점은 임의로 정하지 않고 먼저 확인한다.
8. **콘텐츠 리소스가 없는 화면은 플레이스홀더로 우선 진행**하고, 리소스가 도착하면 교체한다 (기능 개발이 리소스 대기로 막히지 않게).
9. **ESC는 로비/인게임 모두에서 토글되지만 내용이 다르다.** 로비=설정 팝업(볼륨+도움말+메인으로+처음부터), 인게임=일시정지 팝업(볼륨+도움말+게임으로돌아가기+게임종료→메인화면, "처음부터"는 없음). 두 팝업 모두 SFX/BGM 슬라이더는 동일하게 포함.
10. **일시정지도 `GameManager.cs`를 건드리지 않고 구현한다.** `PopupUI.OnOpened`/`OnClosed` 이벤트를 새 `InGamePauseController`가 구독해서, 열릴 때 `Time.timeScale = 0` + `GameManager.Instance.enabled = false`, 닫힐 때 원복. `enabled=false`로 `Update()` 자체를 막아야 일시정지 중 스페이스바로 턴이 넘어가는 사고를 막을 수 있고(`Time.timeScale`만으론 `Update()`가 계속 호출돼 입력 체크가 새어나감), `Time.timeScale=0`은 진행 중이던 정산 코루틴(`WaitForSeconds`)까지 함께 얼려준다.
11. **`DontDestroyOnLoad`는 호출된 오브젝트와 그 자식만 보존한다 (형제는 안 됨).** `DataManager`/`GameDataProvider`(및 추후 `SoundManager`/`CommonUIManager`/업적 토스트)는 반드시 `GlobalManager`가 붙은 오브젝트의 **자식**으로 배치해야 한다. 형제 오브젝트로 두면 첫 씬 전환에서 그대로 파괴되는 실제 버그가 난다.
12. **BootScene에는 `EventSystem`을 두지 않는다.** `BootLoader`가 MainScene을 Additive로 겹쳐 로드해서 Boot+Main이 동시에 활성 상태이므로, `EventSystem`은 MainScene(과 GalleryScene/InGame처럼 Single 로드로 전환되는 씬들)에만 존재해야 중복 경고/입력 꼬임을 피한다.
13. **업적 토스트는 영구 매니저 계층에 둔다.** `GameSessionObserver`/`AchievementTracker`는 `GameManager.Instance`에 의존하므로 InGame 씬 로컬로 유지하되, `AchievementToastManager`(+ 이를 구독하는 `AchievementEventListener`)는 `GlobalManager` 하위(영구 유지, `AchievementUnlockedEvent`는 ScriptableObject라 씬 무관하게 참조 가능)로 옮겨서 게임오버→로비 씬 전환 중에도 토스트가 끊기지 않게 한다.

---

## 작업 프로세스 (본인 담당: UI 제작 + 인게임 연결만, 마감 없음·품질 우선·공통 인프라 우선)

### Phase 0 — 공통 인프라
- [x] `IGameDataProvider` 계약 + 데이터 클래스
- [x] `GlobalManager` / `BootLoader` / `DataManager`(PlayerPrefs+JSON) / `GameDataProvider`
- [x] 이벤트 채널 4종
- [ ] `BootScene`/`MainScene`/`GalleryScene` 씬 생성 (에디터 작업)
- [ ] 위 3개 + `InGame.unity`를 Build Settings에 등록
- [ ] `BootScene`에 `GlobalManager` 오브젝트 배치, **그 자식으로** `DataManager`+`GameDataProvider`(같은 오브젝트) 배치 — 형제로 두지 않기 (설계 원칙 11번)
- [ ] `BootLoader`로 `MainScene` Additive 로드 연결
- [ ] `EventSystem`은 `BootScene`엔 두지 않고 `MainScene`에만 배치 (설계 원칙 12번)

### Phase 1 — 업적 시스템
- [x] `GameSessionObserver` (GameManager 비수정 관찰자)
- [x] `AchievementTracker` 골격 (판정 조건 TODO)
- [x] `AchievementPopulator` / `AchievementSlotUI`(아이콘+배지) / `AchievementToastManager`
- [ ] `InGame.unity`에 `GameSessionObserver`+`AchievementTracker` 오브젝트 배치 (씬 로컬, GameManager.Instance 의존이라 영구 유지 대상 아님)
- [ ] `AchievementToastManager`+`AchievementEventListener`는 `GlobalManager` 하위(영구 유지)로 배치 (설계 원칙 13번 — 씬 전환 중 토스트가 끊기지 않도록)
- [ ] `MainScene`에 업적 패널 배치 (도면 기준: 진행도/달성/미달성 탭 + 아이콘/이름/조건/진행도/배지 한 줄 구성), 아이콘은 플레이스홀더로 시작
- [ ] `GameSessionObserver`가 `SpawnManager`도 볼 수 있어야 하는 조건("스테일 메이트", "제노사이드") 확인 후 확장
- [ ] "탁월수" 조건은 구조적으로 발생 불가 — **시스템 담당자 확인 전까지 보류**
- [ ] 콘텐츠 확정 후 `AchievementTracker`의 TODO 5곳 판정 로직 채우기

### Phase 2 — 로비 화면 (MainScene)
- [x] `LobbyManager`(프롤로그 체크) — 작성 완료, 배치는 미착수
- [x] `SceneLoader` — 작성 완료 (게임 시작/갤러리 버튼 연결은 에디터에서)
- [x] `CommonUIManager` + `PopupUI` — 작성 완료 (Esc 토글 구조)
- [x] `LobbyCharacterInteraction` — 클릭 감지 스캐폴드만 작성, `OnCharacterClicked`는 미바인딩
- [ ] 위 전부 `MainScene`에 실제 배치 (에디터 작업, 아직 씬 자체가 없음)
- [ ] 캐릭터 아트·배경 플레이스홀더 배치
- [ ] **중앙 캐릭터 클릭 반응 내용**: 대사/표정/사운드 등 콘텐츠 미정 — 확정되면 `LobbyCharacterInteraction.OnCharacterClicked`에 연결

### Phase 3 — 설정 팝업(로비) / 일시정지 팝업(인게임)
- [x] `SoundManager`(BGM/SFX 페이드, 볼륨/뮤트, `GlobalManager` 하위 영구 배치용) — 작성 완료. `Awake()`에서 `DataManager` 저장값을 즉시 적용하도록 함(기존 설계엔 없던 보완 — 설정 팝업을 열기 전에도 저장된 볼륨/뮤트가 바로 반영됨)
- [x] `ConfirmDialogManager` + `ResetProgressHandler`("처음부터", 로비 전용) — 작성 완료
- [x] `InGamePauseController` — 작성 완료. `PopupUI.OnOpened`/`OnClosed`를 인스펙터에서 연결해 사용(Esc 감지 자체는 소유하지 않음), `OnDisable()`에 `Time.timeScale` 복구 안전장치 포함(일시정지 중 씬 전환 시 다음 씬이 멈춰있는 사고 방지)
- [x] `SettingsPopupController` — 배치 가이드 작성 중 발견한 누락분. `SoundManager`가 `GlobalManager` 하위(다른 씬)에 있어 인스펙터로 직접 못 꽂으므로, 런타임에 `FindFirstObjectByType`로 찾아 슬라이더/토글과 연결하는 다리. 로비/일시정지 팝업 둘 다 공유
- [ ] 로비 설정 팝업 프리팹: SFX/BGM 체크박스+슬라이더, 도움말/게임으로돌아가기(닫기)/메인으로/처음부터
- [ ] 인게임 일시정지 팝업 프리팹: SFX/BGM 체크박스+슬라이더(로비와 동일 구성 재사용), 도움말/게임으로돌아가기(재개)/게임종료→메인화면 ("처음부터" 없음)
- [ ] 위 2개 프리팹 + 매니저들 씬에 실제 배치

### Phase 4 — 갤러리 화면
- [x] `GalleryPopulator` / `GallerySlotUI` 작성 — 잠금 표시는 기존 `lockedOverlay`(오브젝트 통째로 SetActive)로 충분해서 별도 필드 추가는 안 함. 도면의 "GM-01/OM-01" 코드 접두사 표기는 두 장의 도면이 서로 형식이 달라(단순 "01." vs "GM-01.") 확정된 게 아니라고 판단, 임의로 정하지 않고 기존 "01." 형식 유지 — 접두사 필요하면 알려주면 반영
- [ ] 씬에 배치 (책 펼침 레이아웃, 좌우 페이지, 우측 세로 탭 스토리/오마케), 실제 이미지 도착 전까지 플레이스홀더
- [ ] `GameDataProvider.GetUnlockedGalleryItems()`에 실제 항목 대신 임시 데이터 연결 (현재는 빈 리스트 반환 — 실제 개수/제목은 아직 미확정이라 임의로 채우지 않음)

### Phase 5 — 스토리/컷씬
- [x] `StoryManager` 작성 — Phase 2의 `LobbyManager`가 참조해야 컴파일되어서 순서를 당겨 먼저 작성함. `EndStory()`가 `storyId == "prologue"`일 때만 시청 여부를 저장하도록 수정함(원안은 갤러리 재생에서도 항상 저장해버리는 버그가 있었음)
- [ ] 씬에 배치
- [ ] `GameDataProvider.GetStoryLines()`에 실제 대사 대신 임시 텍스트로 우선 연결, 대사 확정되면 교체 (현재는 빈 리스트 반환)
- [ ] 프롤로그 최초 1회 재생 흐름 확인

### Phase 6 — 인게임 잔여 작업
- [ ] 게임 오버 → 로비 복귀: `GameSessionObserver.OnGameOver` 구독해서 씬 전환 (Phase 0에서 MainScene이 생긴 뒤 가능)
- [ ] CLAUDE.md 구버전에 남아있던 "프로모션 지속시간 아이콘" 등 미착수 HUD 항목 재확인 — `InGameUIManager`가 이미 상당 부분 처리하고 있어 실제로 남은 게 있는지부터 점검

### Phase 7 — 통합 확인
- [ ] Boot→Main→Game→GameOver→Main, 갤러리 왕복, 설정 저장 전체 플로우 테스트
- [ ] WebGL 빌드로 새로고침 후 저장 데이터 유지되는지 확인 (PlayerPrefs 특성상 시크릿 모드 등에서 실패할 수 있음, 알려진 한계로 인지)

---

## 막혀있는 사항 (남 확인 필요, 본인 작업 범위 아님)

- **"탁월수" 업적**: `GameManager.SettlementRoutine()`에서 플레이어가 죽으면 `ResolvePlayerAttack()`이 아예 실행되지 않아 동귀어진 조건 자체가 발생 불가. UI/연결 쪽에서 우회 불가능 — 시스템 담당자가 순서를 조정해야 함.
- **업적 아이콘 7종, 갤러리 이미지, 스토리 대사**: 아직 리소스 없음 — 플레이스홀더로 진행 중, 도착 시 교체.
- **로비 캐릭터 클릭 반응 내용**: 클릭 감지 자체는 구현하지만, 실제로 무엇을 보여줄지(대사 랜덤 출력/표정 변화/사운드 등)는 아직 미정.
- **로비 설정 팝업의 "메인으로" 버튼**: 이미 로비에 있는 상태에서 "메인으로"가 뭘 뜻하는지 불명확 (도면이 로비/인게임 구분 전에 그려짐). `EDITOR_SETUP_GUIDE.md`에서 연결 보류.

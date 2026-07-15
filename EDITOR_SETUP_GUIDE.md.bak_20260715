# 에디터 배치 가이드 (구조 자동화 완료 — 남은 건 연결/시각 작업뿐)

mcp-unity 연결(Unity 창 포커스 유지가 관건이었음)이 되고부터, 씬 생성과
GameObject/컴포넌트 구조는 전부 도구로 만들어뒀습니다. **다만 이 도구는 씬 안의
다른 오브젝트를 참조하는 필드(예: 슬라이더→컨트롤러, 버튼→팝업)는 설정할 수
없다는 걸 확인했습니다** (에셋 참조만 가능, mcp-unity 소스 코드로 확인) — 그래서
아래 "남은 작업"이 실제로 손으로 해야 하는 전부입니다.

기존 `Assets/Scripts/InGame/**`와 `InGame.unity`의 기존 오브젝트는 이번 자동화
과정에서 전혀 건드리지 않았고, 새 오브젝트만 추가했습니다.

---

## 자동으로 완료된 것

### Build Settings
- [x] BootScene / MainScene / GalleryScene 등록됨
- [ ] **`InGame` 씬은 자동 등록이 안 됩니다 — Build Settings 창에서 직접 드래그해서 추가 필요**
- [ ] 등록 순서 확인·정리 필요: 현재 `SampleScene(레거시 기본 씬, 삭제 권장) → BootScene → MainScene → GalleryScene` 순으로 되어 있고 `InGame`이 빠져있음. **Build Settings 창 열어서 SampleScene 항목 제거, InGame 추가, 순서를 BootScene→MainScene→GalleryScene→InGame으로 정렬**

### BootScene
`GlobalManager`(자식: `DataProvider`[DataManager+GameDataProvider], `SoundManager`[AudioSource 자식 2개: BgmSource/SfxSource], `PersistentOverlay`[Canvas+CanvasScaler, 자식: `ToastRoot`, `AchievementToastManager`[AchievementToastManager+AchievementEventListener 컴포넌트], `ToastRoot/AchievementToastTemplate`[토스트 프리팹 원본]]), `BootLoader`(firstSceneName="MainScene" 값까지 설정됨)

### MainScene
`EventSystem`, `Canvas`(Screen Space-Overlay, 1920×1080 스케일러 설정까지 완료), 그 하위:
- `AchievementPanel`(AchievementPopulator 컴포넌트) → `Content` → `AchievementSlotTemplate`(Icon/Title/Condition/Progress/BadgeBackground/BadgeText 전부 존재)
- `CharacterButton`(LobbyCharacterInteraction), `StartGameButton`(SceneLoader→"InGame"), `GalleryButton`(SceneLoader→"GalleryScene"), `SettingsButton`, `HelpButton`
- `SettingsPopup`(PopupUI+SettingsPopupController) → `BgmSlider`/`BgmMuteToggle`/`SfxSlider`/`SfxMuteToggle`/`HelpButton`/`CloseButton`/`MainButton`/`ResetButton`(ResetProgressHandler)
- `ConfirmDialogRoot`(비활성 상태로 시작) → `MessageText`/`YesButton`/`NoButton`
- 씬 최상위: `LobbyManager`, `StoryManager`, `ConfirmDialogManager`, `CommonUIManager`

### GalleryScene
`EventSystem`, `Canvas`(동일 설정), `StoryManager`, 그 하위:
- `GalleryPanel`(GalleryPopulator) → `Content` → `GallerySlotTemplate`(Thumbnail/Title/LockedOverlay)
- `TabStory`/`TabOmake`/`PrevButton`/`NextButton`/`ExitButton`(SceneLoader→"MainScene")

### InGame.unity (기존 오브젝트는 안 건드림, 새로 추가만)
`EventSystem`, `AchievementBridge`(GameSessionObserver+AchievementTracker), `PauseController`(InGamePauseController), `CommonUIManager`, 기존 `Canvas` 밑에 `PausePopup`(PopupUI+SettingsPopupController) → `BgmSlider`/`BgmMuteToggle`/`SfxSlider`/`SfxMuteToggle`/`HelpButton`/`ResumeButton`/`QuitToMainButton`(SceneLoader→"MainScene")

---

## 남은 작업 (전부 인스펙터에서 드래그/클릭만 하면 됨)

### 1. Build Settings 정리 (위 참고)

### 2. 표준 UI 위젯 시각 요소 보정
**위치/크기는 도면 기준으로 대략 배치 완료함** (RectTransform anchoredPosition/sizeDelta는 참조가 아니라 숫자값이라 자동화 가능했음 — MainScene/GalleryScene/InGame 전부 적용, `AchievementPanel/Content`·`GalleryPanel/Content`는 자동 정렬용 Layout Group도 붙여둠). 다만 Slider/Toggle/Button은 **배경·핸들·색상 같은 시각적 하위구조가 비어있습니다**(mcp-unity가 컴포넌트 추가는 되지만 Unity 메뉴 기반 UI 생성은 안 돼서, 껍데기만 만들어짐 — Button은 targetGraphic이 자동으로 자기 자신의 Image로 연결되는 등 기본 동작은 되지만 예쁘진 않음). 배경/핸들 이미지 추가나 색상 조정은 이 시점에 취향껏 진행하시면 됩니다.

### 3. 프리팹으로 만들기 (드래그 3번)
- `MainScene`의 `Canvas/AchievementPanel/Content/AchievementSlotTemplate`를 `Assets/Prefabs/`로 드래그 → 프리팹 생성 후 원본은 씬에서 삭제
- `GalleryScene`의 `Canvas/GalleryPanel/Content/GallerySlotTemplate`를 `Assets/Prefabs/`로 드래그 → 동일
- `BootScene`의 `GlobalManager/PersistentOverlay/ToastRoot/AchievementToastTemplate`를 `Assets/Prefabs/`로 드래그 → 동일
- 각 씬의 `AchievementPopulator.achievementSlotPrefab` / `GalleryPopulator.gallerySlotPrefab` / `AchievementToastManager.toastPrefab` 필드에 방금 만든 프리팹 드래그

### 4. ScriptableObject 이벤트 에셋 2개 생성
Project 창에서 `Create > Events > Achievement Unlocked Event`(이름: `OnAchievementUnlocked`), `Create > Events > Game Event`(이름: `OnSceneTransition`, 선택사항) → `Assets/ScriptableObjects/`에 저장. (도구로 시도했으나 인라인 이름짓기 UI 때문에 응답이 걸려서 직접 만드는 게 안전함)

### 5. 씬 내부 참조 연결 (mcp-unity로 절대 불가능했던 부분 — 전부 인스펙터 드래그)

**BootScene**
- `SoundManager`의 `bgmSource`→`BgmSource`, `sfxSource`→`SfxSource`
- `AchievementToastManager`의 `toastPrefab`→3번에서 만든 프리팹, `toastRoot`→`ToastRoot`
- `AchievementEventListener`의 `achievementEvent`→`OnAchievementUnlocked.asset`, response→`AchievementToastManager.Enqueue`

**MainScene**
- `AchievementPopulator`의 `contentParent`→`Content`, `achievementSlotPrefab`→프리팹, `icons`에 업적 7종 아이콘(플레이스홀더) 연결
- 탭 버튼 3개 OnClick()→`AchievementPopulator.ShowAll/ShowCompletedOnly/ShowIncompleteOnly` (지금은 탭 버튼 자체가 없으니 필요하면 추가로 3개 만들기)
- `CommonUIManager`의 `escTogglePopup`→`SettingsPopup`, `popupPairs`에 (SettingsButton, SettingsPopup) 등록
- `LobbyManager`의 `storyManager`→`StoryManager`
- `SettingsPopup`의 각 Slider/Toggle OnValueChanged()→`SettingsPopupController.OnBgmSliderChanged` 등 4종, `PopupUI.OnOpened`→`SettingsPopupController.RefreshFromSoundManager`, `CloseButton`→`PopupUI.Close()`, `ResetButton`은 이미 `ResetProgressHandler` 붙어있으니 OnClick()→`OnResetButtonClicked()`, `MainButton`은 **연결 보류**(의미 불명확, EDITOR_SETUP_GUIDE 이전 버전 참고)
- `ConfirmDialogManager`의 `dialogRoot`→`ConfirmDialogRoot`, `messageText`→`MessageText`, `yesButton`/`noButton`→`YesButton`/`NoButton`
- `StoryManager`의 필드들(`storyRoot`/`backgroundImage`/`characterImage`/`nameText`/`dialogueText`/`nextButton`)은 스토리 UI를 아직 안 만들어서 비어있음 — 스토리 화면 만들 때 연결

**GalleryScene**
- `GalleryPopulator`의 `contentParent`→`Content`, `gallerySlotPrefab`→프리팹, `storyManager`→씬의 `StoryManager`, `prevButton`/`nextButton`→`PrevButton`/`NextButton`
- 탭 버튼 OnClick()→`ShowCategory("story")` / `ShowCategory("omake")`
- `ExitButton`은 이미 SceneLoader 붙어있어서 추가 연결 불필요

**InGame.unity**
- `AchievementTracker`의 `achievementUnlockedEvent`→`OnAchievementUnlocked.asset` (BootScene 토스트와 같은 에셋)
- `CommonUIManager`의 `escTogglePopup`→`PausePopup`
- `PausePopup`의 `PopupUI.OnOpened`→`InGamePauseController.OnPausePopupOpened` + `SettingsPopupController.RefreshFromSoundManager`, `OnClosed`→`InGamePauseController.OnPausePopupClosed`
- `ResumeButton`→`PopupUI.Close()`, `QuitToMainButton`은 이미 SceneLoader 붙어있음
- 슬라이더/토글 4개 OnValueChanged()→`SettingsPopupController`의 대응 메서드

---

## 확인 필요 (임의로 안 정한 것)
- 로비 설정 팝업의 "메인으로" 버튼 — 이미 로비인데 "메인으로"가 뭘 뜻하는지 불명확, 연결 보류
- 업적 아이콘 7종, 갤러리 썸네일, 캐릭터 아트, 갤러리 코드 접두사 형식 — 전부 플레이스홀더/미확정

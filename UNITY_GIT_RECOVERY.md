# Unity 프로젝트 Git 복구 안내 (2026-11-26)

이 저장소는 Unity **6000.3.23f1** 프로젝트 파일을 포함합니다. 현재 `main` 브랜치를 검사하면 다음 문제가 있습니다.

1. 저장소 최상위에 `Assets/`, `ProjectSettings/`는 있지만 `Packages/`가 없습니다. `My project/Packages/`는 다른 중첩 폴더에 있으며 그 안의 `manifest.json`에는 URP, Input System, TextMesh Pro 등의 레지스트리 패키지 의존성이 없습니다.
2. 기존 `.gitignore`가 `*.obj` 파일을 제외하여 전시 모델 파일이 빠졌습니다. 검사 시 `Assets/` 아래 OBJ 모델의 `.obj.meta`는 **66개** 존재하지만 해당 `.obj` 파일이 저장소 트리에 없습니다.
3. `Assets/Scene/mungahye.unity`는 누락된 `Assets/CharacterPortfolioKit/Models/Standee_01_Red.obj`의 GUID(`3ddf6b875e6bf744a8dc73223c97a5db`)를 16회 참조합니다. 따라서 씬의 모델 참조가 깨질 수 있습니다.
4. `My project/Library/`, `My project/Temp/`, `My project/Logs/`처럼 Unity가 자동 생성하는 캐시가 커밋되어 있습니다. 이 폴더는 버전 관리하지 않아도 됩니다.

## 안전한 복구 순서

**1. 원본 프로젝트를 전체 백업하세요.** 현재 정상 표시되는 Unity 프로젝트를 수정하기 전에 별도의 폴더에 복사하세요. `main` 브랜치를 강제로 초기화하거나, 씬/머티리얼 파일을 지우지 마세요.

**2. 실제 Unity 프로젝트 루트 확인:** `Assets/`, `Packages/`, `ProjectSettings/`가 같은 폴더에 있는지 확인하세요. Unity Hub에서 그 폴더를 열어야 합니다. 현재 저장소는 가장 바깥쪽이 프로젝트 루트의 후보이지만 `Packages/`가 누락돼 있습니다.

**3. 정상 작동하는 로컬 Unity 프로젝트의 `Packages/`를 루트에 복구:** 최소 `Packages/manifest.json`, 가능하면 `Packages/packages-lock.json`을 정상적으로 열리는 로컬 프로젝트에서 가져오세요. `My project/Packages/manifest.json`을 무조건 복사하지 마세요. 그 파일에는 사용 중인 URP/Input System 등 의존성이 빠져 있습니다. 원본이 없다면 Unity Package Manager에서 의존성을 올바르게 설치한 뒤 생성된 manifest를 커밋해야 합니다.

**4. 로컬의 원본 OBJ 모델 파일을 원래 경로에 복구:** `.obj.meta`와 같은 이름의 `.obj`를 각각 가져오세요. 예: `Assets/CharacterPortfolioKit/Models/Standee_01_Red.obj`. **기존 .meta는 삭제하거나 다시 생성하지 마세요.** GUID가 바뀌면 씬과 프리팹의 참조가 다시 끊깁니다.

**5. Fork에서 수정 내용을 커밋하고 Push:** 이 PR의 `.gitignore` 변경이 적용된 브랜치로 이동한 뒤, 원본 프로젝트에서 복구한 `Packages/`, `*.obj`, 필요한 Unity 플러그인 `*.dll`을 추가하세요. Fork의 Unstaged/Untracked 파일까지 확인하고 Commit 후 Push해야 원격 저장소에 반영됩니다.

**6. 별도의 폴더에 새로 Clone하여 재검증:** Unity Hub에서 프로젝트 루트를 Unity 6000.3.23f1로 열고, 컴파일 에러가 없는지 확인한 후 `Assets/Scene/Title.unity`, `Outside.unity`, `mungahye.unity`를 열어 머티리얼·모델·씬 이동을 확인하세요.

## 자동 생성 파일 정리 (선택)

`My project/Library/`, `My project/Temp/`, `My project/Logs/`, `My project/UserSettings/`는 기존에 이미 Git이 추적 중이라 `.gitignore`만 바꿔서는 원격에서 삭제되지 않습니다. **백업을 한 다음**, 폴더가 실제 작업 소스가 아닌 Unity 캐시임을 확인하고 추적만 해제할 수 있습니다:

```bash
git rm -r --cached -- "My project/Library" "My project/Temp" "My project/Logs" "My project/UserSettings"
git commit -m "Stop tracking nested Unity generated files"
git push
```

`--cached`를 사용하면 로컬 파일은 유지하면서 Git 추적만 해제하지만, 이후 커밋에서는 해당 파일이 저장소에서 삭제된 것으로 표시됩니다. 중요한 다른 작업이 포함되어 있지 않은지 먼저 확인하세요.

> 이 수정 제안은 **추적 규칙만 고칩니다.** 누락된 OBJ 모델 원본과 올바른 `Packages/manifest.json`은 GitHub 저장소만으로는 복원할 수 없으며 정상 로컬 프로젝트가 필요합니다. Unity 에디터에서 런타임 테스트를 완료한 것은 아닙니다.

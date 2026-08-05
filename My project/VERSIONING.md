# HandLink 버전 관리 규칙

- 표시 버전은 `MAJOR.MINOR.PATCH` 형식을 사용한다.
- Android `versionCode`는 Play Console에 업로드할 때마다 반드시 1 이상 증가시킨다. 이전 값으로 되돌리거나 재사용하지 않는다.
- 최초 공개 버전은 `1.0.0` / `versionCode` `1`이다.
- 버그 수정 릴리스는 `PATCH`를 증가시킨다. 예: `1.0.1` / `versionCode` `2`
- 기능 추가 릴리스는 `MINOR`를 증가시킨다. 예: `1.1.0` / 다음 `versionCode`
- 호환되지 않는 변경이 포함된 릴리스는 `MAJOR`를 증가시킨다. 예: `2.0.0` / 다음 `versionCode`

# The Whale Knows 🐋
> 2025년 1학기 미디어 프로젝트

<br/>

![version](https://img.shields.io/badge/version-demo-orange)
[![GitHub contributors](https://img.shields.io/github/contributors/VirtualExplorer7613/MediaProject.svg)](https://github.com/VirtualExplorer7613/MediaProject/graphs/contributors)
[![GitHub commit](https://img.shields.io/github/last-commit/VirtualExplorer7613/MediaProject.svg)](https://github.com/VirtualExplorer7613/MediaProject/commits)
[![Made with Unity](https://img.shields.io/badge/Made%20with-Unity-57b9d3.svg?style=flat&logo=unity)](https://unity.com)


---

## 📖 프로젝트 개요

**The Whale Knows**는 해양 쓰레기 문제를 주제로 한 3D 인터랙티브 게임입니다.  
플레이어는 '고래'가 되어 바다 속 쓰레기를 수집하거나, NPC와의 상호작용을 통해 스토리를 진행합니다.  
게임은 Unity로 제작되었으며, 환경 이슈와 감성적인 스토리텔링을 융합하여 몰입감을 극대화합니다.

> 🌊 바다는 기억하고 있다. 그 속의 이야기와 상처를…

---

## 🧩 주요 기능

### 🎮 플레이어 컨트롤
- FPS 기반 카메라 컨트롤 및 자유로운 이동
- 마우스를 이용한 회전, WASD로 전진/후진/좌우이동, QE로 상승/하강 
- 초음파 발사(F키) 기능을 통한 쓰레기 제거

### 🧲 오브젝트 상호작용
- 마우스 우클릭으로 오브젝트 드래그 (쓰레기 제거 미션)
- NPC와의 거리 기반 대화 시스템
- 조건부 퀘스트와 연동된 대화 흐름(JSON 기반)

### 🎤 대화 및 시나리오
- JSON 파일을 기반으로 하는 대화 시스템
- 캐릭터별 표정 변경
- 대화 중 스페이스바로 진행 제어

---

## 📁 폴더 구조

---

## 🛠 기술 스택

| 범주     | 기술명 |
|----------|--------|
| Engine   | Unity 6 |
| Language | C# |
| 시스템 구조 | 싱글톤 기반 매니저 시스템 (Managers.cs) |
| 시나리오 처리 | JSON + `DialogueManager` |
| 오디오 관리 | `SoundManager.cs`로 BGM/효과음 제어 |
| 메모리 관리 | 씬 전환 시 리소스 정리 및 초기화 지원 |

---

## 🔧 실행 방법

1. Unity 6 버전으로 클론한 프로젝트 열기
2. `Scenes/Play/Title.unity` 실행
3. `Play` 버튼 클릭 후 키보드 마우스를 이용해 플레이

> F 키: 초음파 공격 <br/>
> 마우스 우클릭: 쓰레기 아이템 드래그<br/>
> R 키: NPC 대화<br/>
> 스페이스바: 대화 넘기기

---

## 👥 팀 정보

| 이름     | 역할            | GitHub |
|----------|-----------------|--------|
| 이채원   |  기획 및 총괄    | [@leecw49](https://github.com/leecw49) |
| 이재용   |  메인 프로그래밍 | [@VirtualExplorer7613](https://github.com/VirtualExplorer7613) |
| 이현경   |  서브 프로그래밍 / UI / 맵 디자인 |  [@Ye0nan](https://github.com/Ye0nan) |
---

## 🔗 링크

- [🎬 시연 영상]() *(링크 추가 시 반영)*
- [📄 프로젝트 발표자료]() *(PDF 링크 추가 시 반영)*
- [📌 Notion 문서]() *(팀 위키, 기술문서 등)*

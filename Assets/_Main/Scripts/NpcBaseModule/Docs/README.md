# NpcBaseModule

## Назначение

Базовый модуль дружественных NPC и диалогов. Он обрабатывает взаимодействие игрока, состояние NPC, фокус камеры и отображение реплик.

## Основные части

- `IInteractable` (из `ComponentsModule`) задает контракт интерактивного объекта.
- Взаимодействие игрока с NPC обрабатывает общий `PlayerInteractionController` (PlayerModule).
- `FriendlyNpcController` управляет сценарием NPC.
- `FriendlyNpcSettings`, `NpcDialogueLine` и `NpcDialogueAnswer` описывают данные диалога.
- `NpcDialoguePresenter` связывает контроллер с `NpcDialogueView`.
- `DialogueCameraFocus` и `NpcWorldTextView` отвечают за сценовое представление.
- `FriendlyNpcInstaller` собирает зависимости NPC.

## Расширение

Новые типы дружественных NPC должны использовать общий контракт взаимодействия. Ветвление диалога держите в данных и контроллере, а view оставляйте пассивным.

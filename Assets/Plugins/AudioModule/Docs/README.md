# AudioModule: переиспользуемое ядро

## Назначение

Плагин предоставляет общее аудио-ядро проекта: сценовые аудио-события, банки, параметры, callbacks, triggers, музыку и звуки UI. Игровая интеграция настроек находится отдельно в `Assets/_Main/Scripts/AudioModule`.

## Основные части

- `IAudioSystem` и `AudioSystem` создают, запускают и останавливают аудио-события.
- `AudioBank` хранит события, параметры и callbacks одного банка.
- `AudioEventHandle` управляет созданным событием.
- Providers выбирают clip и числовые значения; behaviours изменяют pitch, volume и фильтры во времени.
- Actions позволяют вызвать callback, trigger или другое аудио-событие.
- `MusicPlayer`, `MusicPlaylist` и enumerators реализуют воспроизведение музыки.
- `UISoundCatalog` и `UISoundPlayer` реализуют звуки интерфейса.
- Скрипты в `Editor` генерируют API банков и UI-каталогов.

## Использование

Создайте `AudioBank`, заполните его событиями и сгенерируйте API из инспектора. Для runtime-доступа используйте `IAudioSystem` или `AudioSystem.Resolve`.

## Расширение

Новое поведение аудио-события оформляйте реализацией `IAudioEventBehaviour`, новое действие — реализацией `IAudioEventAction`. Код в `Codegen` вручную не редактируется.

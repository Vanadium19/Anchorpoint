# MenuModule

## Назначение

Модуль содержит навигацию главного меню и игровые экраны паузы и победы.

## Основные части

- `IMenuNavigationService` и `MenuNavigationService` переключают экраны главного меню.
- `MainMenuPresenter` связывает навигацию с `MainMenuView`.
- `GamePauseService` хранит причины паузы и применяет состояние игры.
- `GameplayPauseController` реагирует на ввод паузы.
- `PauseMenuPresenter` управляет `PauseMenuView`.
- `VictoryPresenter` и `VictorySaveDeleteController` показывают победу и очищают сохранение после завершения сценария.

## Подключение

Используйте `MainMenuInstaller`, `MenuNavigationInstaller` и `GameplayMenuInstaller` в соответствующих сценах и контекстах.

## Расширение

Новый экран меню добавляйте в конфигурацию навигации и view. Игровую логику не размещайте в UI-компонентах.

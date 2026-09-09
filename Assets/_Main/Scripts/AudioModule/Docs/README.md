# AudioModule: интеграция приложения

## Назначение

Этот слой связывает игровой проект с переиспользуемым аудио-ядром из `Assets/Plugins/AudioModule`. Он собирается в собственную сборку `AudioModule.Game`, которая ссылается на ядро, `BaseModule`, `SaveModule`, `InputModule` и Zenject; само ядро об этих модулях не знает. Он управляет громкостью музыки и SFX, экраном аудио-настроек, сохранением значений и запуском фонового события главного меню.

## Основные части

- `AudioMixerSettingsService` хранит нормализованную громкость каналов и применяет ее к `AudioMixer`.
- `AudioSettingsSaveService` регистрируется в `SaveModule` и сохраняет настройки при изменении громкости.
- `SettingsMenuPresenter` синхронизирует сервис с `SettingsMenuView`.
- `MainMenuAudioPresenter` запускает `MenuBankAPI.AmbientEvent`.
- `AudioSettingsInstaller`, `AudioSettingsMenuInstaller`, `AudioSystemInstaller` и `MainMenuAudioInstaller` подключают сервисы в нужных контекстах Zenject.

## Настройка

1. Создайте `AudioSettingsConfig` и укажите mixer-параметры музыки и SFX.
2. Добавьте `AudioSettingsInstaller` в общий контекст приложения.
3. На сцене меню подключите `AudioSettingsMenuInstaller` и заполните ссылки `SettingsMenuView`.
4. Для фонового звука меню добавьте `MainMenuAudioInstaller` и убедитесь, что аудио-банк с событием `Menu.Ambient` зарегистрирован.
5. Для геймплейной сцены добавьте `AudioSystem`, укажите его `initialBanks` и подключите `AudioSystemInstaller` к её `SceneContext`, чтобы модули получали `IAudioSystem` через DI.

## Расширение

Новый канал громкости нужно добавить в `AudioMixerChannel`, `AudioSettingsConfig`, снимок `AudioSettingsData`, сервис и UI. Сгенерированные файлы в `Codegen` вручную не редактируются.

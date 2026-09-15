# VFXModule

## Назначение

Модуль запускает визуальные эффекты и повторно использует их экземпляры через простой пул.

## Основные части

- `IEffectsService` предоставляет метод `Fire`, принимает опциональный масштаб эффекта и возвращает `IEffectHandle`, чтобы владелец эффекта мог остановить его.
- `EffectsService` берет эффект из очереди или создает новый, запускает его и возвращает в пул после завершения.
- `EffectsCatalog` связывает `EffectId` с prefab партикла.
- `EffectView` оборачивает `ParticleSystem`, останавливает эмиссию по handle и сообщает о завершении.
- `ScreenEffectsCatalog` связывает `ScreenEffectId` с prefab `Volume` — отдельный каталог для полноэкранных эффектов, по той же схеме, что и `EffectsCatalog`/`EffectId` для партиклов.
- `ScreenEffectFogSettings` — опциональный компонент на самом prefab'е `Volume`: несёт целевые `fogColor`/`fogDensity` для этого конкретного эффекта, чтобы туман был данными эффекта, а не настройкой общего инсталлера. Нет компонента — нет подмешивания тумана.
- `IRadiationScreenEffect`/`RadiationScreenEffect` — полноэкранный тинт и туман поверх сцены: на первый вызов с интенсивностью выше 0 берёт префаб из `ScreenEffectsCatalog` по ключу `ScreenEffectId.Radiation` и создаёт экземпляр (как `IEffectsService` создаёт партиклы — `Volume` не лежит в сцене изначально), дальше управляет его весом от 0 до 1 и, если на префабе есть `ScreenEffectFogSettings`, подмешивает его `fogColor`/`fogDensity` в `RenderSettings.fog*`, восстанавливая исходные настройки тумана сцены (снятые при создании) на интенсивности 0; отсутствующий каталог или запись в нём — валидный no-op для тинта.
- `VfxServiceInstaller` регистрирует оба каталога, сервис эффектов и `RadiationScreenEffect`; сам не знает деталей ни одного конкретного эффекта.

## Подключение

Создайте `EffectsCatalog`, заполните prefab для каждого `EffectId` и укажите контейнер эффектов в `VfxServiceInstaller`. Для полноэкранных эффектов сделайте `ScreenEffectsCatalog`, добавьте в него запись `ScreenEffectId.Radiation` → prefab с `Volume` (профиль с `Vignette`/`Color Adjustments`/`Chromatic Aberration` и т.п., вес 0) и укажите каталог в поле `screenEffectsCatalog`; туман настраивается прямо на этом prefab'е компонентом `ScreenEffectFogSettings`.

## Пауза

`EffectView` подписан на `PauseState` и на паузе ставит партиклы на `Pause`, а на возобновлении возвращает их в `Play`. Эффект, поставленный на паузу, не досчитывает время жизни и не возвращается в пул раньше срока.

## Расширение

Новый эффект добавляйте в `EffectId` и каталог. Для долгоживущего эффекта храните возвращённый `IEffectHandle` и вызывайте `Stop`, когда сценарий заканчивается. Новый полноэкранный эффект — в `ScreenEffectId` и `ScreenEffectsCatalog`.

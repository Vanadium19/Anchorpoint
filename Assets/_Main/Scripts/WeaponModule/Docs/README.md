# WeaponModule

## Назначение

Модуль реализует оружие игрока: модель боезапаса, создание оружия, стрельбу, перезарядку, прицеливание, переключение и процедурную отдачу; анимации рук и оружия клиповые.

## Основные части

- `WeaponModel` хранит магазин и резерв патронов.
- `WeaponController` реализует `IWeapon` и связывает модель с представлением.
- `WeaponInventory` переключает оружие и публикует `CurrentWeaponChanged`.
- `WeaponFactory` создает контроллеры через Zenject.
- `AmmoReserveService` сохраняет боезапас между переключениями.
- `WeaponView` управляет аниматорами рук и оружия, процедурной отдачей и рассылает состояние оружия частям представления (звук, хват, патроны).
- `RecoilProcessor` и `SwayProcessor` рассчитывают процедурную отдачу и раскачивание.
- `AmmoPresenter` обновляет `AmmoView`.

## Конфигурация

`WeaponConfig` содержит характеристики выстрела, магазина, перезарядки, прицеливания и процедурных эффектов.

## Типы патронов

`WeaponConfig.AmmoType` (`Rifle`, `Pistol`, `Shotgun`) задает тип боезапаса оружия. Предметы патронов используют `AmmoEffectDataSo` с совпадающим типом: патроны попадают в текущее оружие этого типа, а если оно не в руках, то в экипированное в слотах. `IWeaponInventory.CanAcceptAmmo` и `TryAddReserveAmmo` проверяют и добавляют резерв.

## Режимы стрельбы

- `IsAutomatic` — автоматический огонь при удержании или одиночный по нажатию.
- `ProjectilesPerShot` — число снарядов за выстрел (дробовик). Дробовик стреляет prefab `Pellet` — уменьшенная пуля с коротким сужающимся трассером и уменьшенным эффектом попадания (`hitEffectScale`).
- `SpreadAngle` — угол разброса в градусах для каждого снаряда.
- `FireInputBuffer` — сколько секунд помнится нажатие спуска, сделанное во время перезарядки затвора (`FireRate`); выстрел происходит, как только оружие готово. Быстрые клики короче кадра учитываются через `IsFirePressed`.

## Режимы перезарядки

- `Magazine` — полная перезарядка магазина за `ReloadTime`.
- `SingleRound` — по одному патрону за `RoundReloadTime`; выстрел при непустом магазине прерывает перезарядку.

## Части представления

Механизмы оружия (хват рук, видимые патроны, движущиеся детали, эффекты) — компоненты-наследники `WeaponViewPart` на prefab оружия. `WeaponView` не знает о конкретных частях: он рассылает всем частям prefab (находит их сам при первой рассылке) изменения состояния оружия — `Initialize`, `OnTriggerHeld`, `OnFired`, `OnActionCycleStarted`, `OnReloadStarted`, `OnReloadStateChanged`, `OnRoundInserted`, `OnLoadedRoundsChanged`, `OnHolsterStateChanged`, `OnPaused`, `OnAnimationEvent`, `ResetVisuals`. Часть переопределяет только нужные обработчики. Текущие части: `WeaponGripView`, `ChamberRoundsView`, `WeaponAudioView`, `MuzzleFlashView` (вспышка выстрела: prefab из WarFX создается у `muzzle` на каждый выстрел и живет `flashLifetime`).

Единая схема для звуков, анимаций и эффектов: каждый момент оружия — это id события (`WeaponEventIds`). Состояния поднимает `WeaponView` (`Fired`, `ReloadStarted`, `EmptyReloadStarted`, `RoundInserted`), моменты внутри клипа — Animation Event `RaiseEvent` с произвольным id. На id реагируют части. Аниматорами управляет только `WeaponView`, звуки играет только `WeaponAudioView`, эффекты и механизмы — отдельные части. Подход один для всех оружий, различается только содержимое prefab (какие части и какие привязки звуков).

## Анимации механизмов

Все движения рук и оружия — обычные `AnimationClip` в аниматорах `arms` (руки) и `gun` (оружие). Базовые контроллеры `acarms`/`acgun` общие; у револьвера и дробовика свои `AnimatorOverrideController` в `Assets/_Main/Animations/Revolver` и `Assets/_Main/Animations/Shotgun`, которые подменяют клипы АК и плейсхолдеры (пустые клипы в `arms/` и `gun/`, например `RoundReloadArms`, `ActionCycleGun`).

- Перезарядка магазином — триггеры `Reload`/`EmptyReload` (`EmptyReload` при `UseEmptyReloadAnimation` и пустом магазине). У револьвера `EmptyReload` — вариант без выброса гильз.
- Перезарядка по патрону — bool `IsRoundReloading`: состояние `RoundReload` с зацикленным клипом длиной `RoundReloadTime` (один патрон за цикл), выход с blend 0.2 с.
- Ход механизма после выстрела — состояние `Cycle` в слое `Action` обоих аниматоров (`WeaponView.PlayActionCycle`): у дробовика помпа (у рук слой аддитивный), у револьвера поворот барабана на одну камору. Клип барабана в конце вызывает событие `CompleteActionCycle` (id `ActionCycleCompleted`), барабан возвращается в исходный поворот, а `ChamberRoundsView` заново показывает патроны.
- Спуск — слой `TriggerPress` по bool `TriggerHold` у рук и у оружия; `TriggerPress → TriggerOut` ждет конца клипа, поэтому короткий клик доигрывает нажатие.
- `ChamberRoundsView` (револьвер) показывает патроны в каморах: массив `rounds` упорядочен от каморы под стволом в сторону вращения, `casings` — гильзы тех же камор, их видимость копируется с патронов в начале перезарядки (`OnReloadStarted`). Во время перезарядки патрон каморы прячется в том же кадре, когда аниматор показывает его гильзу (меши совпадают, вместе они мерцают), новое количество показывается по событию клипа `LoadRounds` (id `RoundsLoaded`, момент, когда спидлоадер отдает патроны) или по окончании/отмене перезарядки. Id событий задаются в инспекторе (`roundsAdvancedEventId`, `roundsLoadedEventId`), поэтому компонент подходит и для других видимых патронов (верх магазина, трубчатый магазин).
- `WeaponAnimationEvents` на объекте `gun` принимает Animation Event клипов `RaiseEvent(eventId)` — единственный вид события клипа. `WeaponView` передает id всем частям (`WeaponViewPart.OnAnimationEvent`). Id `ActionCycleCompleted` (клип барабана в конце) и `RoundsLoaded` (спидлоадер отдал патроны) — константы `WeaponEventIds`; остальные id (`CylinderOpen`, `Pump` и т.д.) произвольные и связываются со звуками в prefab.

Клипы револьвера и дробовика запечены из прежней процедурной анимации (IK рук к оружию уже внутри клипов). Длина перезарядки подгоняется под конфиг скоростью состояний `Reload`/`EmptyReload`/`RoundReload` (параметр `ReloadSpeed` в `acarms`/`acgun`): `WeaponView.PlayReload` ставит `длина клипа / ReloadTime` (или `RoundReloadTime`). Клипы рук для расчета задаются в `WeaponView` (`reloadClip`, `emptyReloadClip`, `roundReloadClip`); клип оружия той же длины. Без клипа скорость 1. Поэтому при смене времени в конфиге клипы править не нужно, а клип АК (2.97 с) растягивается под `ReloadTime` = 2 с.

## Хват рук

Движения рук и оружия задают клипы, а положение кистей на оружии — процедурный слой `WeaponGripView` поверх аниматора (гибрид). Подход одинаковый для всех оружий.

- `WeaponGripConfig` (`Assets/_Main/Configs/Weapons/Grips`) хранит для каждой руки (`HandGripData`) точку хвата и поворот кисти относительно якоря на оружии, повороты 15 костей пальцев, а для правой руки — позу указательного при нажатом спуске. `IsReleasedDuringReload` отдает руку клипу на время перезарядки.
- `WeaponGripView` на корне prefab оружия в `LateUpdate` тянет руку к точке хвата (`TwoBoneArmSolver`, изгиб локтя берется из клипа) и ставит пальцы. `HandRig` — кости руки и якорь: у правой руки это модель оружия (`RevolverModel`, `ShotgunModel`) или `gun_root_bone` (AK), у левой — деталь, за которую держится рука (`Pump` у дробовика).
- Нажатие спуска указательным процедурное (`triggerPressDuration`/`triggerReleaseDuration`, короткий клик доводит нажатие до конца); слой `TriggerPress` аниматора рук для указательного перекрывается хватом, спуск оружия по-прежнему анимирует аниматор `gun`.
- `WeaponGripView` — часть представления (`WeaponViewPart`): реагирует на спуск, перезарядку, паузу и сброс.

Как поправить хват: в Play Mode выбрать оружие, в инспекторе `WeaponGripView` → Pose Editing → `Begin Pose Editing` (аниматоры рук и оружия замораживаются), поставить кисть и пальцы, нажать `Capture Right Hand Grip` / `Capture Left Hand Grip` (для нажатого спуска — `Capture Trigger Press Pose`), затем `End Pose Editing`. Поза пишется в ассет конфига и сохраняется после выхода из Play Mode. Для нового оружия: добавить `WeaponGripView`, `Assign Bones`, задать якоря и конфиг, снять позы.

- Руки в prefab оружия используют меш `Assets/_Main/Models/Arms/arms.asset` — копию меша из `arms2.fbx`, у которой сечение фаланг `_02`/`_03` уменьшено на 3%/6%, чтобы грани соседних фаланг не совпадали (z-fighting на прямых пальцах).

## Звук

Звуки оружия проигрываются через `IAudioSystem` из `AudioModule`; в prefab задаются только идентификаторы событий банка `Weapons` (`WeaponsBankAPI`), собственных `AudioSource` у оружия нет.

- `WeaponAudioView` — единственное место звуков оружия, на корне prefab. Массив `sounds` связывает id события (`WeaponEventIds` или id из клипа) со звуком банка (`AudioEventKey`, id проверяется списком банка). Выстрел (`Fired`) ограничен по голосам: минимальный интервал `fireAudioMinInterval`, не больше `fireAudioMaxVoices` одновременно, самый старый гасится за `fireAudioStealFadeOut`. Если для `EmptyReloadStarted` звука нет, при перезарядке пустого магазина играет `ReloadStarted`.
- Звук перезарядки и последний звук из клипа хранят `AudioEventHandle` и затухают за `mechanismAudioFadeOut`, когда перезарядка завершена, прервана или оружие сброшено.
- `Bullet`: звук попадания с ограничением частоты; у дроби эффект попадания показывает каждая дробина, а звук — только одна (`SetHitSoundEnabled`). Пуля игнорирует коллайдеры стрелка, поэтому точка вылета может находиться внутри капсулы игрока (короткий ствол револьвера).

`AudioEventHandle.Dispose(fadeout)` в ядре AudioModule гасит событие по id, а не конкретный экземпляр; для отдельного голоса используйте `Stop(fadeout)`, а после затухания — `Dispose()`.

Банк `Weapons` должен быть в `initialBanks` `AudioSystem` на сцене, где есть оружие (сейчас `Camp` и `Game`).

## Точность

`HipInaccuracy` и `AimInaccuracy` в `WeaponConfig` — случайное отклонение всего выстрела в градусах от бедра и при прицеливании. Отклонение выбирается один раз на выстрел, затем к каждому снаряду добавляется `SpreadAngle` (у дробовика дробины разлетаются вокруг отклоненного направления).

## Модификации

Модификации ставятся на конкретный экземпляр оружия (`ItemTable`), а не на тип: id установленных модификаций хранятся в `WeaponModificationMetadata` предмета и сохраняются вместе с ним (`ItemMemento.WeaponModificationIDs`). Потеря оружия означает потерю его модификаций.

- `WeaponModificationConfig` (`Assets/_Main/Configs/Weapons/Modifications`) — модификация и ее эффект: `DamageMultiplier`, `MagazineCapacityBonus`, `AimInaccuracyMultiplier` (влияет только на прицеливание). `ID` — ключ сохранения, после релиза не менять. `Slot` (`Sight`, `Magazine`, `Barrel`) — на оружии не больше одной модификации на слот.
- `WeaponItemSo.AvailableModifications` — какие модификации принимает оружие.
- `WeaponStats` — итоговые характеристики: значения `WeaponConfig` с примененными модификациями. `WeaponController` берет урон, емкость магазина и отклонение из них, остальное — из `WeaponConfig`.
- `IWeaponModificationService` — чтение, проверка и установка модификаций, расчет `WeaponStats`, событие `ModificationsChanged`. Привязывается `WeaponModificationInstaller` в `SceneContext` (Camp и Game), чтобы сервис был общим для оружия игрока (`GameObjectContext`) и станций на сцене. `WeaponInventory` по событию пересчитывает характеристики уже созданного оружия: при уменьшении магазина лишние патроны уходят в резерв.
- `WeaponAttachmentsView` — часть представления на prefab оружия: для каждой модификации задает показываемые объекты, скрываемые штатные детали и `aimPositionOffset`, на который сдвигается `AimPosition` при прицеливании (взгляд через коллиматор). Модели модификаций лежат под костями `gun_root_bone`/`mag_bone`, поэтому следуют клипам.

- `CollimatorReticleView` — часть на объекте коллиматора: перекрестие (`Reticle`, отдельный меш `Models/Weapons/Attachments/RedDotSight_Reticle`) перед рендером камеры ставится в окне на линию от глаза к далекой точке по оси прицела (`SightAxis`, направлена вдоль ствола). Поэтому точка, как у настоящего коллиматора, показывает направление ствола и не уезжает с мушки при отдаче и раскачивании; при выходе глаза за `maxOffset` точку скрывает корпус. `aimPositionOffset` коллиматора подбирается так, чтобы глаз при прицеливании лежал на оси прицела.

Покупка модификаций за ресурсы — в `ArsenalModule`.

## Расширение

Новая модификация: `WeaponModificationConfig`, добавить ее в `AvailableModifications` нужных `WeaponItemSo`, модель и запись в `WeaponAttachmentsView` prefab оружия, цену в `ArsenalConfig`.

Новое оружие обычно добавляется новым `WeaponConfig` и prefab `WeaponView`. Общий ввод остается в `PlayerModule`, а контракт снаряда задается в `ComponentsModule`.

Расширение анимаций без изменения существующих классов:

- Новый механизм (затвор, магазин, лазер, счетчик) — новый наследник `WeaponViewPart` на prefab оружия; `WeaponView` подхватит его сам (массив `parts` обновляется при сохранении prefab).
- Новый момент в клипе — Animation Event с функцией `RaiseEvent` и строковым id; часть сравнивает id в `OnAnimationEvent`. Id лучше выносить в сериализуемое поле части, как в `ChamberRoundsView`.
- Новые клипы — через `AnimatorOverrideController` поверх `acarms`/`acgun`, хват рук — через новый `WeaponGripConfig`.

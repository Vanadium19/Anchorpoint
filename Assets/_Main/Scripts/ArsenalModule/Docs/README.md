# ArsenalModule

## Назначение

Модуль отвечает за лагерное здание «Арсенал»: стенд, на котором игрок за ресурс «Электроника»
устанавливает модификации на принадлежащее ему оружие. Модификации ставятся на конкретный
экземпляр оружия (`ItemTable`) через `WeaponModule.IWeaponModificationService`.

## Основные части

- `IExternalUI` (из `InventoryModule`) задает контракт мирового объекта, чей экран открывает
  общий `PlayerInteractionController` (PlayerModule) через `ExternalUIManager`.
- `ArsenalStationView` — компонент на префабе здания: реализует `IExternalUI` и `IArsenalStation`,
  логики установки не содержит.
- `IArsenalService` и `ArsenalService` собирают оружие игрока (экипированное и из инвентаря,
  включая вложенные контейнеры), проверяют, хватает ли ресурсов, списывают цену и вызывают установку
  модификации. `ArsenalService` является `ExternalUIHandler` для всех объектов с `IArsenalStation`.
- `ArsenalScreenView`, `ArsenalWeaponSlotView`, `ArsenalModificationSlotView`, `ArsenalCostSlotView` —
  пассивный экран: список оружия, список модификаций выбранного оружия и строки стоимости.
- `ArsenalPresenter` хранит выбранное оружие, связывает `ArsenalScreenView` с `IArsenalService`
  и резолвит локализованный текст.
- `ArsenalInstaller` регистрирует `ArsenalService`.

## Данные

- `ArsenalCost` — предмет (`ItemDataSo`) и его количество.
- `ArsenalModificationPrice` — модификация (`WeaponModificationConfig`) и список `ArsenalCost`.
- `ArsenalConfig` — цены всех модификаций одного арсенала и `TitleKey` заголовка экрана.
  Модификация без записи в `ArsenalConfig` в арсенале не продается.
- `Arsenal*DisplayData` — подготовленные презентером данные для отрисовки.

Количество «Электроники» для каждой модификации задает геймдизайнер в `ArsenalConfig`.

Ассеты:

- цены арсенала — `Assets/_Main/Configs/Arsenal/ArsenalModificationsConfig.asset`;
- постройка — `Assets/_Main/Configs/Constructing/Items/ArsenalConfig.asset` (`BuildingConfig`, цена постройки)
  и `Assets/_Main/Prefabs/Buildings/Arsenal.prefab`;
- экран — `Assets/_Main/Prefabs/UI/Arsenal/`;
- ресурс — `Assets/_Main/Configs/Inventory/Items/Electronics/ElectronicsData.asset`, выпадает из
  `Configs/LootSpawn/Resources.asset`.

Для тестов арсенал уже построен в `Assets/StreamingAssets/Saves/GameSaveWithInfinityResources.json`
(там же 10 «Электроники» в рюкзаке): скопируйте файл в `Application.persistentDataPath/Saves/GameSave.json`.

## Подключение

`ArsenalInstaller` устанавливается в `SceneContext` сцены лагеря рядом с `WeaponModificationInstaller`
и `InventoryInstaller`: сервису нужны `IInventoryManager`, `IEquipmentSlotService`,
`IWeaponModificationService`, `ExternalUIManager` и `Canvas`.

Префаб здания должен содержать `BuildingView` и `ArsenalStationView`; в `ArsenalStationView`
назначаются `ArsenalConfig` и префаб экрана с компонентом `ArsenalScreenView`.
Дальнейшее взаимодействие игрока со зданием не требует изменений в `PlayerModule`.

## Расширение

Новые цены добавляются в `ArsenalConfig` без изменения кода. Сами модификации — эффекты, совместимость
с оружием (`WeaponItemSo.AvailableModifications`) и внешний вид — живут в `WeaponModule`; модуль
арсенала только продает уже существующие модификации. Игровой код должен зависеть от
`IArsenalService`, а не от конкретной реализации.

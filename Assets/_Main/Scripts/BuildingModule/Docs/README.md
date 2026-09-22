# BuildingModule

## Назначение

Модуль реализует строительство лагеря: каталог построек, меню выбора, режим размещения, превью коллизий, оплату ресурсами, регистрацию созданных объектов и сохранение состояния.

## Основные части

- `BuildingCatalog` и `BuildingConfig` описывают доступные постройки, категории, цену и очки базы.
- `ConstructionModeService`, `PlacementService` и `PlacementController` управляют выбором и размещением.
- `PreviewService` создает визуальное превью и проверяет коллизии.
- `StorageService` списывает ресурсы через инвентарь.
- `BuildingRegistry` хранит построенные объекты.
- `IBuildingDamageService` и `BuildingDamageService` хранят здоровье построек, принимают урон и сообщают о поломке.
- `BuildingStructureTargetSource` и `BuildingStructureTarget` показывают целые постройки как цели для атакующих.
- `BuildingSaveService` создает и восстанавливает снимки построек и контейнеров.
- `BuildingMenuPresenter`, `BuildingPricePresenter` и `BaseLevelPreviewBridge` связывают логику с UI.
- `BuildingUpgradeService` хранит уровни построек, проверяет цену следующего уровня и меняет визуал через `BuildingView`.
- `BuildingUpgradePresenter` открывает MVP-панель улучшения по интеракции здания или после открытия его внешнего UI.

## Подключение

`BuildingModuleInstaller` регистрирует сервисы и принимает сценовые ссылки, каталоги и конфиги. Для объектов с хранилищем prefab должен предоставлять `IContainerUI`.
`BuildingView.boundsMeshFilter` должен ссылаться на визуальный `MeshFilter` постройки: его bounds используют эффекты, которым нужны габариты здания.

## Улучшения построек

`BuildingUpgradeConfig` содержит ровно три уровня. Первый уровень может оставить `visualPrefab` пустым и использовать исходный визуал постройки; у второго и третьего задаются `visualPrefab` и `Price`. Конфиг назначается в `BuildingConfig`.

Чтобы смена визуала не затронула компоненты здания, `BuildingView.initialVisual` указывает на исходную модель, а `upgradeVisualContainer` — на дочерний контейнер для визуальных prefab. Улучшенные визуалы содержат только визуальную иерархию. Это позволяет добавлять цепочки конкретных ассетов без изменения runtime-кода.

Для построек без `IExternalUI` `BuildingView` принимает обычную интеракцию и передает ее presenter. Если на том же объекте есть `IExternalUI` (верстак или сундук), приоритет остается у существующего окна; `BuildingUpgradePresenter` показывает панель после `ExternalUIManager.UIOpened` и не заменяет внешний UI.

`BuildingSnapshot.UpgradeLevel` сохраняет уровень. Отсутствующее поле старого снимка имеет значение `0` и восстанавливается как первый уровень.

## Расширение

Новую постройку добавляйте через `BuildingConfig` и `BuildingCatalog`. При изменении сохраняемого состояния обновляйте `BuildingSnapshot` и memento-классы совместно.

## Урон постройкам

Постройки как цели атаки: `BuildingStructureTargetSource` реализует `IStructureTargetSource` из `ComponentsModule` и отдает ближайшую целую постройку, а `BuildingStructureTarget` наносит ей урон через `IBuildingDamageService`. Кто именно атакует, модуль не знает.

`IBuildingDamageService` — общий API состояния постройки: `ApplyDamage` снимает здоровье, `IsBroken` и `TryGetHealth` читают текущее состояние, `Restore` возвращает полное здоровье. Максимум здоровья берётся из `BuildingConfig.MaxHealth`, здоровье создаётся при первом обращении и живёт до конца сцены — в сохранение не попадает.

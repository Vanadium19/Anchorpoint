# BuildingModule

## Назначение

Модуль реализует строительство лагеря: каталог построек, меню выбора, режим размещения, превью коллизий, оплату ресурсами, регистрацию созданных объектов и сохранение состояния.

## Основные части

- `BuildingCatalog` и `BuildingConfig` описывают доступные постройки, категории, цену и очки базы.
- `ConstructionModeService`, `PlacementService` и `PlacementController` управляют выбором и размещением.
- `PreviewService` создает визуальное превью и проверяет коллизии.
- `StorageService` списывает ресурсы через инвентарь.
- `BuildingRegistry` хранит построенные объекты.
- `BuildingModel` хранит здоровье, стадию стройки и поломку. `BuildingController` владеет моделью и обновляет вид. `BuildingLifecycleService` тикает стройку.
- `BuildingStructureTargetSource` и `BuildingStructureTarget` показывают целые постройки как цели для атакующих.
- `BuildingSaveService` создает и восстанавливает снимки построек и контейнеров.
- `BuildingMenuPresenter`, `BuildingPricePresenter` и `BaseLevelPreviewBridge` связывают логику с UI.

## Подключение

`BuildingModuleInstaller` регистрирует сервисы и принимает сценовые ссылки, каталоги и конфиги. Для объектов с хранилищем prefab должен предоставлять `IContainerUI`.
`BuildingView.boundsMeshFilter` должен ссылаться на визуальный `MeshFilter` постройки: его bounds используют эффекты, которым нужны габариты здания.

## Расширение

Новую постройку добавляйте через `BuildingConfig` и `BuildingCatalog`. При изменении сохраняемого состояния обновляйте `BuildingSnapshot` и memento-классы совместно.

## Урон постройкам

Постройки как цели атаки: `BuildingStructureTargetSource` реализует `IStructureTargetSource` из `ComponentsModule` и отдает ближайшую целую постройку, а `BuildingStructureTarget` наносит ей урон через `BuildingModel.TakeDamage`. Кто именно атакует, модуль не знает.

Здоровье живёт в `BuildingModel`: максимум берётся из `BuildingConfig.MaxHealth`, ноль здоровья переводит постройку в `BuildingState.Broken` и меняет её вид. То же состояние пишется в `BuildingSnapshot`.

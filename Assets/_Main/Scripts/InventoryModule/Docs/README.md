# InventoryModule

## Назначение

Модуль реализует сеточный инвентарь, экипировку, контейнеры, контекстное меню предметов, выпадение добычи и сериализацию состояния.

## Основные части

- `ItemTable`, `GridTable`, `InventoryMetadata` и `ContainerMetadata` представляют модель инвентаря.
- `CharacterInventory` хранит инвентарь персонажа.
- `GridService`, `EquipmentSlotService`, `DropService` и `ContainerWindowService` выполняют операции над предметами.
- `ContextActionService` формирует доступные действия: использование, экипировку, открытие и выбрасывание.
- `InventoryManager` связывает модель, сервисы и UI.
- `InventorySaveable` и `ItemSerializer` сохраняют состояние.
- `DeathLootStorage` и `DeathLootSpawner` переносят предметы погибшего персонажа в мир.

## Подбор оружия

`LootPickupService` сначала экипирует предмет в свободный слот. Если подходящие оружейные слоты заняты, оружие из активного слота (`IEquipmentSlotService.ActiveSlot`, его выставляет `WeaponModule`) или первого совместимого выбрасывается на место подобранного, а новое экипируется. Только если замена невозможна, предмет кладется в инвентарь.

## Данные

`ItemDataSo` описывает предмет, размеры, prefab, доступность выбрасывания и связанные эффекты. `ItemCatalog` используется при восстановлении сохранения.

## Модификации оружия

`WeaponModificationMetadata` — метаданные `ItemTable` с идентификаторами установленных на оружие модификаций (`ModificationIds`, строки). Модуль не зависит от `WeaponModule`: что означает id, определяет `WeaponModule`, а запись выполняет, например, `ArsenalModule`. Метаданные создаются через `ItemTable.InitializeWeaponModifications()`, дубликат id не добавляется (`AddModification` возвращает `false`). Состояние сохраняется как `ItemMemento.ModificationIds`; `ItemSerializer` пишет и читает список, в старых сохранениях он пустой или отсутствует и загружается без ошибок.

## Расширение

Новое действие контекстного меню оформляйте парой config/action и добавляйте в preset. Новые поля сохраняемого предмета отражайте в memento и сериализаторе.

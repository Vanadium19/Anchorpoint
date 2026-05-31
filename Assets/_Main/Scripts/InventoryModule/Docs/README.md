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

## Данные

`ItemDataSo` описывает предмет, размеры, prefab, доступность выбрасывания и связанные эффекты. `ItemCatalog` используется при восстановлении сохранения.

## Расширение

Новое действие контекстного меню оформляйте парой config/action и добавляйте в preset. Новые поля сохраняемого предмета отражайте в memento и сериализаторе.

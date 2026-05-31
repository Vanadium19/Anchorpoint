# SpawnModule

## Назначение

Модуль создает противников и стартовую добычу на сцене по настроенным точкам появления.

## Основные части

- `EnemySpawnController` создает волны противников по `SpawnConfig`.
- `EnemyFactory` инстанцирует prefab противника через Zenject.
- `LootSpawnController` создает стартовую добычу.
- `LootFactory` создает одиночные и сложенные мировые предметы.
- `LevelSpawnPointsView` и `LevelLootSpawnPointsView` содержат сценовые точки.
- `LootSpawnPointConfig` и `LootEntry` задают таблицу и диапазоны добычи.

## Подключение

Добавьте `SpawnInstaller` и `LootSpawnInstaller` на сцену, заполните prefab противника, настройки таймингов и списки точек.

## Расширение

Для новых правил появления расширяйте конфиги и контроллеры, сохраняя фабрики простыми. Данные предметов берите из `InventoryModule`.

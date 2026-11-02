# ArsenalModule

## Назначение

Модуль отвечает за Арсенал: отдельный строящийся стенд лагеря, на котором игрок за ресурсы
(Electronics) устанавливает на оружие модификации. Модификация постоянна: ее нельзя снять,
возврата ресурсов нет. На один слот одного экземпляра оружия приходится одна модификация.

## Основные части

- `IExternalUI` (из `InventoryModule`) задает контракт мирового объекта, чей экран открывает
  общий `PlayerInteractionController` (PlayerModule) через `ExternalUIManager`.
- `ArsenalView` — компонент на префабе здания Арсенала: реализует `IExternalUI`, логики не содержит.
- `IArsenalService` и `ArsenalService` ищут оружие игрока (`GetWeapons`), считают сборку (`GetBuild`),
  количество ресурса (`GetOwnedCount`), проверяют возможность установки (`GetInstallStatus`,
  `ArsenalInstallStatus`) и выполняют ее (`TryInstall`). `ArsenalService` также наследует
  `ExternalUIHandler` и создает экран для каждого `ArsenalView`. Это единственный источник правды о том, можно ли
  установить модификацию.
- `ArsenalScreenView`, `ArsenalWeaponRowView`, `ArsenalModificationRowView`, `ArsenalCostRowView`,
  `ArsenalStatRowView` — пассивный экран и его строки.
- `ArsenalPresenter` владеет выбором оружия и модификации, резолвит локализованный текст,
  строит превью характеристик через `WeaponBuild.With` и перерисовывает экран.
- `ArsenalInstaller` регистрирует `ArsenalService`.

## Данные

Модуль не вводит собственных конфигов. Источником данных служат типы `WeaponModule`:

- `WeaponItemSo.AvailableModifications` — список модификаций, доступных оружию; это и есть правило
  совместимости.
- `WeaponModificationConfig` — слот, модификаторы характеристик, цена (`WeaponModificationCost`),
  ключи названия и описания.

Для отображения используются классы `Arsenal*DisplayData` и `ArsenalScreenTexts`: презентер
подготавливает их, а экран только рисует.

## Установка модификации

`ArsenalService.TryInstall` атомарна и выполняет шаги по порядку:

1. `GetInstallStatus` должен вернуть `Available` (совместимость, дубликат, занятость слота, ресурсы;
   одинаковые предметы в цене суммируются).
2. `IInventoryManager.TryRemoveItems` списывает каждую цену.
3. Идентификатор модификации записывается в `ItemTable.WeaponModificationMetadata`.
4. `IWeaponInventory.RefreshBuild` применяет сборку к уже созданному контроллеру оружия.
5. Вызывается событие `BuildChanged`.

Статусы `GetInstallStatus` проверяются в порядке: `NotCompatible`, `AlreadyInstalled`, `SlotOccupied`,
`NotEnoughResources`, `Available`. Ограничение слота (одна модификация на слот) проверяется здесь, а не в
`WeaponModule`.

Ограничение: список оружия (`GetWeapons`) собирается из слотов экипировки, главной сетки и, рекурсивно,
из вложенных контейнеров. Дополнительные сетки, зарегистрированные в `IInventoryManager`
(например, сундуки), API менеджера наружу не отдает, поэтому оружие, лежащее только в них, в списке
не появится.

## Подключение

Добавьте `ArsenalView` на префаб здания Арсенала и назначьте ему префаб экрана с компонентом
`ArsenalScreenView`. Здание добавляется в меню строительства через `BuildingConfig` и `BuildingCatalog`.

`ArsenalInstaller` добавляется в сцене Camp в `GameObjectContext` объекта Player (рядом с
`WeaponInstaller`), а не в `SceneContext`. Причина: `IWeaponInventory` и `IWeaponStatsProvider`
регистрируются в этом дочернем контейнере, а `Canvas`, `ExternalUIManager` и инвентарь
(`IInventoryManager`, `IEquipmentSlotService`) приходят из родительского. Дочерний контейнер видит
родительский, обратное неверно, поэтому в `SceneContext` сервис не разрешил бы `IWeaponInventory`.

## Для дизайнера

- Цена модификации — `WeaponModificationConfig.costs` (предмет и количество) в
  `Assets/_Main/Configs/Weapons/Modifications`. Сейчас везде ресурс Electronics.
- Данные здания Арсенала (`BuildingConfig`: цена и время постройки, prefab, прочность, ремонт) — `Assets/_Main/Configs/Constructing/Items/ArsenalConfig.asset`; запись в `BuildingCatalog` обязательна.
- Ресурс Electronics — `Assets/_Main/Configs/Inventory/Items/Electronics/Electronics.asset`; его вес в
  добыче задается в `Assets/_Main/Configs/LootSpawn/Resources.asset`.
- Какие модификации доступны какому оружию — список `availableModifications` в ассете оружия
  (`WeaponItemSo`).

## Локализация

Все тексты берутся из таблицы `Localization` через `UtilsModule.LocalizedText`. Ключи с префиксом
`arsenal.` описывают заголовки экрана, статусы, слоты и названия характеристик. Названия и описания
модификаций задаются ключами `nameKey`/`descriptionKey` в `WeaponModificationConfig`.

## Расширение

Новая модификация добавляется ассетом `WeaponModificationConfig`, записью в
`WeaponItemSo.AvailableModifications`, при необходимости записью в `WeaponModificationView` на prefab
оружия и ключами локализации, без изменения кода. Новая характеристика требует значения
в `WeaponStat` и строки в `StatDefinitions` презентера плюс ключа локализации. Игровой код должен
зависеть от `IArsenalService`, а не от конкретной реализации.

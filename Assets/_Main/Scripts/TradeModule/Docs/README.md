# TradeModule

## Назначение

Модуль отвечает за встречу с торговцами: неуязвимых NPC, которые открывают экран обмена
ресурсов по конфигурируемым предложениям.

## Основные части

- `IExternalUI` (из `InventoryModule`) задает контракт мирового объекта, чей экран открывает
  общий `PlayerInteractionController` (PlayerModule) через `ExternalUIManager`.
- `TraderView` — компонент на префабе торговца: реализует `IExternalUI` и `ITraderOffersProvider`,
  логики обмена не содержит.
- `ITradeService` и `TradeService` проверяют и выполняют обмен, храня единственный источник
  правды о том, хватает ли игроку ресурсов.
- `TradeView` и `TradeOfferSlotView` — пассивный экран обмена и строка одного предложения.
- `TradePresenter` связывает `TradeView` с `ITradeService` и резолвит локализованный текст.
- `TradeInstaller` регистрирует `TradeService`.

Торговец не несет `IHealthComponent`/`IDamageable` — в проекте урон применяется только через
разрешенный `IDamageable`, поэтому объект без него не может получить урон. Это и делает
торговца неуязвимым.

## Данные

- `TradeItemAmount` — предмет (`ItemDataSo`) и его количество.
- `TradeOfferConfig` — одно предложение: списки `Requested`/`Offered` и `TitleKey` для заголовка.
- `TraderOffersConfig` — набор предложений одного торговца и `TraderNameKey` для его имени.

## Подключение

Добавьте `TraderView` на префаб торговца и назначьте ему `TraderOffersConfig` и префаб экрана
обмена с компонентом `TradeView`. `TradeInstaller` должен быть установлен в том же контейнере
Zenject, где доступны `InventoryModule.IInventoryManager` и `InventoryModule.ExternalUIManager`.
Дальнейшее взаимодействие игрока с торговцем не требует изменений в `PlayerModule`.

## Расширение

Новые предложения добавляются через `TradeOfferConfig` и `TraderOffersConfig` без изменения кода.
Игровой код должен зависеть от `ITradeService`, а не от конкретной реализации.

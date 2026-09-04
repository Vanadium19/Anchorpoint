# RandomEventsModule

## Назначение

Каркас случайных событий. Событие — это выбор внутри пула скоупа → прогон последовательности шагов-действий (гейт по условиям — тоже шаг) → завершение с кулдауном.

Ядро (`Runtime/Core`, `Runtime/Composition`, `Runtime/State`, `Runtime/Data`, `Runtime/Signals`) не знает ни одного конкретного события, условия или действия игры.

## Идентификация

- Событие адресуется ссылкой на ассет `RandomEventDefinition` — отдельного поля `id` нет.
- Ключ состояния и сигнала — ассет `RandomEventKey`; условие и источник сигнала ссылаются на один и тот же ассет. `PersistentId` — GUID ассета, записывается при валидации в редакторе и уходит в сейв, поэтому переименование/перемещение ассета сохранённое состояние не ломает.
- Единственные строки в модуле — ключи локализации (`displayNameKey`, `localizationKey`), их проверяет система локализации.

## Модель события

`RandomEventDefinition` — ScriptableObject, описывающий одно событие:

| Поле | Смысл |
| --- | --- |
| `weight` | Вес при взвешенном выборе внутри пула |
| `isEnabled` | Выключатель события |
| `isExclusive` | Пока активно, другие события не стартуют |
| `cooldownSeconds` | Пауза до следующего запуска этого же события |
| `actionSteps` | Плоский список шагов действия: по умолчанию каждый шаг идёт после предыдущего, `runInParallel` — вместе с предыдущим |
| `displayNameKey` | Ключ локализации имени для UI и отладки |

`actionSteps` — плоский список `RandomEventActionStep`, у каждого шага свой `[SerializeReference] action` и галка `runInParallel` (несколько подряд отмеченных шагов = один параллельный блок). `CreateActionGroups` собирает из этого списка последовательность групп для исполнения. Гейт по условиям — обычный шаг: `IRandomEventAction`, который проверяет `RandomEventConditionSet` и при провале возвращает из `ExecuteAsync` `false` (см. «Расширение → 2»).

Редактируется через кастомный инспектор `RandomEventDefinitionEditor` — реордерируемый список шагов с поиском по типу для `[SerializeReference]`-полей действий и условий.

Определение не знает, в каком оно скоупе или пуле — принадлежность решается списками `RandomEventPool.definitions`, один и тот же `RandomEventDefinition` можно вписать сразу в несколько пулов и скоупов.

## Конфигурация: сцена, скоуп

- **`RandomEventsRules`** — своё значение на каждую сцену (обычное поле инсталлера, не ассет): `evaluationIntervalSeconds`, `minIntervalBetweenEventsSeconds`.
- **`RandomEventScope`** — ассет на каждый скоуп (их может быть несколько в одной сцене), держит список пулов `pools: List<RandomEventPool>`.
- **`RandomEventPool`** — `[Serializable]`-элемент этого списка, а не отдельный ассет: свой `picker` и свой список `RandomEventDefinition`.

`StartRandom(scope)` перебирает `scope.Pools` независимо: у каждого пула, если есть подходящие кандидаты, выбирает событие его собственным `picker` и пытается стартовать. Одна попытка может запустить события сразу из нескольких пулов, если ни одно из уже активных не помечено `isExclusive`.

## Основные части

- `IRandomEventService`/`RandomEventService` — запуск (`Start`, `StartRandom`), остановка (`Stop`, `StopAll`), события `EventStarted`/`EventFinished`, контроль эксклюзивности, кулдаунов, минимального интервала между событиями.
- `IRandomEventTriggerRunner`/`RandomEventTriggerRunner` — периодическая проверка и приём внешних сигналов.
- `IRandomEventStateStore`/`RandomEventStateStore` — сохраняемое хранилище `int`/`float`/`bool` по ключам-ассетам `RandomEventKey`.
- `RandomEventSignalRelay` — база для адаптеров слоя оркестрации, превращающих факт игрового модуля в сигнал.
- `RandomEventsRules` — конфигурация уровня сцены (см. выше).
- `RandomEventDefinition`, `RandomEventActionStep`, `RandomEventScope`, `RandomEventPool`, `RandomEventKey` — данные событий.
- `IRandomEventAction.ExecuteAsync` возвращает `bool`: `false` — сигнал гейта остановить дальнейшую цепочку без ошибки (кулдаун события всё равно применяется, но общий `minIntervalBetweenEventsSeconds` не сдвигается).
- Слой композиции: `IRandomEventCondition`, `IRandomEventAction`, `IRandomEventPicker`, `RandomEventActionBase`, `IRandomEventConditionAsset`/`IRandomEventActionAsset`/`IRandomEventPickerAsset`, их Zenject-базы `ZenjectRandomEventConditionAsset<T>`/`ZenjectRandomEventActionAsset<T>`/`ZenjectRandomEventPickerAsset<T>`, `RandomEventConditionSet`/`ConditionEvaluator` — блок «условия + режим совпадения» для gate-действий.
- `WeightedRandomPicker`/`WeightedRandomPickerAsset` — дефолтный алгоритм выбора, используется автоматически, если `picker` в `RandomEventPool` не назначен. Кэшируется по одному экземпляру на пул в `RandomEventService`.
- Уведомления: `IRandomEventNotifier`/`RandomEventNotifier`, `RandomEventNotificationPresenter`, `RandomEventNotificationView` (MVP, очередь сообщений).
- Сохранение: `RandomEventsSaveable`, `RandomEventsSaveHandler`, `RandomEventsMemento`.

## Готовые условия и действия

`Runtime/Conditions` и `Runtime/Actions` — папки под условия и действия конкретного проекта; в самом модуле их нет. Пишутся по образцу из «Расширение → 2».

## Триггеры

`RandomEventTriggerRunner` тикает каждые `evaluationIntervalSeconds` и вызывает `StartRandom(scope)`; `0` отключает цикл. Пауза учитывается через `IPausable`/`IPauseManager`.

`IRandomEventTriggerRunner.ReportSignal(signalKey)` увеличивает счётчик в `IRandomEventStateStore` и сразу вызывает `Evaluate()`, независимо от того, включён ли периодический цикл. Игровые модули этот интерфейс не инжектят напрямую — связку делает релей в слое оркестрации (см. «Расширение → 3»).

Семантика проверки:

- Кандидатом на выбор событие становится по `isEnabled`, `weight > 0` и отсутствию кулдауна; условия с побочными эффектами проверяются позже, когда очередь доходит до gate-шага.
- Проверка внутри гейта короткозамкнутая: `All` останавливается на первом `false`, `Any` — на первом `true`.
- Глобальные блокировки (уже активно эксклюзивное событие, не выдержан `minIntervalBetweenEventsSeconds`) проверяются до перебора кандидатов.
- `ReportSignal` запускает проверку синхронно — не вызывайте его на высокочастотных фактах, копите их в релее и репортите агрегатом.

## Сохранение

Единственное сохраняемое состояние — `IRandomEventStateStore` (словари `int`/`float`/`bool`, ключ — `RandomEventKey.PersistentId`, то есть GUID ассета-ключа).

`RandomEventsSaveHandler` регистрирует `RandomEventsSaveable`; `Load()` вызывают сценовые хендлеры (`CampSaveHandler`/`GameSceneSaveHandler`), регистрация выполняется раньше загрузки за счёт `Container.BindExecutionOrder<RandomEventsSaveHandler>(-100)`.

`SaveKey` `"random_events"` менять нельзя. Ассет `RandomEventKey` не удалять и не пересоздавать — новый ассет получит новый GUID, и накопленное состояние осиротеет.

## Подключение

1. Создать ассеты через `Create → Game → Configs → RandomEvents`: по `RandomEventScope` на каждый скоуп, по `RandomEventKey` на каждый ключ состояния/сигнала, по `RandomEventDefinition` на каждое событие. Класть в `Assets/_Main/Configs/RandomEvents/`.
2. В каждом `RandomEventDefinition` собрать `actionSteps`, отметить `runInParallel` там, где нужно. Для гейта — добавить шаг со своим действием-условием и задать ему `RandomEventConditionSet`.
3. В `RandomEventScope` заполнить `pools`: минимум один элемент, при необходимости — несколько с разными `picker` и наборами `definitions`.
4. Добавить в сцены `Camp.unity` и `Game.unity` объект с `RandomEventsInstaller`, прописать в `SceneContext → Installers`: `rules`, `scope` и `notificationView` — свои на сцену.
5. Создать на HUD-канвасе объект уведомления: `CanvasGroup` + `TMP_Text`, повесить `RandomEventNotificationView`, проставить ссылки.
6. Завести в `Assets/Locales` ключи локализации для текстов, которые действия покажут через `IRandomEventNotifier`/`RandomEventNotificationView`.

## Расширение

### 1. Из существующих ассетов

Новый `RandomEventDefinition` из уже написанных условий/действий собирается в инспекторе без кода.

### 2. Своё условие или действие

Runtime-класс плюс `[Serializable]`-ассет, в модуле автора:

```csharp
public class PlayerHealthBelowCondition : IRandomEventCondition
{
    private readonly IHealthComponent _health;
    private readonly float _threshold;

    public PlayerHealthBelowCondition(IHealthComponent health, float threshold)
    {
        _health = health;
        _threshold = threshold;
    }

    public bool IsMet() => _health.CurrentHealth < _threshold;
}
```

```csharp
[Serializable]
public class PlayerHealthBelowConditionAsset : ZenjectRandomEventConditionAsset<PlayerHealthBelowCondition>
{
    [SerializeField] [Min(0f)] private float threshold = 30f;

    protected override object[] GetArguments() => new object[] { threshold };
}
```

Всё, что не передано через `GetArguments()`, Zenject подставит сам — включая публичные сервисы других модулей. Для действия — то же самое, но наследоваться от `RandomEventActionBase` и `ZenjectRandomEventActionAsset<T>`.

`GetArguments()` не типизирован: Zenject сопоставляет аргументы с параметрами конструктора по типу, одинаковые типы — по порядку.

### 3. Свой источник сигнала

Игровой модуль публикует факт и о случайных событиях не знает:

```csharp
public interface IAlchemyService
{
    event Action Used;
}
```

Релей в слое оркестрации связывает его с триггер-раннером:

```csharp
public class AlchemyUsageRelay : RandomEventSignalRelay
{
    private readonly IAlchemyService _alchemy;

    public AlchemyUsageRelay(
        IRandomEventTriggerRunner triggerRunner,
        RandomEventKey signal,
        IAlchemyService alchemy)
        : base(triggerRunner, signal)
    {
        _alchemy = alchemy;
    }

    protected override void Subscribe() => _alchemy.Used += OnUsed;

    protected override void Unsubscribe() => _alchemy.Used -= OnUsed;

    private void OnUsed() => Report();
}
```

```csharp
Container.BindInterfacesTo<AlchemyUsageRelay>().AsSingle().WithArguments(alchemyUsageSignal).NonLazy();
```

Тот же ассет `alchemyUsageSignal` кладётся в поле сигнала условия, читающего его из `IRandomEventStateStore`.

### 4. Свой алгоритм выбора

Пикер создаётся один раз на пул (лениво, при первом `StartRandom`, выбравшем этот пул) и живёт всё время жизни `RandomEventService` — память между вызовами можно держать полем на самом пикере.

```csharp
public class NoImmediateRepeatPicker : IRandomEventPicker
{
    private RandomEventDefinition _lastPicked;

    public RandomEventDefinition Pick(IReadOnlyList<RandomEventDefinition> candidates)
    {
        if (candidates == null || candidates.Count == 0)
            return null;

        var remaining = candidates.Count > 1
            ? candidates.Where(c => c != _lastPicked).ToList()
            : candidates;

        var picked = remaining[Random.Range(0, remaining.Count)];
        _lastPicked = picked;

        return picked;
    }
}
```

```csharp
[Serializable]
public class NoImmediateRepeatPickerAsset : ZenjectRandomEventPickerAsset<NoImmediateRepeatPicker>
{
}
```

Назначается в поле `picker` конкретного элемента `pools` — у других пулов того же скоупа остаётся дефолтный `WeightedRandomPicker`.

Ни в одном из этих случаев файлы ядра не меняются.

# RandomEventsModule

## Назначение

Конструктор случайных событий. Событие — это выбор внутри пула → прогон дерева действий → завершение с кулдауном. Когда проверять, при каких условиях, с каким шансом и из какого пула — данные скоупа, а не код.

Ядро (`Runtime/Core`, `Runtime/Composition`, `Runtime/State`, `Runtime/Data`, `Runtime/Signals`) не знает ни одного конкретного события, условия или действия игры. Готовые кирпичи в `Runtime/Chance` и `Runtime/Picking` тоже доменно-нейтральны: они работают только с ключами состояния, временем и весами. `Runtime/Conditions` и `Runtime/Actions` несут только композицию (`RandomEventConditionSet`/`RandomEventConditionGroup`, `RandomEventActionPlan`) и уведомление (`NotifyAction`) — источники триггера, конкретные условия и остальные действия подключает модуль, которому они нужны (см. «Расширение»).

## Идентификация

- Событие адресуется ссылкой на ассет `RandomEventDefinition` — отдельного поля `id` нет.
- Ключ состояния и сигнала — обычная строка (`string`); условие, действие и источник сигнала должны использовать один и тот же литерал, совпадение проверяется по значению, а не по ссылке. Владеет строкой модуль-потребитель — публикуйте её как `[RandomEventSignalKey] public const string` на релее (см. «Расширение → 3»).
- Инспекторные поля `signal`/`counterKey` (`StateScaledChanceAsset` и подобные) помечены `[RandomEventSignalPicker]`: вместо ручного набора строки редактор рисует выпадающий список из всех `[RandomEventSignalKey]`-констант в проекте (`RandomEventSignalPickerDrawer`, ищет их через `TypeCache`, ядро по-прежнему ни одной конкретной строки не знает). Опечатка невозможна — значение всегда берётся из существующей константы, а не набирается заново. Если в поле уже стоит строка без такой константы (устарела, переименовали), поле показывает обычный текст с пометкой «не найден», ничего не теряя.
- Кроме ключей состояния — ключи локализации (`displayNameKey`, `localizationKey`), их проверяет система локализации.

## Конфигурация: скоуп → триггер → пул

**`RandomEventScope`** — ассет на скоуп (в сцене их может быть несколько, каждый на своём контексте):

| Поле | Смысл |
| --- | --- |
| `minIntervalBetweenEventsSeconds` | Минимальная пауза между завершением одного события и стартом следующего, общая для всех триггеров скоупа |
| `triggers` | Список правил запуска |

**`RandomEventTrigger`** — одно полное правило запуска, четыре ответа:

| Поле | Вопрос | Чем заполняется |
| --- | --- | --- |
| `source` | Когда проверять | своя реализация `IRandomEventTriggerSourceAsset` (см. «Расширение») |
| `conditions` | При каких условиях | `RandomEventConditionSet` |
| `chance` | С каким шансом | `FixedChanceAsset`, `StateScaledChanceAsset`; пусто — всегда |
| `pool` | Из чего выбирать | `RandomEventPool` |

Триггеры независимы: у каждого свой таймер, свои условия, свой шанс и свой пул. Лагерь может проверять бытовые события раз в 30 секунд, рейд — раз в 120 с накопительным шансом, а события алхимического стола — только после N использований, и всё это один ассет скоупа.

Источник, условия и шанс проверяются **до** выбора события, поэтому несработавший триггер не тратит кулдаун ни одного события и не двигает `minIntervalBetweenEventsSeconds`.

**`RandomEventPool`** — `[Serializable]`-часть триггера: свой `picker` и список `entries` (`RandomEventPoolEntry`: событие + его вес в этом пуле). Вес живёт в пуле, поэтому одно и то же событие бывает частым в одном пуле и редким в другом. Пустой `picker` означает `WeightedRandomPicker`.

## Модель события

`RandomEventDefinition` — ScriptableObject, описывающий одно событие:

| Поле | Смысл |
| --- | --- |
| `isEnabled` | Выключатель события |
| `isExclusive` | Пока активно, другие события не стартуют |
| `cooldownSeconds` | Пауза до следующего запуска этого же события |
| `sequence` | Дерево действий |
| `displayNameKey` | Ключ локализации имени для UI и отладки |

Определение не знает, в каком оно скоупе, пуле и насколько вероятно — принадлежность и вес решает `RandomEventPool.entries`, один и тот же `RandomEventDefinition` можно вписать сразу в несколько пулов и скоупов.

`RandomEventActionSequence` — плоский список шагов `RandomEventActionStep`: по умолчанию каждый шаг идёт после предыдущего, `runInParallel` — вместе с предыдущим (несколько подряд отмеченных шагов = один параллельный блок). `Create` собирает из списка `RandomEventActionPlan` — то, что исполняется.

Последовательности вкладываются друг в друга: ветка или тело любого композитного действия — это такая же `RandomEventActionSequence`, с тем же инспектором на любой глубине.

`IRandomEventAction.ExecuteAsync` возвращает `bool`: `false` — сигнал гейта остановить дальнейшую цепочку без ошибки. Событие, остановленное гейтом, считается несостоявшимся: кулдаун не назначается, общий минимальный интервал не сдвигается.

Редактируется через `RandomEventDefinitionEditor` и `RandomEventActionSequenceDrawer` — реордерируемый список шагов с поиском по типу для `[SerializeReference]`-полей.

## Готовые кирпичи

Конкретные условия и большинство действий в модуле не поставляются — их подключает модуль, которому они нужны (см. «Расширение»). Из коробки есть доменно-нейтральные части:

**Источники триггера** (`Runtime/TriggerSources`):

- `IntervalTriggerSource` — готов по истечении фиксированного интервала, сигналов не слушает. Пока скоуп активен только в своей сцене (см. «Подключение», п. 4), это и есть проверка «по времени нахождения на базе».
- `SignalCountTriggerSource` — готов, когда счётчик под ключом сигнала достиг `requiredCount`; счётчик — тот же, что копит `ReportSignal`, отдельного ключа не заводит и сам сбрасывает его при срабатывании. Подходит для «после N использований X» — источник сигнала описывается отдельно (см. «Расширение → 3»).

**Шансы** (`Runtime/Chance`):

- `FixedChance` — постоянный процент.
- `StateScaledChance` — `basePercent + stepPercent * счётчик`, с потолком `maxPercent`. Что увеличивает и сбрасывает счётчик, решает конфигурация и действия модуля-потребителя.

**Условия** (`Runtime/Conditions`): композиция — `RandomEventConditionSet`/`RandomEventConditionGroup` (AND/OR над `IRandomEventConditionAsset[]`) — плюс одно доменное условие, которое уже подключает модуль: `HasBuildingsCondition` (`IBuildingRegistry.Buildings.Count > 0`) — используется, чтобы событие не стартовало, когда на базе нечему гореть.

**Действия** (`Runtime/Actions`):

- `NotifyAction` — показать локализованное сообщение через `IRandomEventNotifier`.
- `ChanceGateAction` — доменно-нейтральный гейт: крутит `IRandomEventChance` и гасит остаток последовательности при неудаче. В отличие от `RandomEventTrigger.chance` (общий на все события пула), это шанс конкретного события — ставится первым шагом его `sequence`.
- `ConditionGateAction` — доменно-нейтральный гейт по состоянию: держит свой `RandomEventConditionSet` (тот же блок AND/OR над `IRandomEventConditionAsset[]`, что и у триггера) и возвращает `false`, если условия не выполнены, гася остаток последовательности без назначения кулдауна. Пустой набор всегда проходит. Ставится шагом `sequence`, когда условие проверяется на момент запуска действия, а не триггера.
- `FireEventAction` — поджигает случайное здание из `IBuildingRegistry`, раз в `spreadIntervalSeconds` перекидывает огонь на ближайшее незагоревшееся здание в радиусе `spreadRadius` (не больше `maxBurningBuildings` одновременно). Здание тушится, когда игрок держит кнопку взаимодействия в `extinguishRadius` от него суммарно `extinguishSeconds`; прогресс сбрасывается, стоит отпустить кнопку или отойти. Само здание не гаснет и урона не получает — горит, пока игрок не потушит. Пока игрок в `extinguishRadius` от горящего здания, `RandomEventProgressHudView` показывает шкалу прогресса тушения. VFX и звук горения — через `IEffectsService` и `IAudioSystem`, живут, пока горит здание.
- `RadiationSurgeAction` — наносит игроку урон `damagePerTick` каждые `tickIntervalSeconds` в течение `durationSeconds`. VFX и зацикленный звук — через `IEffectsService` и `IAudioSystem` на всю длительность.
- `BlockEvacuationAction`/`UnblockEvacuationAction` — пара шагов вокруг той части события, которую нельзя пропустить, уйдя в шутер: первый закрывает эвакуацию и задаёт сообщение `blockedMessageKey`, второй снимает запрет с тем же ключом (пустой ключ снимает все). Запреты различаются по ключу, поэтому два одновременных события не снимают блокировку друг друга.
- `DelayAction` — пауза на `durationSeconds` по `IRandomEventClock`: время на паузе не считается.
- `RandomEventActionPlan` — исполнитель последовательности, общий для события и всех вложенных веток; не действие, а движок, на котором строятся все действия.

**Выбор** (`Runtime/Picking`): `WeightedRandomPicker` — взвешенный бросок по весам пула, используется автоматически, если `picker` не назначен.

## Основные части

- `IRandomEventService`/`RandomEventService` — запуск (`Start`, `StartFromPool`), остановка (`Stop`, `StopAll`), события `EventStarted`/`EventFinished`, контроль эксклюзивности, кулдаунов, минимального интервала между событиями.
- `IRandomEventTriggerRunner`/`RandomEventTriggerRunner` — держит рантайм-триггеры скоупа, опрашивает периодические и принимает внешние сигналы.
- `IRandomEventClock`/`RandomEventClock` — ожидание, не считающее время на паузе; им пользуются действия с задержкой или таймаутом.
- `IRandomEventStateStore`/`RandomEventStateStore` — сохраняемое хранилище `int`/`float`/`bool` по строковым ключам.
- `IEvacuationBlocker`/`EvacuationBlocker` — набор запретов на эвакуацию по ключам; реализует `IEvacuationGate` из `EvacuationModule`, поэтому сценовая эвакуация спрашивает его сама, и показывает сообщение самого свежего запрета. Брошенные запреты снимаются сами, когда завершилось последнее активное событие, — забытый или пропущенный гейтом `UnblockEvacuationAction` не запирает сцену насовсем.
- `RandomEventSignalRelay` — база для адаптеров слоя оркестрации, превращающих факт игрового модуля в сигнал.
- `WorkbenchUsageRelay` — репортит сигнал `"workbench_usage"` на каждое открытие верстака (`ICraftService.IsWorkbenchOpen`); зависимость от `ICraftService` необязательна ([InjectOptional]), поэтому сцена без верстака (шутер) релей просто не активирует. Заглушка под будущий алхимический стол (`RandomEventsInstaller` регистрирует его сам, отдельного модуля-владельца сигнала пока нет).
- `RandomEventTriggerInstance` — рантайм-форма триггера: разрешённые источник, условия, шанс и пикер живут всё время жизни раннера, поэтому могут помнить прошлые срабатывания.
- `RandomEventActionPlan` — исполнитель последовательности, общий для события и всех вложенных веток.
- Слой композиции: `IRandomEventCondition`, `IRandomEventAction`, `IRandomEventPicker`, `IRandomEventChance`, `IRandomEventTriggerSource`, `RandomEventActionBase`, их ассет-интерфейсы и Zenject-базы `ZenjectRandomEvent*Asset<T>`, `RandomEventConditionSet`/`RandomEventConditionGroup`.
- Уведомления: `IRandomEventNotifier`/`RandomEventNotifier`, `RandomEventNotificationPresenter`, `RandomEventNotificationView` (MVP, очередь сообщений).
- `RandomEventProgressHudView` — HUD-подсказка + шкала прогресса для действий вида «держи кнопку рядом с чем-то»; используется `FireEventAction` для тушения. Биндится в `RandomEventsInstaller` как `progressHudView`, показывается/скрывается и обновляется напрямую из действия, без презентера.
- Сохранение: `RandomEventsSaveable`, `RandomEventsSaveHandler`, `RandomEventsMemento`.

## Семантика проверки

- Раннер каждый кадр опрашивает те триггеры, у которых подошёл их `PollIntervalSeconds`; `IRandomEventTriggerRunner.ReportSignal(signal)` увеличивает счётчик в `IRandomEventStateStore` и сразу опрашивает те триггеры, чей источник объявил этот ключ своим. `Evaluate()` опрашивает все триггеры немедленно.
- Порядок внутри триггера: источник → условия → шанс → выбор из пула.
- Кандидатом на выбор событие становится по `isEnabled`, весу больше нуля и отсутствию кулдауна.
- Глобальные блокировки (активно эксклюзивное событие, не выдержан `minIntervalBetweenEventsSeconds`) проверяются до перебора кандидатов.
- Пауза: раннер и часы зарегистрированы в `IPauseManager`, на паузе опрос не идёт и ожидания не тикают.
- `ReportSignal` опрашивает синхронно — не вызывайте его на высокочастотных фактах, копите их в релее и репортите агрегатом.
- Игровые модули `IRandomEventTriggerRunner` напрямую не инжектят — связку делает релей в слое оркестрации (см. «Расширение → 3»).

## Сохранение

Единственное сохраняемое состояние — `IRandomEventStateStore` (словари `int`/`float`/`bool`, ключ — строковый литерал сигнала/счётчика). Активные события и кулдауны не сохраняются: смена сцены отменяет запущенные события.

`RandomEventsSaveHandler` регистрирует `RandomEventsSaveable`; `Load()` вызывают сценовые хендлеры (`CampSaveHandler`/`GameSceneSaveHandler`), регистрация выполняется раньше загрузки за счёт `Container.BindExecutionOrder<RandomEventsSaveHandler>(-100)`.

`SaveKey` `"random_events"` менять нельзя. Строковый ключ сигнала/счётчика тоже менять нельзя после того, как под ним накопилось сохранённое состояние — переименование литерала осиротит накопленное значение в сейве так же, как переименование поля.

## Подключение

1. Создать ассеты через `Create → Game → Configs → RandomEvents`: по `RandomEventScope` на каждый скоуп, по `RandomEventDefinition` на каждое событие. Класть в `Assets/_Main/Configs/RandomEvents/`. Ключи состояния/сигнала — не ассеты, а строковые константы модуля-потребителя (см. «Идентификация»); в инспекторные поля `signal`/`counterKey` набираются вручную тем же литералом.
2. В каждом `RandomEventDefinition` собрать `sequence`, отметить `runInParallel` там, где нужно.
3. В `RandomEventScope` завести по триггеру на каждый способ запуска: выбрать источник, при необходимости условия и шанс, заполнить пул событиями и весами.
4. Добавить в сцены `Camp.unity` и `Game.unity` объект с `RandomEventsInstaller`, прописать в `SceneContext → Installers`: `scope`, `notificationView` и `progressHudView` — свои на сцену.
5. Создать на HUD-канвасе объект уведомления: `CanvasGroup` + `TMP_Text`, повесить `RandomEventNotificationView`, проставить ссылки.
6. Создать на HUD-канвасе объект прогресс-подсказки: `CanvasGroup` + `TMP_Text` (подсказка) + `Image` (заполняемая шкала), повесить `RandomEventProgressHudView`, проставить ссылки.
7. Завести в `Assets/Locales` ключи локализации для текстов, которые покажет `NotifyAction` или прогресс-подсказка.

## Расширение

### 1. Из существующих ассетов

Когда нужный набор источника триггера, условий, шанса и действий уже подключён (своим модулем или другим тикетом), новое событие и новое правило запуска собираются в инспекторе без кода.

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

    protected override object[] GetArguments(DiContainer container) => new object[] { threshold };
}
```

Всё, что не передано через `GetArguments()`, Zenject подставит сам — включая публичные сервисы других модулей. Для действия — то же самое, но наследоваться от `RandomEventActionBase` и `ZenjectRandomEventActionAsset<T>`; долгое действие принимает `CancellationToken` из `ExecuteAsync` и переопределяет `RequestStop`.

`GetArguments()` не типизирован: Zenject сопоставляет аргументы с параметрами конструктора по типу, одинаковые типы — по порядку. Контейнер передаётся аргументом, поэтому ассет может собрать вложенные `RandomEventActionSequence.Create(container)` и `RandomEventConditionSet.Create(container)` и отдать их конструктору — так устроен composite-action с вложенной последовательностью (ветка, тело цикла и т.п.).

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
    [RandomEventSignalKey] public const string SignalKey = "alchemy_table_usage";

    private readonly IAlchemyService _alchemy;

    public AlchemyUsageRelay(IRandomEventTriggerRunner triggerRunner, IAlchemyService alchemy)
        : base(triggerRunner, SignalKey)
    {
        _alchemy = alchemy;
    }

    protected override void Subscribe() => _alchemy.Used += OnUsed;

    protected override void Unsubscribe() => _alchemy.Used -= OnUsed;

    private void OnUsed() => Report();
}
```

```csharp
Container.BindInterfacesTo<AlchemyUsageRelay>().AsSingle().NonLazy();
```

Ключ — константа на самом релее, а не инжектируемый аргумент, так что нет смысла городить биндинг ради значения, которое и так зашито в код. `[RandomEventSignalKey]` делает её видимой для `[RandomEventSignalPicker]` на поле `signal`/`counterKey` источника триггера, который её ждёт (см. «Расширение → 5») — там её выбирают из списка, а не перепечатывают.

### 4. Свой алгоритм выбора

Пикер создаётся один раз на триггер и живёт всё время жизни раннера — память между вызовами можно держать полем на самом пикере.

```csharp
public class NoImmediateRepeatPicker : IRandomEventPicker
{
    private RandomEventDefinition _lastPicked;

    public RandomEventDefinition Pick(IReadOnlyList<RandomEventCandidate> candidates)
    {
        if (candidates == null || candidates.Count == 0)
            return null;

        var remaining = candidates.Count > 1
            ? candidates.Where(c => c.Definition != _lastPicked).ToList()
            : candidates;

        _lastPicked = remaining[Random.Range(0, remaining.Count)].Definition;

        return _lastPicked;
    }
}
```

```csharp
[Serializable]
public class NoImmediateRepeatPickerAsset : ZenjectRandomEventPickerAsset<NoImmediateRepeatPicker>
{
}
```

Назначается в поле `picker` конкретного пула — у других триггеров остаётся `WeightedRandomPicker`.

### 5. Свой источник триггера или своя формула шанса

Так же, как условие: класс плюс `[Serializable]`-ассет от `ZenjectRandomEventTriggerSourceAsset<T>` или `ZenjectRandomEventChanceAsset<T>`. Источник отвечает за `PollIntervalSeconds`, `HandlesSignal` и `TryFire`, шанс — за один `Roll`.

Ни в одном из этих случаев файлы ядра не меняются.

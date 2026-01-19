# Code Style (C# / Unity)

Внутренний документ команды • 18.01.2026

Документ фиксирует правила оформления кода и именования. Используется на ревью и при разработке.

---

## 1) Именование

### 1.1 Сущности (классы/переменные/поля/свойства/аргументы) — **существительные**
Имя должно точно описывать сущность. **Сокращения запрещены** (кроме общепринятых вроде `ID`, `UI`, `IP`, `URL`).

**Хорошо**
- `HealthComponent`, `Inventory`, `PlayerDeathHandler`, `DamageCalculator`
- `currentHealth`, `maxHealth`, `damageAmount`, `spawnPoint`
- `playerTransform`, `cameraPivot`, `inputDirection`

**Плохо**
- `HpComp`, `Inv`, `PlrDeathHnd` (сокращения)
- `data`, `temp`, `value1` (неинформативно)
- `DoDamage` (глагол для сущности)

---

### 1.2 Булевы значения
`bool` часто лучше начинать с `is/has/can/should`.

**Примеры**
- `isAlive`, `hasKey`, `canJump`, `shouldRespawn`, `isGrounded`, `hasTarget`

---

### 1.3 Константы — `PascalCase`
**Хорошо**
```csharp
private const int MaxItems = 32;
private const float DefaultMoveSpeed = 5f;
public const string DefaultSaveFileName = "save.json";
```

**Плохо**
```csharp
private const int MAX_ITEMS = 32;
private const float defaultMoveSpeed = 5f;
```

---

### 1.4 Методы — начинаются с глагола, `PascalCase`
**Хорошо**
- `ApplyDamage(int amount)`
- `TakeItem(Item item)`
- `MoveTo(Vector3 position)`
- `Initialize()`
- `TryGetTarget(out Target target)`
- `UpdateHealthBar()`

**Плохо**
- `Damage(int amount)` (похоже на сущность)
- `PlayerMovement()` (похоже на класс)

---

### 1.5 События (events) — описывают **что произошло**
Имя ивента — глагол/причастие в прошедшем времени, описывающий изменение/событие.

**Хорошо**
- `HealthChanged`
- `Died`
- `ItemPickedUp`
- `StateChanged`

**Плохо**
- `ChangeHealth` (звучит как команда)
- `OnHealthChanged` (это имя обработчика, не события)

---

### 1.6 Обработчики событий: `On` + имя события
Методы подписки/обработчики пишутся `On` + `EventName`.

**Пример**
```csharp
health.HealthChanged += OnHealthChanged;

private void OnHealthChanged(int current, int max)
{
    UpdateHealthBar();
}
```

---

## 2) Стиль имен в C#

### 2.1 Классы — `PascalCase`
**Хорошая практика**
- `HealthComponent`, `Inventory`, `PlayerDeathHandler`

**Плохая практика**
- `player_controller`, `gameManager`

---

### 2.2 Методы — `PascalCase`
**Примеры**
- `StartGame()`
- `LoadScene(string sceneName)`
- `HandleInput()`
- `TryConsumeAmmo(int amount)`

---

### 2.3 Локальные переменные и аргументы — `camelCase`
**Примеры**
```csharp
public void ApplyDamage(int damageAmount)
{
    var newHealth = _currentHealth - damageAmount;
    _currentHealth = Mathf.Max(newHealth, 0);
}
```

---

### 2.4 Приватные поля/свойства (кроме `SerializeField`) — `_camelCase`
**Примеры**
```csharp
private int _currentHealth;
private float _moveSpeed;
private readonly List<Item> _items = new();
```

---

### 2.5 Unity-поля для инспектора: `[SerializeField] private` — `camelCase`
**Примеры**
```csharp
[SerializeField] private float moveSpeed;
[SerializeField] private Transform cameraPivot;
[SerializeField] private AudioSource hitAudio;
```

---

### 2.6 Public / protected поля, свойства, события — `PascalCase`
**Примеры**
```csharp
public int CurrentHealth => _currentHealth;
public event Action HealthChanged;
protected Transform Target { get; private set; }
```

---

## 3) Модификаторы доступа

**Всегда у всего пишем модификаторы доступа** (за исключением членов интерфейса).

**Хорошая практика**
```csharp
private void Update() { }
public string MyProperty { get; }
private int _myField;
```

**Плохая практика**
```csharp
void Update() { }
string MyProperty { get; }
int _myField;
```

---

## 4) Свойства и короткие методы

### 4.1 Предпочитаем read-only свойства через `=>`
**Хорошая практика**
```csharp
private int _myField;
public int MyField => _myField;
```

**Плохая практика**
```csharp
public int MyProperty { get; private set; }
```
---

### 4.2 Однострочные методы — через `=>`
**Примеры**
```csharp
public void ResetHealth() => _currentHealth = _maxHealth;

private bool IsDead() => _currentHealth <= 0;

public int GetDamage() => _weapon.Damage;
```

---

## 5) Файлы и структура

### 5.1 Один класс — один файл
Имя файла = имя класса: `PlayerDeathHandler.cs` содержит `PlayerDeathHandler`.

---

### 5.2 Структура класса (порядок секций)
1) поля  
2) ивенты  
3) конструкторы  
4) свойства  
5) методы  

---

### 5.3 Порядок между членами по доступу
Сначала `public`, затем `protected`, затем `private`.

> Исключение: unity методы (Awake/Start/Update и т.д.), всегда идут перед рукописными методами

**Пример**
```csharp
public class Health
{
    private readonly int _maxHealth;

    private int _currentHealth;

    public event Action<int> HealthChanged;

    public Health(int maxHealth)
    {
        _maxHealth = maxHealth;
    }

    public int CurrentHealth => _currentHealth;

    public void TakeDamage(int amount)
    {
        // ...
    }

    private void Die()
    {
        // ...
    }
}


```

---

## 6) Форматирование и пустые строки

### 6.1 Поля с разными модификаторами/атрибутами разделяем пустой строкой
**Пример**
```csharp
[SerializeField] private float moveSpeed;

private readonly int _maxHealth;

private int _currentHealth;
```

---

### 6.2 Все методы разделяем пустой строкой (кроме интерфейсов)
**Пример**
```csharp
public void Initialize()
{
    // ...
}

public void ApplyDamage(int amount)
{
    // ...
}
```

---

### 6.3 Условные конструкции (`if/for/while` и т.д.) отделяем пустой строкой
**Пример**
```csharp
private void Update()
{
    if (!isAlive)
        return;

    var input = ReadInput();

    if (input.sqrMagnitude > 0f)
        Move(input);

    for (var i = 0; i < _items.Count; i++)
        ProcessItem(_items[i]);
}
```

---

## 7) Namespace / using

У всех классов должен быть `using/namespace` по имени корневой папки модуля, в котором находится объект.

**Пример**
```csharp
namespace InventoryModule
{
    public sealed class Inventory { }
}
```

---

## 8) Комментарии

**Никаких комментариев в коде.** Код должен быть выразительным без комментариев.

Допускается:
- `// TODO:`  
- `// FIXME:`  
- `/// <summary>...</summary>` для методов/публичного API

---

## 9) Логи

`Debug.Log` разрешён только для внутреннего логирования. В продукт-коде его не должно быть.

---

## 10) Unity-специфика

### 10.1 `[SerializeField] private` вместо `public` для инспектора
**Хорошо**
```csharp
[SerializeField] private Transform target;
```

**Плохо**
```csharp
public Transform target;
```

---

### 10.2 Запрет/ограничение: `FindObjectOfType`, `GameObject.Find`
Используем DI/ссылки через инспектор/фабрики/сервисы.

**Плохо**
```csharp
var player = GameObject.Find("Player");
var ui = FindObjectOfType<UIRoot>();
```

---

### 10.3 `Update`: минимум логики и аллокаций
Не делать тяжёлых аллокаций, кешировать ссылки, стараться не писать много логики в `Update`, выносить в методы.

**Хорошо**
```csharp
[SerializeField] private Enemy[] _enemies;

private void Update()
{
    TickCombat();
}

private void TickCombat()
{
    for (int i = 0; i < _enemies.Length; i++)
        _enemies[i].Tick();
}
```

**Плохо**
```csharp
private void Update()
{
    // аллокации / тяжелые операции каждый кадр
    var enemies = FindObjectsOfType<Enemy>();
    foreach (var enemy in enemies)
    enemy.DoSomething();
}
```

---

### 10.4 События: подписка/отписка в `OnEnable/OnDisable`
**Пример**
```csharp
private void OnEnable()
{
    health.HealthChanged += OnHealthChanged;
}

private void OnDisable()
{
    health.HealthChanged -= OnHealthChanged;
}
```

---

### 10.5 Не использовать `GetComponent<T>()` в `Update`
Стараться прокидывать ссылки на компоненты через `SerializeField`.

Также **не использовать связку** `[RequireComponent] + GetComponent в Awake`.

**Хорошо**
```csharp
[SerializeField] private Rigidbody rigidbody;

private void FixedUpdate()
{
    rigidbody.AddForce(Vector3.forward);
}
```

**Плохо**
```csharp
private void Update()
{
    GetComponent<Rigidbody>().AddForce(Vector3.forward);
}

[RequireComponent(typeof(Rigidbody))]
public class Mover : MonoBehaviour
{
    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }
}
```

---

### 10.6 Не использовать `UnityAction`, использовать `Action`

**Хорошо**
```csharp
public event Action<int> HealthChanged;
```

**Плохо**
```csharp
public event UnityAction<int> HealthChanged;
```

---

## 11) Асинхронщина (если используете)

- Суффикс `Async`: `LoadDataAsync()`
- `CancellationToken` обязателен для долгих операций.

**Пример**
```csharp
public async Task LoadDataAsync(CancellationToken token)
{
    token.ThrowIfCancellationRequested();
    await Task.Delay(100, token);
}
```

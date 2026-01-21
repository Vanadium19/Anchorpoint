using UnityEngine;

public class FreeCameraController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; // Скорость движения
    [SerializeField] private float lookSpeed = 2f; // Скорость поворота
    [SerializeField] private float minHeight = 1f; // Минимальная высота камеры
    [SerializeField] private float maxHeight = 10f; // Максимальная высота камеры (по желанию)

    private bool _isActive = false; // Камера активна или нет
    private bool _isRotating = false; // Флаг для вращения
    private Vector3 _initialPosition; // Исходная позиция камеры
    private Quaternion _initialRotation; // Исходное вращение камеры
    private float _rotationX = 0f; // Переменная для отслеживания угла вращения по оси X (вверх/вниз)

    private void Start()
    {
        // Сохраняем начальную позицию и вращение камеры
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
    }

    private void Update()
    {
        if (!_isActive) return; // Если камера не активна, не выполняем действия

        MoveCamera(); // Двигаем камеру
        RotateCamera(); // Поворачиваем камеру

        // Ограничиваем высоту камеры
        Vector3 position = transform.position;
        position.y = Mathf.Clamp(position.y, minHeight, maxHeight); // Ограничиваем ось Y
        transform.position = position;
    }

    private void MoveCamera()
    {
        // Движение камеры по осям X и Z
        float horizontal = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        float vertical = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;

        // Перемещаем по осям X и Z (не учитываем ось Y)
        transform.Translate(horizontal, 0f, vertical);
    }

    private void RotateCamera()
    {
        // Вращение камеры с помощью колесика мыши
        if (Input.GetMouseButtonDown(2)) // Если нажали на колесико мыши
        {
            _isRotating = true;
        }

        if (Input.GetMouseButtonUp(2)) // Если отпустили колесико мыши
        {
            _isRotating = false;
        }

        // Вращение камеры только если колесико мыши нажато
        if (_isRotating)
        {
            float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

            // Ограничиваем вращение по оси X (вверх/вниз), чтобы камера не переворачивалась
            _rotationX -= mouseY;
            _rotationX = Mathf.Clamp(_rotationX, -80f, 80f); // Ограничиваем угол вращения по оси X

            // Окружность камеры по оси Y и ограничение по оси X для вращения по вертикали
            transform.localRotation = Quaternion.Euler(_rotationX, transform.localEulerAngles.y + mouseX, 0f);
        }
    }

    // Метод для включения / выключения камеры
    public void SetActive(bool isActive)
    {
        _isActive = isActive;

        // Если камера деактивирована, возвращаем её в исходное положение
        if (!_isActive)
        {
            transform.position = _initialPosition;
            transform.rotation = _initialRotation;
        }
    }
}

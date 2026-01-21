using UnityEngine;

public sealed class PlacementManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Material greenMaterial;
    [SerializeField] private Material redMaterial;
    [SerializeField] private BuildMenuController buildMenuController; // Ссылка на BuildMenuController
    [SerializeField] private FreeCameraController freeCameraController;
    private GameObject _previewObject;
    private GameObject _selectedPrefab;
    private Tile _currentTile;

    private bool _isPlacementMode;

    private void Update()
    {
        if (!_isPlacementMode)
            return;

        UpdatePreview();

        if (Input.GetMouseButtonDown(1))
            CancelPlacement();

        if (Input.GetMouseButtonDown(0))
            TryPlace();
    }

    // Добавляем метод Reset() для очистки состояния
    public void Reset()
    {
        _isPlacementMode = false;
        Destroy(_previewObject);
        _currentTile = null;
        _selectedPrefab = null;
    }

    public void StartPlacement(GameObject prefab)
    {
        _selectedPrefab = prefab;
        _previewObject = Instantiate(prefab);
        ApplyPreviewMaterial(greenMaterial);
        _isPlacementMode = true;
    }

    private void UpdatePreview()
    {
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out var hit))
            return;

        var tile = gridManager.GetNearestTile(hit.point);

        if (tile == null)
            return;

        _previewObject.transform.position = tile.WorldPosition;
        _currentTile = tile;

        ApplyPreviewMaterial(tile.IsOccupied ? redMaterial : greenMaterial);
    }

    private void TryPlace()
    {
        if (_currentTile == null || _currentTile.IsOccupied)
            return;

        Instantiate(_selectedPrefab, _currentTile.WorldPosition, Quaternion.identity);
        _currentTile.Occupy();

        ExitPlacement();
    }

    private void CancelPlacement() => ExitPlacement();

    private void ExitPlacement()
    {
        Destroy(_previewObject);
        _isPlacementMode = false;

        // Закрываем меню и возвращаем камеру в исходное положение
        buildMenuController.FinishPlacing();
    }

    private void ApplyPreviewMaterial(Material material)
    {
        var renderer = _previewObject.GetComponentInChildren<Renderer>();
        renderer.material = material;
    }

    public void CancelAll()
    {
        Reset(); // Очистить режим размещения
        buildMenuController.FinishPlacing(); // Закрыть меню
        freeCameraController.SetActive(false); // Деактивировать камеру
        Cursor.lockState = CursorLockMode.None; // Разблокировать курсор
        Cursor.visible = true; // Сделать курсор видимым
    }


}

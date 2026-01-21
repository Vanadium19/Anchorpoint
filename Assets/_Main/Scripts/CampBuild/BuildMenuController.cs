using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class BuildMenuController : MonoBehaviour
{
    [SerializeField] private GameObject buildMenuPanel;      // Панель с меню
    [SerializeField] private FreeCameraController freeCameraController; // Ссылка на камеру
    [SerializeField] private CanvasGroup canvasGroup;         // CanvasGroup для панели
    [SerializeField] private float fadeDuration = 0.5f;      // Время анимации для прозрачности
    [SerializeField] private PlacementManager placementManager;  // Ссылка на PlacementManager
    [SerializeField] private GameObject buildPrefab;   // Префаб для строительства

    private bool _isPlacing = false;
    private bool _isCameraFree = false; // Флаг, указывающий, должна ли камера быть свободной


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            ToggleMenu();

        if (Input.GetKeyDown(KeyCode.Escape))
            CancelAll(); 
    }



    public void StartPlacing()
    {
        _isPlacing = true;
        StartCoroutine(FadeMenu(false));
        freeCameraController.SetActive(true);  // Камера активна
    }

    public void FinishPlacing()
    {
        buildMenuPanel.SetActive(true); // Снова включаем меню
        freeCameraController.SetActive(false); // Камера фиксирована
        placementManager.Reset(); // Сбросить размещение
    }

    private void ToggleMenu()
    {
        bool isActive = !buildMenuPanel.activeSelf;
        buildMenuPanel.SetActive(isActive);

        if (isActive)
        {
            // Если меню открыто, фиксируем камеру
            _isCameraFree = false;
            freeCameraController.SetActive(false);  // Камера фиксирована
            canvasGroup.alpha = 1f; // Панель видима
        }
        else
        {
            // Если меню закрыто, возвращаем камеру в свободный режим
            _isCameraFree = true;
            StartCoroutine(FadeMenu(true)); // Анимация меню
        }
    }




    private void CancelAll()
    {
        placementManager.CancelAll(); // Очищаем режим размещения
        freeCameraController.SetActive(false); // Камера снова фиксирована
        buildMenuPanel.SetActive(false); // Меню становится неактивным
    }


    private IEnumerator FadeMenu(bool fadeIn)
    {
        float targetAlpha = fadeIn ? 1f : 0.5f; // Прозрачность
        float startAlpha = canvasGroup.alpha;

        float time = 0;
        while (time < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }

    public void OnCampFireButtonClicked()
    {
        // Запускаем установку
        StartPlacing();

        // Переходим в режим установки с выбранным префабом
        placementManager.StartPlacement(buildPrefab);
    }
}

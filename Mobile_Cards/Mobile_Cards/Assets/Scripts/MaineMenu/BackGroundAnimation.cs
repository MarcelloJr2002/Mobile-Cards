using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundAnimation : MonoBehaviour
{
    public CardsModels cardsModels; // Referencja do klasy z modelami kart
    public float spawnInterval = 0.5f; // Czas między generowaniem kart
    public float moveDuration = 3f; // Czas animacji karty w górę
    public float fixedScale = 0.1f; // Stała skala kart
    public float yOffsetAboveScene = 5f; // Jak wysoko karta powinna zniknąć ponad górną krawędzią sceny
    public float extraWidthFactor = 2.5f; // Współczynnik poszerzenia obszaru spawn (1 = szerokość sceny, 1.5 = 50% szersza)

    private Camera mainCamera;
    private float sceneWidth; // Szerokość sceny (widoczna przez kamerę)
    private float bottomY; // Dolna krawędź sceny (poza widocznym obszarem)
    private float topY; // Górna krawędź sceny (znikanie kart)

    private void Start()
    {
        // Oblicz wymiary widocznej sceny na podstawie kamery
        mainCamera = Camera.main;
        sceneWidth = mainCamera.orthographicSize * 2 * mainCamera.aspect;
        bottomY = mainCamera.transform.position.y - mainCamera.orthographicSize - 1f; // 1f poniżej dolnej krawędzi
        topY = mainCamera.transform.position.y + mainCamera.orthographicSize + yOffsetAboveScene; // Wyżej niż górna krawędź sceny

        // Regularne generowanie kart
        InvokeRepeating(nameof(SpawnCard), 0f, spawnInterval);
    }

    private void SpawnCard()
    {
        // Wybierz losowy sprite z CardsModels
        Sprite randomSprite = cardsModels.GetRandomSprite();

        // Stwórz nowy obiekt karty
        GameObject card = new GameObject("Card");
        SpriteRenderer renderer = card.AddComponent<SpriteRenderer>();
        renderer.sprite = randomSprite;

        // Ustal losową pozycję początkową karty na poszerzonej szerokości sceny
        float randomX = Random.Range(-sceneWidth * extraWidthFactor / 2, sceneWidth * extraWidthFactor / 2);
        Vector3 spawnPosition = new Vector3(randomX, bottomY, 0f);
        card.transform.position = spawnPosition;

        // Ustaw stałą skalę karty
        card.transform.localScale = Vector3.one * fixedScale;

        // Animacja karty w górę (aż poza górną krawędź sceny)
        Vector3 endPosition = new Vector3(randomX, topY, 0f);
        card.transform.DOMove(endPosition, moveDuration).OnComplete(() => Destroy(card));
    }

}

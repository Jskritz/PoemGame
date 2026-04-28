using UnityEngine;
using System.Collections.Generic;

public class ScreenKiler : MonoBehaviour
{
    public static ScreenKiler Instance { get; private set; }

    [SerializeField]
    private bool dontDestroyOnLoad = true;

    [SerializeField]
    private Color removalColor = Color.black;

    private const int columns = 4;  
    private const int rows = 4;
    private const int sectionCount = columns * rows;

    private readonly bool[] removedSections = new bool[sectionCount];
    private Texture2D fillTexture;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }

        InitializeSingleton();
    }

    private void InitializeSingleton()
    {
        fillTexture = new Texture2D(1, 1);
        fillTexture.SetPixel(0, 0, Color.white);
        fillTexture.Apply();
    }

    private void Start()
    {
        // Use this for initialization.
    }

    private void Update()
    {
        // Example trigger: press Space to remove a new random section.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RemoveRandomScreenSection();
        }
    }

    public void RemoveRandomScreenSection()
    {
        List<int> availableSections = GetAvailableSectionIndices();

        if (availableSections.Count == 0)
        {
            Debug.Log("All screen sections have already been removed.");
            return;
        }

        int nextSection = availableSections[Random.Range(0, availableSections.Count)];
        RemoveSection(nextSection);
    }

    public void RemoveSection(int sectionIndex)
    {
        if (sectionIndex < 0 || sectionIndex >= sectionCount)
        {
            Debug.LogWarning($"Invalid section index: {sectionIndex}");
            return;
        }

        if (removedSections[sectionIndex])
        {
            Debug.LogWarning($"Section {sectionIndex} is already removed.");
            return;
        }

        removedSections[sectionIndex] = true;
        Debug.Log($"Removed screen section {sectionIndex}");
    }

    private List<int> GetAvailableSectionIndices()
    {
        List<int> available = new List<int>(sectionCount);

        for (int i = 0; i < sectionCount; i++)
        {
            if (!removedSections[i])
            {
                available.Add(i);
            }
        }

        return available;
    }

    private Rect GetSectionRect(int sectionIndex)
    {
        int x = sectionIndex % columns;
        int y = sectionIndex / columns;

        float sectionWidth = Screen.width / (float)columns;
        float sectionHeight = Screen.height / (float)rows;

        return new Rect(x * sectionWidth, y * sectionHeight, sectionWidth, sectionHeight);
    }

    private void OnGUI()
    {
        if (fillTexture == null)
        {
            return;
        }

        Color originalColor = GUI.color;
        GUI.color = removalColor;

        for (int i = 0; i < sectionCount; i++)
        {
            if (removedSections[i])
            {
                GUI.DrawTexture(GetSectionRect(i), fillTexture);
            }
        }

        GUI.color = originalColor;
    }

    public void ResetRemovedSections()
    {
        for (int i = 0; i < sectionCount; i++)
        {
            removedSections[i] = false;
        }
    }

    public bool IsSectionRemoved(int sectionIndex)
    {
        return sectionIndex >= 0 && sectionIndex < sectionCount && removedSections[sectionIndex];
    }
}

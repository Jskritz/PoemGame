using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ScreenRemovalBlock
{
    public Rect rect;
    public Color color;

    public ScreenRemovalBlock(Rect rect, Color color)
    {
        this.rect = rect;
        this.color = color;
    }
}

public class ScreenKiler : MonoBehaviour
{
    public static ScreenKiler Instance { get; private set; }

    [SerializeField]
    private bool dontDestroyOnLoad = true;

    private List<ScreenRemovalBlock> removedBlocks = new List<ScreenRemovalBlock>();
    private List<Vector2> uncoveredPoints = new List<Vector2>();
    private Texture2D fillTexture;
    private float totalScreenArea;

    [SerializeField]
    private int pointSamplingStep = 5; // Sample points every N pixels for efficiency

    [SerializeField]
    private int maxAttempts = 100; // Maximum attempts to find a valid block position

    // Pool of colors for blocks
    private Color[] blockColors = new Color[]
    {
        new Color(0f, 0f, 0f, 1f),       // Black
        new Color(0.5f, 0f, 0f, 1f),     // Dark Red
        new Color(0f, 0.5f, 0f, 1f),     // Dark Green
        new Color(0f, 0f, 0.5f, 1f),     // Dark Blue
        new Color(0.5f, 0.5f, 0f, 1f),   // Dark Yellow
        new Color(0.5f, 0f, 0.5f, 1f),   // Dark Magenta
        new Color(0f, 0.5f, 0.5f, 1f),   // Dark Cyan
        new Color(0.3f, 0.3f, 0.3f, 1f), // Dark Gray
    };

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
        
        totalScreenArea = Screen.width * Screen.height;
        
        // Initialize uncovered points grid
        uncoveredPoints.Clear();
        for (int x = 0; x < Screen.width; x += pointSamplingStep)
        {
            for (int y = 0; y < Screen.height; y += pointSamplingStep)
            {
                uncoveredPoints.Add(new Vector2(x, y));
            }
        }
        
        Debug.Log($"ScreenKiler initialized. Total screen area: {totalScreenArea} pixels. Uncovered points: {uncoveredPoints.Count}");
    }

    private void Start()
    {
        // Use this for initialization.
    }

    private void Update()
    {
        // Trigger: press Space to remove a new random block.
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            RemoveRandomBlock();
            CheckForEndgame();
        }
    }

    public void CheckForEndgame()
    {
        if (GetTotalRemovedScreenPercentage() >= 100f)
        {
            Debug.Log("Game Over! The entire screen has been removed.");
            // You can add additional endgame logic here, such as showing a message or restarting the game.
        }
    }

    public void RemoveRandomBlock()
    {
        // Find an uncovered position
        Vector2 uncoveredPosition = FindUncoveredPosition();
        if (uncoveredPosition == Vector2.negativeInfinity)
        {
            Debug.Log("No uncovered positions found! Screen is fully covered.");
            return;
        }

        // Generate a block centered on this position that covers at least 10% non-overlapping area
        float targetArea = totalScreenArea * 0.1f; // 10% of screen
        Rect blockAroundPosition = Rect.zero;
        int attempts = 0;

        blockAroundPosition = GenerateBlockAroundPosition(uncoveredPosition);
        AddBlock(blockAroundPosition);
        return;
        while (attempts < maxAttempts)
        {
            blockAroundPosition = GenerateBlockAroundPosition(uncoveredPosition);
            float nonOverlappingArea = CalculateNonOverlappingArea(blockAroundPosition);
            Debug.Log($"Generated block with non-overlapping area {nonOverlappingArea} pixels (target: {targetArea} pixels).");
            if (nonOverlappingArea >= targetArea)
            {
                // Success! Add the block
                AddBlock(blockAroundPosition);
                return;
            }

            attempts++;
        }

        // If we couldn't find a valid block after max attempts, log a warning
        Debug.LogWarning($"Could not find a block around position ({uncoveredPosition.x}, {uncoveredPosition.y}) that covers 10% non-overlapping area after {maxAttempts} attempts.");
    }

    private Vector2 FindUncoveredPosition()
    {
        if (uncoveredPoints.Count == 0)
        {
            return Vector2.negativeInfinity; // No uncovered positions found
        }

        // Pick a random point from the remaining uncovered points
        int randomIndex = Random.Range(0, uncoveredPoints.Count);
        return uncoveredPoints[randomIndex];
    }

    private Rect GenerateBlockAroundPosition(Vector2 centerPosition)
    {
        // Block should cover at least 10% of total screen area (with some variation)
        float targetArea = totalScreenArea * Random.Range(0.10f, 0.15f); // 10-15% to ensure we meet the 10% requirement
        Debug.Log($"Generating block from {totalScreenArea} total area. Target area: {targetArea} pixels.");
        
        // Random aspect ratio
        float aspectRatio = Random.Range(0.5f, 2f);

        // Calculate dimensions without clamping first
        float height = Mathf.Sqrt(targetArea / aspectRatio);
        float width = targetArea / height;

        // Clamp to screen bounds
        width = Mathf.Min(width, Screen.width);
        height = Mathf.Min(height, Screen.height);

        // Position the block centered on the uncovered position
        float halfWidth = width / 2f;
        float halfHeight = height / 2f;

        float x = centerPosition.x - halfWidth;
        float y = centerPosition.y - halfHeight;

        // Ensure the block stays within screen bounds
        x = Mathf.Clamp(x, 0, Screen.width - width);
        y = Mathf.Clamp(y, 0, Screen.height - height);

        return new Rect(x, y, width, height);
    }

    public void AddBlock(Rect rect)
    {
        Color randomColor = blockColors[Random.Range(0, blockColors.Length)];
        ScreenRemovalBlock block = new ScreenRemovalBlock(rect, randomColor);
        removedBlocks.Add(block);
        
        // Remove all points covered by this new block
        uncoveredPoints.RemoveAll(point => rect.Contains(point));
        
        Debug.Log($"Added new block at ({rect.x}, {rect.y}) with size {rect.width}x{rect.height}. Total blocks: {removedBlocks.Count}. Remaining uncovered points: {uncoveredPoints.Count}. Coverage: {GetTotalRemovedScreenPercentage():F1}%");
    }

    /// <summary>
    /// Calculates the non-overlapping area of a rect with existing blocks.
    /// </summary>
    private float CalculateNonOverlappingArea(Rect rect)
    {
        float area = rect.width * rect.height;
        
        foreach (var block in removedBlocks)
        {
            if (rect.Overlaps(block.rect))
            {
                Rect overlap = GetRectIntersection(rect, block.rect);
                area -= overlap.width * overlap.height;
            }
        }
        
        return Mathf.Max(0, area);
    }

    /// <summary>
    /// Gets the intersection rectangle of two rects.
    /// </summary>
    private Rect GetRectIntersection(Rect a, Rect b)
    {
        float xMin = Mathf.Max(a.xMin, b.xMin);
        float xMax = Mathf.Min(a.xMax, b.xMax);
        float yMin = Mathf.Max(a.yMin, b.yMin);
        float yMax = Mathf.Min(a.yMax, b.yMax);
        
        if (xMin >= xMax || yMin >= yMax)
        {
            return Rect.zero;
        }
        
        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    }

    private void OnGUI()
    {
        if (fillTexture == null)
        {
            return;
        }

        Color originalColor = GUI.color;

        foreach (var block in removedBlocks)
        {
            GUI.color = block.color;
            GUI.DrawTexture(block.rect, fillTexture);
        }

        GUI.color = originalColor;
    }

    public void ResetRemovedSections()
    {
        removedBlocks.Clear();
        
        // Reinitialize uncovered points
        uncoveredPoints.Clear();
        for (int x = 0; x < Screen.width; x += pointSamplingStep)
        {
            for (int y = 0; y < Screen.height; y += pointSamplingStep)
            {
                uncoveredPoints.Add(new Vector2(x, y));
            }
        }
        
        Debug.Log("All blocks have been reset.");
    }

    public bool IsPointRemoved(Vector2 point)
    {
        foreach (var block in removedBlocks)
        {
            if (block.rect.Contains(point))
            {
                return true;
            }
        }
        return false;
    }

    public int GetTotalBlockCount()
    {
        return removedBlocks.Count;
    }

    public float GetTotalRemovedScreenPercentage()
    {
        float totalRemovedArea = 0f;
        
        foreach (var block in removedBlocks)
        {
            totalRemovedArea += block.rect.width * block.rect.height;
        }
        
        return (totalRemovedArea / totalScreenArea) * 100f;
    }
}

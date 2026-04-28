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

    [SerializeField]
    private float minBlockSizePercent = 10f; // Minimum percentage of screen to cover

    [SerializeField]
    private float maxBlockSizePercent = 30f; // Maximum percentage of screen to cover

    [SerializeField]
    private int maxAttempts = 50; // Maximum attempts to find a valid block position

    private List<ScreenRemovalBlock> removedBlocks = new List<ScreenRemovalBlock>();
    private Texture2D fillTexture;
    private float totalScreenArea;

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
        }
    }

    public void RemoveRandomBlock()
    {
        float targetAreaMin = totalScreenArea * (minBlockSizePercent / 100f);
        float targetAreaMax = totalScreenArea * (maxBlockSizePercent / 100f);
        
        // Try to generate a block that covers at least minBlockSizePercent of screen (accounting for overlaps)
        Rect newBlock = Rect.zero;
        int attempts = 0;
        
        while (attempts < maxAttempts)
        {
            newBlock = GenerateRandomBlock(targetAreaMin, targetAreaMax);
            float nonOverlappingArea = CalculateNonOverlappingArea(newBlock);
            
            // Check if the non-overlapping area meets the minimum requirement
            if (nonOverlappingArea >= targetAreaMin)
            {
                AddBlock(newBlock);
                return;
            }
            
            attempts++;
        }
        
        // If we couldn't find a suitable position after max attempts, add it anyway
        Debug.LogWarning($"Could not find a position where block covers {minBlockSizePercent}% non-overlapping area after {maxAttempts} attempts. Adding block anyway.");
        AddBlock(newBlock);
    }

    private Rect GenerateRandomBlock(float minArea, float maxArea)
    {
        // Generate a random size within the acceptable range
        float targetArea = Random.Range(minArea, maxArea);
        
        // Random aspect ratio between 0.5 and 2.0
        float aspectRatio = Random.Range(0.5f, 2f);
        
        // Calculate width and height based on target area and aspect ratio
        float height = Mathf.Sqrt(targetArea / aspectRatio);
        float width = targetArea / height;
        
        // Clamp to screen bounds
        width = Mathf.Min(width, Screen.width);
        height = Mathf.Min(height, Screen.height);
        
        // Random position, ensuring block stays within screen
        float x = Random.Range(0, Screen.width - width);
        float y = Random.Range(0, Screen.height - height);
        
        return new Rect(x, y, width, height);
    }

    public void AddBlock(Rect rect)
    {
        Color randomColor = blockColors[Random.Range(0, blockColors.Length)];
        ScreenRemovalBlock block = new ScreenRemovalBlock(rect, randomColor);
        removedBlocks.Add(block);
        
        Debug.Log($"Added new block at ({rect.x}, {rect.y}) with size {rect.width}x{rect.height}. Total blocks: {removedBlocks.Count}");
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

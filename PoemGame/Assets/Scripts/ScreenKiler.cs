using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ScreenRemovalBlock
{
    public Rect rect;
    public Color color;
    public GameObject blockObject;

    public ScreenRemovalBlock(Rect rect, Color color, GameObject blockObject)
    {
        this.rect = rect;
        this.color = color;
        this.blockObject = blockObject;
    }
}

public class ScreenKiler : MonoBehaviour
{
    public static ScreenKiler Instance { get; private set; }

    [SerializeField]
    private bool dontDestroyOnLoad = true;

    [SerializeField]
    private Transform blocksParent;

    [SerializeField]
    private GameObject blockPrefab;

    [SerializeField]
    private Camera blockCamera;

    [SerializeField]
    private float blockZ = 0f;

    private List<ScreenRemovalBlock> removedBlocks = new List<ScreenRemovalBlock>();
    private List<Vector2> uncoveredPoints = new List<Vector2>();
    private float totalScreenArea;

    [SerializeField]
    private int pointSamplingStep = 5; // Sample points every N pixels for efficiency
    [SerializeField]
    private float minSizePercentage = 0.1f; // Minimum size of a block as a percentage of the screen area

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
        totalScreenArea = Screen.width * Screen.height;

        if (blockCamera == null)
        {
            blockCamera = Camera.main;
        }

        if (blockCamera == null)
        {
            Debug.LogWarning("ScreenKiler: No camera assigned for block placement. Assign blockCamera in the Inspector.");
        }

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
        if (uncoveredPoints.Count == 0)
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

        // // Generate a block centered on this position that covers at least 10% non-overlapping area
        // float targetArea = totalScreenArea * 0.1f; // 10% of screen
        Rect blockAroundPosition = Rect.zero;
        
        blockAroundPosition = GenerateBlockAroundPosition(uncoveredPosition);
        AddBlock(blockAroundPosition);
        return;
        
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
        // Block should cover at least minSizePercentage of total screen area (with some variation)
        float targetArea = totalScreenArea * Random.Range(minSizePercentage, minSizePercentage*2); 
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
        Color blockColor = blockColors[Random.Range(0, blockColors.Length)];
        GameObject blockObject = CreateBlockObject(rect, blockColor);
        ScreenRemovalBlock block = new ScreenRemovalBlock(rect, blockColor, blockObject);
        removedBlocks.Add(block);
        
        // Remove all points covered by this new block
        uncoveredPoints.RemoveAll(point => rect.Contains(point));
        
        Debug.Log($"Added new block at ({rect.x}, {rect.y}) with size {rect.width}x{rect.height}. Total blocks: {removedBlocks.Count}. Remaining uncovered points: {uncoveredPoints.Count}. Coverage: {GetTotalRemovedScreenPercentage():F1}%");
    }

    private GameObject CreateBlockObject(Rect screenRect, Color color)
    {
        if (blockPrefab == null)
        {
            Debug.LogWarning("ScreenKiler: blockPrefab is not assigned. Cannot create block object.");
            return null;
        }

        if (blocksParent == null)
        {
            Debug.LogWarning("ScreenKiler: blocksParent is not assigned. Cannot parent block object.");
            return null;
        }

        if (blockCamera == null)
        {
            Debug.LogWarning("ScreenKiler: blockCamera is not assigned. Cannot convert screen to world position.");
            return null;
        }

        float zDistance = Mathf.Abs(blockCamera.transform.position.z - blockZ);
        Vector3 worldMin = blockCamera.ScreenToWorldPoint(new Vector3(screenRect.xMin, screenRect.yMin, zDistance));
        Vector3 worldMax = blockCamera.ScreenToWorldPoint(new Vector3(screenRect.xMax, screenRect.yMax, zDistance));
        Vector3 worldCenter = (worldMin + worldMax) * 0.5f;
        Vector3 worldSize = new Vector3(Mathf.Abs(worldMax.x - worldMin.x), Mathf.Abs(worldMax.y - worldMin.y), 1f);

        GameObject go = Instantiate(blockPrefab, blocksParent);
        go.transform.position = new Vector3(worldCenter.x, worldCenter.y, blockZ);

        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = color;
            Vector2 spriteWorldSize = sr.bounds.size;
            if (spriteWorldSize.x > 0 && spriteWorldSize.y > 0)
            {
                go.transform.localScale = new Vector3(worldSize.x / spriteWorldSize.x, worldSize.y / spriteWorldSize.y, 1f);
            }
        }
        else
        {
            go.transform.localScale = new Vector3(worldSize.x, worldSize.y, 1f);
        }

        return go;
    }

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

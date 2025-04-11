using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnhancedMeshGenerator : MonoBehaviour
{
    // ------------------------------------------------------------
    // Public Variables (Inspector Organization)
    // ------------------------------------------------------------
    [Header("Material & Instance Settings")]
    public Material material;
    public int instanceCount = 100;

    [Header("Powerup Settings")]
    public Material powerupMaterial;          // Unique material for the powerup cone.
    public Vector3 powerupScale = new Vector3(1f, 1f, 1f); // Desired scale for the powerup.

    [Header("Cube Dimensions (Player & Boxes)")]
    public float width = 1f;
    public float height = 1f;
    public float depth = 1f;

    [Header("Movement & Physics Settings")]
    public float movementSpeed = 5f;
    public float gravity = 9.8f;
    public float jumpForce = 15f;
    public float riseMultiplier = 1f;
    public float fallMultiplier = 0.5f;

    [Header("Fireball & Powerup Variables")]
    public GameObject fireballPrefab;
    public int playerLives = 3;
    public float invincibilityDuration = 5f;

    [Header("Camera Reference")]
    public PlayerCameraFollow cameraFollow;

    [Header("Positions & Random Box Generation Settings")]
    public float constantZPosition = 0f;
    public float minX = -50f;
    public float maxX = 50f;
    public float minY = -50f;
    public float maxY = 50f;

    [Header("Ground Settings")]
    public float groundY = -20f;
    public float groundWidth = 200f;
    public float groundDepth = 200f;

    [Header("Spawner Settings")]
    public int numPowerups = 3;   // Number of powerups to spawn
    public int numEnemies = 3;    // Number of enemies to spawn

    // ------------------------------------------------------------
    // Private Variables
    // ------------------------------------------------------------
    private Mesh cubeMesh;
    private List<Matrix4x4> matrices = new List<Matrix4x4>();
    private List<int> colliderIds = new List<int>();

    private int playerID = -1;
    private Vector3 playerVelocity = Vector3.zero;
    private bool isGrounded = false;
    private bool playerInvincible = false; // for invincibility state

    private List<int> enemyIDs = new List<int>();
    private Vector3 currentPlayerPos;       // To store the player's position

    // ------------------------------------------------------------
    // Unity Methods
    // ------------------------------------------------------------
    void Start()
    {
        SetupCamera();
        CreateCubeMesh();
        CreatePlayer();
        CreateGround();
        GenerateRandomBoxes();

        // Spawn multiple enemies and powerups
        GenerateRandomEnemies();
        GenerateRandomPowerups();
    }

    void Update()
    {
        UpdatePlayer();
        RenderBoxes();
    }

    // ------------------------------------------------------------
    // Initialization Methods
    // ------------------------------------------------------------
    void SetupCamera()
    {
        if (cameraFollow == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cameraFollow = mainCamera.GetComponent<PlayerCameraFollow>();
                if (cameraFollow == null)
                {
                    cameraFollow = mainCamera.gameObject.AddComponent<PlayerCameraFollow>();
                }
            }
            else
            {
                GameObject cameraObj = new GameObject("PlayerCamera");
                Camera cam = cameraObj.AddComponent<Camera>();
                cameraFollow = cameraObj.AddComponent<PlayerCameraFollow>();
                cam.tag = "MainCamera";
            }
            cameraFollow.offset = new Vector3(0, 0, -15);
            cameraFollow.smoothSpeed = 0.1f;
        }
    }

    void CreateCubeMesh()
    {
        cubeMesh = new Mesh();
        Vector3[] vertices = new Vector3[8]
        {
            new Vector3(0, 0, 0),
            new Vector3(width, 0, 0),
            new Vector3(width, 0, depth),
            new Vector3(0, 0, depth),
            new Vector3(0, height, 0),
            new Vector3(width, height, 0),
            new Vector3(width, height, depth),
            new Vector3(0, height, depth)
        };

        int[] triangles = new int[36]
        {
            // Front face
            0, 4, 1,
            1, 4, 5,
            // Back face
            2, 6, 3,
            3, 6, 7,
            // Left face
            0, 3, 4,
            4, 3, 7,
            // Right face
            1, 5, 2,
            2, 5, 6,
            // Bottom face
            0, 1, 3,
            3, 1, 2,
            // Top face
            4, 7, 5,
            5, 7, 6
        };

        Vector2[] uvs = new Vector2[8];
        for (int i = 0; i < 8; i++)
        {
            uvs[i] = new Vector2(vertices[i].x / width, vertices[i].z / depth);
        }

        cubeMesh.vertices = vertices;
        cubeMesh.triangles = triangles;
        cubeMesh.uv = uvs;
        cubeMesh.RecalculateNormals();
        cubeMesh.RecalculateBounds();
    }

    void CreatePlayer()
    {
        Vector3 playerPosition = new Vector3(0, 10, constantZPosition);
        Vector3 playerScale = Vector3.one;
        Quaternion playerRotation = Quaternion.identity;

        // Create a dedicated GameObject for the player
        GameObject playerObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        playerObj.name = "Player";
        playerObj.transform.position = playerPosition;
        playerObj.transform.localScale = playerScale;

        MeshRenderer renderer = playerObj.GetComponent<MeshRenderer>();
        if (renderer != null)
            renderer.material = material;

        // Attach the PlayerController script (ensure it is defined)
        playerObj.AddComponent<PlayerController>();
        playerObj.tag = "Player";

        playerID = CollisionManager.Instance.RegisterCollider(
            playerPosition,
            new Vector3(width * playerScale.x, height * playerScale.y, depth * playerScale.z),
            true);

        Matrix4x4 playerMatrix = Matrix4x4.TRS(playerPosition, playerRotation, playerScale);
        matrices.Add(playerMatrix);
        colliderIds.Add(playerID);
        CollisionManager.Instance.UpdateMatrix(playerID, playerMatrix);
    }

    void CreateGround()
    {
        Vector3 groundPosition = new Vector3(0, groundY, constantZPosition);
        Vector3 groundScale = new Vector3(groundWidth, 1f, groundDepth);
        Quaternion groundRotation = Quaternion.identity;

        int groundID = CollisionManager.Instance.RegisterCollider(
            groundPosition,
            new Vector3(groundWidth, 1f, groundDepth),
            false);

        Matrix4x4 groundMatrix = Matrix4x4.TRS(groundPosition, groundRotation, groundScale);
        matrices.Add(groundMatrix);
        colliderIds.Add(groundID);
        CollisionManager.Instance.UpdateMatrix(groundID, groundMatrix);
    }

    void GenerateRandomBoxes()
    {
        List<Rect> placedObstacles = new List<Rect>();
        int obstaclesToSpawn = instanceCount - 2;
        int obstacleCount = 0;
        int maxGlobalAttempts = obstaclesToSpawn * 10;
        int globalAttempts = 0;

        while (obstacleCount < obstaclesToSpawn && globalAttempts < maxGlobalAttempts)
        {
            globalAttempts++;
            float obstacleX = Random.Range(minX, maxX);
            Vector3 scale = new Vector3(
                Random.Range(0.5f, 3f),
                Random.Range(0.5f, 3f),
                Random.Range(0.5f, 3f)
            );

            int angleIndex = Random.Range(0, 4);
            Quaternion rotation = Quaternion.Euler(0, 0, 90 * angleIndex);

            float obstacleY = groundY + 0.5f + (height * scale.y) / 2f;
            Vector3 position = new Vector3(obstacleX, obstacleY, constantZPosition);

            float boxWidth = width * scale.x;
            float boxHeight = height * scale.y;
            Rect newObstacleRect = new Rect(obstacleX - boxWidth / 2f, obstacleY - boxHeight / 2f, boxWidth, boxHeight);

            bool validPlacement = true;
            foreach (Rect placed in placedObstacles)
            {
                if (newObstacleRect.Overlaps(placed))
                {
                    validPlacement = false;
                    break;
                }
            }

            if (validPlacement)
            {
                placedObstacles.Add(newObstacleRect);
                int id = CollisionManager.Instance.RegisterCollider(
                    position,
                    new Vector3(width * scale.x, height * scale.y, depth * scale.z),
                    false
                );

                Matrix4x4 boxMatrix = Matrix4x4.TRS(position, rotation, scale);
                matrices.Add(boxMatrix);
                colliderIds.Add(id);
                CollisionManager.Instance.UpdateMatrix(id, boxMatrix);

                obstacleCount++;
            }
        }
    }

    // ------------------------------------------------------------
    // Spawning Methods for Enemies and Powerups
    // ------------------------------------------------------------
    void GenerateRandomEnemies()
    {
        for (int i = 0; i < numEnemies; i++)
        {
            float x = Random.Range(minX, maxX);
            float y = groundY + 1f; // spawn enemy slightly above ground
            Vector3 spawnPos = new Vector3(x, y, constantZPosition);
            SpawnEnemy(spawnPos);
        }
    }

    void GenerateRandomPowerups()
    {
        for (int i = 0; i < numPowerups; i++)
        {
            float x = Random.Range(minX, maxX);
            float y = groundY + 0.5f + (powerupScale.y / 2f);
            Vector3 spawnPos = new Vector3(x, y, constantZPosition);
            CreatePowerup(spawnPos);
        }
    }

    // ------------------------------------------------------------
    // Powerup Creation Method (Creates a Cone Powerup)
    // ------------------------------------------------------------
    public void CreatePowerup(Vector3 position)
    {
        GameObject powerupObj = new GameObject("Powerup");
        powerupObj.transform.position = position;
        powerupObj.transform.localScale = powerupScale;

        MeshFilter meshFilter = powerupObj.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateConeMesh(1f, 0.5f, 20);

        MeshRenderer meshRenderer = powerupObj.AddComponent<MeshRenderer>();
        if (powerupMaterial != null)
            meshRenderer.material = powerupMaterial;

        MeshCollider collider = powerupObj.AddComponent<MeshCollider>();
        collider.sharedMesh = meshFilter.mesh;
        collider.convex = true;
        collider.isTrigger = true;

        Rigidbody rb = powerupObj.AddComponent<Rigidbody>();
        rb.isKinematic = true;

        powerupObj.AddComponent<Powerup>();
    }

    // ------------------------------------------------------------
    // Example Cone Mesh Generator
    // ------------------------------------------------------------
    public static Mesh CreateConeMesh(float height, float bottomRadius, int segments)
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[segments + 2];
        vertices[0] = new Vector3(0, height, 0);
        float angleStep = 2 * Mathf.PI / segments;
        for (int i = 0; i < segments; i++)
        {
            float angle = i * angleStep;
            vertices[i + 1] = new Vector3(Mathf.Cos(angle) * bottomRadius, 0, Mathf.Sin(angle) * bottomRadius);
        }
        vertices[segments + 1] = Vector3.zero;

        int sideTriangleCount = segments;
        int baseTriangleCount = segments;
        int[] triangles = new int[(sideTriangleCount + baseTriangleCount) * 3];
        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = next + 1;
        }
        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;
            int baseCenterIndex = segments + 1;
            int tIndex = segments * 3 + i * 3;
            triangles[tIndex] = baseCenterIndex;
            triangles[tIndex + 1] = next + 1;
            triangles[tIndex + 2] = i + 1;
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    // ------------------------------------------------------------
    // Enemy Methods
    // ------------------------------------------------------------
    public void SpawnEnemy(Vector3 spawnPosition)
    {
        GameObject enemyObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        enemyObj.name = "Enemy";
        enemyObj.transform.position = spawnPosition;
        enemyObj.transform.localScale = Vector3.one;
        enemyObj.AddComponent<Enemy>();  // Ensure Enemy.cs is present
        int enemyID = CollisionManager.Instance.RegisterCollider(
            spawnPosition,
            new Vector3(width, height, depth),
            false
        );
        enemyIDs.Add(enemyID);
    }

    // ------------------------------------------------------------
    // Player Update & Collision Handling
    // ------------------------------------------------------------
    void UpdatePlayer()
    {
        if (playerID == -1)
            return;

        Matrix4x4 playerMatrix = matrices[colliderIds.IndexOf(playerID)];
        DecomposeMatrix(playerMatrix, out Vector3 pos, out Quaternion rot, out Vector3 scale);

        if (isGrounded)
        {
            playerVelocity.y = 0;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                playerVelocity.y = jumpForce;
                isGrounded = false;
            }
        }

        if (!isGrounded)
        {
            float multiplier = (playerVelocity.y > 0) ? riseMultiplier : fallMultiplier;
            playerVelocity.y -= gravity * multiplier * Time.deltaTime;
        }

        float horizontal = 0;
        if (Input.GetKey(KeyCode.A)) horizontal -= 1;
        if (Input.GetKey(KeyCode.D)) horizontal += 1;
        float effectiveSpeed = movementSpeed;
        if (!isGrounded) effectiveSpeed *= 0.5f;
        Vector3 newPos = pos;
        newPos.x += horizontal * effectiveSpeed * Time.deltaTime;

        if (!CheckCollisionAt(playerID, new Vector3(newPos.x, pos.y, pos.z)))
            pos.x = newPos.x;

        newPos = pos;
        newPos.y += playerVelocity.y * Time.deltaTime;
        if (CheckCollisionAt(playerID, new Vector3(pos.x, newPos.y, pos.z)))
        {
            if (playerVelocity.y < 0)
                isGrounded = true;
            playerVelocity.y = 0;
        }
        else
        {
            pos.y = newPos.y;
            isGrounded = false;
        }

        Matrix4x4 newMatrix = Matrix4x4.TRS(pos, rot, scale);
        matrices[colliderIds.IndexOf(playerID)] = newMatrix;
        CollisionManager.Instance.UpdateCollider(
            playerID,
            pos,
            new Vector3(width * scale.x, height * scale.y, depth * scale.z)
        );
        CollisionManager.Instance.UpdateMatrix(playerID, newMatrix);

        currentPlayerPos = pos;

        if (cameraFollow != null)
            cameraFollow.SetPlayerPosition(pos);
    }

    bool CheckCollisionAt(int id, Vector3 position)
    {
        return CollisionManager.Instance.CheckCollision(id, position, out _);
    }

    // ------------------------------------------------------------
    // Rendering Methods (with scale-zero for objects outside camera view)
    // ------------------------------------------------------------
    void RenderBoxes()
    {
        List<Matrix4x4> displayMatrices = new List<Matrix4x4>();
        Camera cam = Camera.main;
        Vector3 camPos = cam.transform.position;
        Vector3 camForward = cam.transform.forward;
        foreach (Matrix4x4 mat in matrices)
        {
            Vector3 pos = mat.GetColumn(3);
            Vector3 toObj = (pos - camPos).normalized;
            if (Vector3.Dot(camForward, toObj) < 0f)
            {
                // Object is behind the camera; hide it by setting its scale to zero.
                Matrix4x4 modMat = Matrix4x4.TRS(pos, Quaternion.LookRotation(mat.GetColumn(2)), Vector3.zero);
                displayMatrices.Add(modMat);
            }
            else
            {
                displayMatrices.Add(mat);
            }
        }
        Matrix4x4[] matrixArray = displayMatrices.ToArray();
        for (int i = 0; i < matrixArray.Length; i += 1023)
        {
            int batchSize = Mathf.Min(1023, matrixArray.Length - i);
            Matrix4x4[] batchMatrices = new Matrix4x4[batchSize];
            System.Array.Copy(matrixArray, i, batchMatrices, 0, batchSize);
            Graphics.DrawMeshInstanced(cubeMesh, 0, material, batchMatrices, batchSize);
        }
    }

    // ------------------------------------------------------------
    // Utility Methods
    // ------------------------------------------------------------
    // Decomposes a Matrix4x4 into position, rotation, and scale components.
    void DecomposeMatrix(Matrix4x4 m, out Vector3 position, out Quaternion rotation, out Vector3 scale)
    {
        // Extract position from the matrix
        position = new Vector3(m.m03, m.m13, m.m23);
        // Extract scale by calculating the magnitude of the basis vectors
        scale = new Vector3(
            new Vector3(m.m00, m.m10, m.m20).magnitude,
            new Vector3(m.m01, m.m11, m.m21).magnitude,
            new Vector3(m.m02, m.m12, m.m22).magnitude
        );
        // Remove scale from the basis vectors to extract rotation
        Vector3 forward = new Vector3(m.m02, m.m12, m.m22);
        Vector3 upwards = new Vector3(m.m01, m.m11, m.m21);
        if (scale.x != 0)
            forward /= scale.x;
        if (scale.y != 0)
            upwards /= scale.y;
        rotation = Quaternion.LookRotation(forward, upwards);
    }

    // ------------------------------------------------------------
    // Additional Utility Methods
    // ------------------------------------------------------------
    public void AddRandomBox()
    {
        Vector3 position = new Vector3(
            Random.Range(minX, maxX),
            Random.Range(minY, maxY),
            constantZPosition
        );
        Quaternion rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        Vector3 scale = new Vector3(
            Random.Range(0.5f, 3f),
            Random.Range(0.5f, 3f),
            Random.Range(0.5f, 3f)
        );
        int id = CollisionManager.Instance.RegisterCollider(
            position,
            new Vector3(width * scale.x, height * scale.y, depth * scale.z),
            false
        );
        Matrix4x4 boxMatrix = Matrix4x4.TRS(position, rotation, scale);
        matrices.Add(boxMatrix);
        colliderIds.Add(id);
        CollisionManager.Instance.UpdateMatrix(id, boxMatrix);
    }

    // ------------------------------------------------------------
    // Powerup Methods (fireball no longer granted by F-key)
    // ------------------------------------------------------------
    public void ActivateFireballPowerup(Vector3 spawnPosition)
    {
        playerLives++;
        StartCoroutine(ActivateInvincibility(invincibilityDuration));
        if (fireballPrefab != null)
            Instantiate(fireballPrefab, spawnPosition, Quaternion.identity);
    }

    private IEnumerator ActivateInvincibility(float duration)
    {
        playerInvincible = true;
        yield return new WaitForSeconds(duration);
        playerInvincible = false;
    }
}

using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.Barracuda;

public class MoveToGoal : Agent
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material loseMaterial;
    [SerializeField] private MeshRenderer floorMeshRenderer;
    private Rigidbody rb;
    private float startTime;
    private float totalDistanceMoved;
    private Vector3 lastPosition;
    private float moveSpeed = 5f;
    public int stage = 1;
    private int episodesTrained = 0;



    private string pythonScriptPath = @"C:\FINAL YEAR PROJECT\AI GAME\Game\VISE_GamePlay\train_npc.py";

    [System.Serializable]
    public class GameplayLog
    {
        public int episodeNumber;
        public float timeTaken;
        public float totalReward;
        public bool npcWon;
        public float totalDistanceMoved;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        LoadLatestModel(); // Load latest model at start
    }

    public override void OnEpisodeBegin()
    {
        episodesTrained++;
        transform.localPosition = new Vector3(26f, 0, -40f);
        SetEnvironmentStage(stage);
        startTime = Time.time;
        totalDistanceMoved = 0f;
        lastPosition = transform.localPosition;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition / 20f); // Adjust denominator to match your world scale
        sensor.AddObservation(targetTransform.localPosition / 20f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        Vector3 previousPosition = transform.localPosition;
        Vector3 movement = new Vector3(moveX, 0, moveZ) * moveSpeed;
        rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);

        Vector3 currentPosition = transform.localPosition;
        float currentDistance = Vector3.Distance(currentPosition, targetTransform.localPosition);
        float previousDistance = Vector3.Distance(previousPosition, targetTransform.localPosition);

        if (currentDistance < previousDistance)
            AddReward(0.7f);
        else
            AddReward(-0.01f);

        if (currentDistance < 3f)
            AddReward(0.3f / (currentDistance + 0.1f));

        if (Mathf.Abs(moveX) < 0.01f && Mathf.Abs(moveZ) < 0.01f)
            AddReward(-0.05f);

        AddReward(-0.0005f);

        if (IsOutOfBounds(currentPosition))
        {
            AddReward(-1f);
            floorMeshRenderer.material = loseMaterial;
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }

    private void SetEnvironmentStage(int currentStage)
    {
        switch (currentStage)
        {
            case 1: moveSpeed = 5f; break;
            case 2: moveSpeed = 4f; break;
            case 3: moveSpeed = 3f; break;
        }
    }

    private void Update()
    {
        totalDistanceMoved += Vector3.Distance(transform.localPosition, lastPosition);
        lastPosition = transform.localPosition;

        AdjustDifficulty();
    }

    private void AdjustDifficulty()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "GameplayLogs.json");
        if (!File.Exists(filePath)) return;

        string jsonData = File.ReadAllText(filePath);
        Wrapper wrapper = JsonUtility.FromJson<Wrapper>(jsonData);
        List<GameplayLog> logs = wrapper.logs;

        if (logs.Count >= 10)
        {
            float avgReward = 0;
            for (int i = logs.Count - 10; i < logs.Count; i++)
            {
                avgReward += logs[i].totalReward;
            }
            avgReward /= 10;

            if (avgReward > 50) stage = 2;
            if (avgReward > 100) stage = 3;
        }
    }

    private bool IsOutOfBounds(Vector3 position)
    {
        return position.x < -36.1f || position.x > 36.7f || position.z < -46.8f || position.z > 45.6f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            float timeTaken = Time.time - startTime;
            float efficiencyBonus = Mathf.Clamp01(1f - timeTaken / 100f);
            SetReward(+5f + efficiencyBonus);
            floorMeshRenderer.material = winMaterial;
            SaveGameplayData(true, timeTaken);
            EndEpisodeWithUpdate();
        }
        else if (other.CompareTag("Obstacle"))
        {
            SetReward(-0.5f);
            floorMeshRenderer.material = loseMaterial;
            SaveGameplayData(false, Time.time - startTime);
            EndEpisodeWithUpdate();
        }
        else if (other.CompareTag("Wall"))
        {
            AddReward(-0.5f);
        }
    }

    private void SaveGameplayData(bool npcWon, float timeTaken)
    {
        GameplayLog log = new GameplayLog
        {
            episodeNumber = episodesTrained,
            timeTaken = timeTaken,
            totalReward = GetCumulativeReward(),
            npcWon = npcWon,
            totalDistanceMoved = totalDistanceMoved
        };

        string filePath = Path.Combine(Application.persistentDataPath, "GameplayLogs.json");

        List<GameplayLog> logs = new List<GameplayLog>();

        if (File.Exists(filePath))
        {
            string existingJson = File.ReadAllText(filePath);
            logs = JsonUtility.FromJson<Wrapper>(existingJson).logs;
        }

        logs.Add(log);

        Wrapper wrapper = new Wrapper { logs = logs };
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(filePath, json, Encoding.UTF8);

        UnityEngine.Debug.Log("Game data saved: " + json);
    }

    private void EndEpisodeWithUpdate()
    {
        EndEpisode();
        StartCoroutine(RetrainModel()); // Use Coroutine instead of blocking thread
    }

    private IEnumerator RetrainModel()
    {
        if (File.Exists(pythonScriptPath))
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = "\"" + pythonScriptPath + "\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            Process process = Process.Start(psi);
            UnityEngine.Debug.Log("Training started...");

            while (!process.StandardOutput.EndOfStream)
            {
                string outputLine = process.StandardOutput.ReadLine();
                if (!string.IsNullOrEmpty(outputLine))
                {
                    UnityEngine.Debug.Log(outputLine);
                }
            }

            process.WaitForExit();
            UnityEngine.Debug.Log("Training completed. Checking model update...");

            float timeout = 15f; // max seconds to wait
            float timer = 0f;
            string modelPath = Path.Combine(Application.dataPath, "Resources/models/MoveToGoal-12828373.nn");

            while (!File.Exists(modelPath) && timer < timeout)
            {
                yield return new WaitForSeconds(1f);
                timer += 1f;
            }

            if (File.Exists(modelPath))
            {
                LoadLatestModel();
            }
            else
            {
                UnityEngine.Debug.LogError("Model update failed: No new model file found.");
            }
        }
        else
        {
            UnityEngine.Debug.LogError("Python script not found: " + pythonScriptPath);
        }
    }

    private void LoadLatestModel()
    {
        BehaviorParameters behavior = GetComponent<BehaviorParameters>();

        // Unload the previous model before loading a new one
        if (behavior.Model != null)
        {
            Resources.UnloadAsset(behavior.Model);
        }

        NNModel newModel = Resources.Load<NNModel>("models/MoveToGoal-12828373");

        if (newModel != null)
        {
            behavior.Model = newModel;
            UnityEngine.Debug.Log("NPC updated with latest model!");
        }
        else
        {
            UnityEngine.Debug.LogError("Failed to load NPC model. Ensure it's in 'Assets/Resources/models/' and named 'MoveToGoal-xyz.nn'.");
        }
    }


    [System.Serializable]
    private class Wrapper
    {
        public List<GameplayLog> logs;
    }
}
using GorillaTag.Gravity;
using GTModTemplate.Classes;
using GTModTemplate.Patches;
using GTModTemplate.Utilities;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GTModTemplate;

public class Main : MonoBehaviour
{
    internal class RandomUtils // My utility class for random utilities (great name right?) This mod uses bingus's Mod Template
    {
        public static GameObject? FindInactiveObject(string objectName) // Look for inactive objects in the game since GameObject.Find cannot find them
        {
            GameObject[] all = Resources.FindObjectsOfTypeAll<GameObject>();
            List<GameObject> objs = new List<GameObject>();
            foreach (GameObject obj in all)
            {
                if (obj.name == objectName) return obj;
            }
            return null;
        }
        public static List<GameObject> FindChildrenContaining(Transform parent, string searchText, bool caseSensitive = false) // Find objects containing text in their name
        {
            List<GameObject> results = new List<GameObject>();
            string search = caseSensitive ? searchText : searchText.ToLower();

            foreach (Transform child in parent.GetComponentsInChildren<Transform>(true)) // true = include inactive
            {
                string childName = caseSensitive ? child.name : child.name.ToLower();
                if (childName.Contains(search))
                    results.Add(child.gameObject);
            }
            return results;
        }
    }
    public static Main? Instance;

    // This is a log, used for writing information for debug purposes
    public GorillaLog Log = new();

    // Mod variables
    private GameObject? babyParent;
    private float retryTimer = 0f;
    private int babyCount = 5;
    private float plusHoldTimer = 0f;
    private float minusHoldTimer = 0f;
    private int findRetries = 0;

    // This is called when the mod initializes
    private void Start()
    {
        Instance = this;

        HarmonyPatches.Patch(); // Patch the game
        Config.Load(); // Load configuration data
        Application.quitting += Config.Save;  // Save configuration on exit

        // Stops the OnPlayerSpawned method from creating unhandled errors, so other mods
        // can still work even if yours breaks.
        GorillaTagger.OnPlayerSpawned(() => MethodUtilities.Attempt(OnPlayerSpawned));
    }
    private void Update()
    {
        if (Keyboard.current.gKey.wasPressedThisFrame && babyParent != null) // toggle babies visibility with G key
        {
            babyParent.SetActive(!babyParent.activeSelf);
            VRRig.LocalRig.PlayHandTapLocal(67, false, 0.1f);
        }
        if (Keyboard.current.equalsKey.wasPressedThisFrame || (Time.time > plusHoldTimer && Keyboard.current.equalsKey.isPressed)) // create a baby with = key
        {
            if (babyParent != null)
            {
                var singularBaby = RandomUtils.FindChildrenContaining(babyParent.transform, "BabyOffset", false);
                if (singularBaby[Random.Range(0, singularBaby.Count)] != null)
                {
                    var clonedBaby = Instantiate(singularBaby[0], babyParent.transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)), Quaternion.identity);
                    clonedBaby.transform.SetParent(babyParent.transform);
                    babyCount += 1;
                    VRRig.LocalRig.PlayHandTapLocal(67, false, 0.1f);
                    plusHoldTimer = Time.time + 0.1f;
                    Log.WriteLine("Spawned a bug baby!");
                }
                else
                {
                    Log.WriteLine("Could not find a baby to clone!");
                }
            }
            else
            {
                Log.WriteLine("BabyParent is nil!");
            }
        }
        if (Keyboard.current.minusKey.wasPressedThisFrame || (Time.time > minusHoldTimer && Keyboard.current.minusKey.isPressed)) // destroy a baby with - key
        {
            if (babyParent != null)
            {
                var singularBaby = RandomUtils.FindChildrenContaining(babyParent.transform, "BabyOffset", false);
                if (singularBaby[Random.Range(0, singularBaby.Count)] != null && singularBaby.Count > 1)
                {
                    Destroy(singularBaby[0]);
                    babyCount -= 1;
                    VRRig.LocalRig.PlayHandTapLocal(67, false, 0.1f);
                    minusHoldTimer = Time.time + 0.1f;
                    Log.WriteLine("Destroyed a bug baby!");
                }
                else
                {
                    Log.WriteLine("Could not find a baby to destroy!");
                }
            }
            else
            {
                Log.WriteLine("BabyParent is nil!");
            }
        }
        if (Time.time > retryTimer && babyParent == null && findRetries < 40) // Retries over and over again until it finds the babies or gives up after 40 attempts
        {
            retryTimer = Time.time + 2f;
            findRetries += 1;
            attemptLoadBabies();
        }
    }
    private void attemptLoadBabies() // Attempts to find the BugBabies object in the scene and set it to active
    {
        var bugBabies = RandomUtils.FindInactiveObject("BugBabies");
        if (bugBabies != null)
        {
            bugBabies.SetActive(true);
            babyParent = bugBabies;
            Log.WriteLine("Found BugBabies object and set it to active!");
        }
        else
        {
            Log.WriteLine("Could not find BugBabies object!");
        }
    }
    // This is called when everything is ready in the game before the gorilla is spawned into the world.
    private void OnPlayerSpawned()
    {
        // Write some text to the log file
        Log.WriteLine("Bringing back bug babies");
    }
    private GUIStyle txtStyle = new GUIStyle();
    void OnGUI() // setup the pcgui for the baby counter
    {
        GUI.Label(new Rect(0, 0, 380, 80), $"Bug Babies: {babyCount}", txtStyle);
        txtStyle.fontSize = 30;
        txtStyle.fontStyle = FontStyle.Bold;
        txtStyle.alignment = TextAnchor.MiddleCenter;
        txtStyle.normal.textColor = Color.white;
    }
}

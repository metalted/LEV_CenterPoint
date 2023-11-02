using BepInEx;
using BepInEx.Configuration;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyLib;
using ZeepSDK.LevelEditor;
using System.Linq;

namespace LEV_CenterPoint
{
    [BepInPlugin(pluginGUID, pluginName, pluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string pluginGUID = "com.metalted.zeepkist.lev_centerpoint";
        public const string pluginName = "Level Editor Center Point";
        public const string pluginVersion = "1.0";
        public static ConfigFile CFG;
        public static bool inEditor = false;
        public List<GameObject> currentCenterPoints = new List<GameObject>();
        public ConfigEntry<KeyCode> addKey;
        public ConfigEntry<KeyCode> removeKey;

        private void Awake()
        {
            CFG = Config;
            // Plugin startup logic
            Logger.LogInfo($"Plugin {pluginGUID} is loaded!");
            Harmony harmony = new Harmony(pluginGUID);
            harmony.PatchAll();

            LevelEditorApi.EnteredLevelEditor += () => { inEditor = true; };
            LevelEditorApi.ExitedLevelEditor += () => { inEditor = false; };

            addKey = Config.Bind("Controls", "Create Centerpoints", KeyCode.Keypad2, "");
            removeKey = Config.Bind("Controls", "Remove Centerpoints", KeyCode.Keypad3, "");
        }

        public void Update()
        {
            if (inEditor)
            {
                if(Input.GetKeyDown((KeyCode)removeKey.BoxedValue))
                {
                    RemoveCenterPoints();
                }

                if(Input.GetKeyDown((KeyCode)addKey.BoxedValue))
                {
                    CreateCenterPoints();
                }
            }
        }

        public void RemoveCenterPoints()
        {
            foreach(GameObject obj in currentCenterPoints) 
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }

            currentCenterPoints.Clear();
        }

        public void CreateCenterPoints()
        {
            RemoveCenterPoints();
            GameObject[] allObject = SceneManager.GetActiveScene().GetRootGameObjects();
            List<BlockProperties> allBlocks = new List<BlockProperties>();

            foreach (GameObject obj in allObject)
            {
                if (obj != null)
                {
                    BlockProperties bp = obj.GetComponent<BlockProperties>();
                    if( bp != null )
                    {
                        allBlocks.Add(bp);
                    }
                }
            }

            //Go over each block and find the center
            foreach(BlockProperties bp in allBlocks)
            {
                
                Renderer[] renderers = bp.GetComponentsInChildren<Renderer>();

                if(renderers.Length == 0)
                {
                    continue;
                }

                //Create the bounds
                Bounds bounds = renderers[0].bounds;

                foreach (Renderer renderer in renderers)
                {
                    bounds.Encapsulate(renderer.bounds);
                }

                GameObject centerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                centerSphere.GetComponent<Renderer>().material.color = Color.red;
                GameObject.Destroy(centerSphere.GetComponent<SphereCollider>());
                centerSphere.layer = LayerMask.NameToLayer("Gizmo");
                centerSphere.transform.position = bounds.center;
                centerSphere.transform.parent = bp.gameObject.transform;
                currentCenterPoints.Add(centerSphere);
            }
        }
    }
}

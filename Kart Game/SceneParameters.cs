using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public enum MyGameMode
{
    Laps, Pins
}
public static class SceneParameters
{
    public static MyGameMode gameMode;


    private static GameObject FindInChildrenIncludingInactive(GameObject go, string name)
    {
        for (int i = 0; i < go.transform.childCount; i++) {
            if (go.transform.GetChild(i).gameObject.name == name) return go.transform.GetChild(i).gameObject;
            GameObject found = FindInChildrenIncludingInactive(go.transform.GetChild(i).gameObject, name);
            if (found != null) return found;
        }

        return null; 
    }

    public static GameObject FindIncludingInactive(string name)
    {
        Scene scene = SceneManager.GetActiveScene();
        
        //if (!scene.isLoaded) {
        //    //no scene loaded
        //    return null;
        //}
        //Debug.Log(scene.name);

        var game_objects = new List<GameObject>();
        scene.GetRootGameObjects(game_objects);

        foreach (GameObject obj in game_objects) {
            if (obj.transform.name == name) return obj;

            GameObject found = FindInChildrenIncludingInactive(obj, name);
            if (found) return found;
        }

        return null;
    }
}

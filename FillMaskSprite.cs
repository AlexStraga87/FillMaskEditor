using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System;
using Game;

public class FillMaskSprite : EditorWindow
{
    [MenuItem("Tools/Fill all Sprite Mask")]
    public static void FillAllSpriteMasks()
    {
        List<GameObject> gameObjects = GetAllPrefabs();
        foreach (var gameObject in gameObjects)
        {
            if (FillMask(gameObject))
            {
                EditorUtility.SetDirty(gameObject);
                if (PrefabUtility.GetCorrespondingObjectFromSource(gameObject) != null)
                    PrefabUtility.ApplyPrefabInstance(gameObject, InteractionMode.AutomatedAction);
            }
        }
    }

    // Получение всех префабов в проекте, кроме Variant префабов
    public static List<GameObject> GetAllPrefabs()
    {
        string[] interactionGUIDs = AssetDatabase.FindAssets("t:Prefab");
        List<GameObject> list = new List<GameObject>();
        foreach (string guid2 in interactionGUIDs)
        {
            GameObject gameObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid2)) as GameObject;
            if (PrefabUtility.GetPrefabAssetType(gameObject) != PrefabAssetType.Variant)
                list.Add(gameObject);
        }
        return list;
    }

    // Получаем всех детей и сам корневой gameObject 
    private static List<GameObject> GetAllGameObject(GameObject root)
    {
        List<GameObject> result = new List<GameObject>();
        result.Add(root);
        if (root.transform.childCount > 0)
        {
            foreach (Transform VARIABLE in root.transform)
            {
                Searcher(result, VARIABLE.gameObject);
            }
        }
        return result;
    }

    // Перебираем всех детей
    private static void Searcher(List<GameObject> list, GameObject root)
    {
        list.Add(root);
        if (root.transform.childCount > 0)
        {
            foreach (Transform VARIABLE in root.transform)
            {
                Searcher(list, VARIABLE.gameObject);
            }
        }
    }

    // Заполняем Маски объектам и их детям если они есть
    private static bool FillMask(GameObject gameObject)
    {
        bool isChanged = false;
        List<GameObject> childs = GetAllGameObject(gameObject);

        foreach (var child in childs)
        {
            var spriteMaskChangeRuntime = child.GetComponent<SpriteMaskChangeRuntime>();
            var spriteRenderer = child.GetComponent<SpriteRenderer>();
            var spriteMask = child.GetComponent<SpriteMask>();

            if (spriteMaskChangeRuntime)
            {
                FillMask(spriteMaskChangeRuntime);
                isChanged = true;
            }
            if (spriteRenderer && spriteMask)
            {
                FillMask(spriteRenderer, spriteMask);
                isChanged = true;
            }
        }

        return isChanged;
    }

    private static void FillMask(SpriteMaskChangeRuntime spriteMaskChangeRuntime)
    {
        var serObject = new SerializedObject(spriteMaskChangeRuntime);

        var sprites = serObject.FindProperty("_sprites");

        var mask = serObject.FindProperty("_masks");
        mask.ClearArray();

        var spriteList = new List<Sprite>();
        for (int i = 0; i < sprites.arraySize; i++)
        {
            spriteList.Add((Sprite)sprites.GetArrayElementAtIndex(i).objectReferenceValue);
        }

        for (int i = 0; i < spriteList.Count; i++)
        {
            mask.InsertArrayElementAtIndex(i);
            //if (spriteList[i] != null)
            mask.GetArrayElementAtIndex(i).objectReferenceValue = GetMaskForCurrentSprite(spriteList[i]);
        }

        serObject.ApplyModifiedProperties();
        PrefabUtility.RecordPrefabInstancePropertyModifications(spriteMaskChangeRuntime);
    }

    private static void FillMask(SpriteRenderer spriteRenderer, SpriteMask spriteMask)
    {
        spriteMask.sprite = GetMaskForCurrentSprite(spriteRenderer.sprite);
        PrefabUtility.RecordPrefabInstancePropertyModifications(spriteMask);
    }

    // Основываясь на спрайте ищем ему подходящую маску
    private static Sprite GetMaskForCurrentSprite(Sprite sprite)
    {
        if (sprite == null)
            return null;
        
        if (sprite.name.Contains("_xxx"))
        {
            Debug.LogWarning(sprite.name + " не требуется маска");
            return null;
        }

        string path = AssetDatabase.GetAssetPath(sprite); 
        path = path.Insert(path.Length - 4, "_MASK");
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();

        foreach (var currentSprite in sprites)
        {
            if (currentSprite.name == sprite.name)
            {
                return currentSprite;
            }
        }
        Debug.LogWarning("Не могу найти файл " + path);
        return null;
    }

}


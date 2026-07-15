using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

public static class PretendardFontSetup
{
    private const string SourceFontPath = "Assets/Font/Pretendard-Regular.otf";
    private const string FontAssetSavePath = "Assets/Font/Pretendard-Regular SDF.asset";

    [MenuItem("Tools/ChessGod/Create Pretendard TMP Font Asset")]
    public static void CreateFontAsset()
    {
        Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);
        if (sourceFont == null)
        {
            Debug.LogError($"[PretendardFontSetup] Source font not found at {SourceFontPath}");
            return;
        }

        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
            sourceFont,
            90,
            9,
            GlyphRenderMode.SDFAA,
            1024,
            1024,
            AtlasPopulationMode.Dynamic,
            true);

        if (fontAsset == null)
        {
            Debug.LogError("[PretendardFontSetup] Failed to create TMP_FontAsset.");
            return;
        }

        if (File.Exists(FontAssetSavePath))
        {
            AssetDatabase.DeleteAsset(FontAssetSavePath);
        }

        AssetDatabase.CreateAsset(fontAsset, FontAssetSavePath);
        AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        RegisterFallback(fontAsset);

        Debug.Log("[PretendardFontSetup] Pretendard TMP Font Asset created and registered as fallback.");
    }

    private static void RegisterFallback(TMP_FontAsset fontAsset)
    {
        TMP_FontAsset liberationSans = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (liberationSans != null)
        {
            if (liberationSans.fallbackFontAssetTable == null)
            {
                liberationSans.fallbackFontAssetTable = new List<TMP_FontAsset>();
            }
            if (!liberationSans.fallbackFontAssetTable.Contains(fontAsset))
            {
                liberationSans.fallbackFontAssetTable.Add(fontAsset);
            }
            EditorUtility.SetDirty(liberationSans);
        }
        else
        {
            Debug.LogWarning("[PretendardFontSetup] LiberationSans SDF.asset not found; skipped per-font fallback registration.");
        }

        TMP_Settings settings = TMP_Settings.instance;
        if (settings != null)
        {
            if (TMP_Settings.fallbackFontAssets == null)
            {
                TMP_Settings.fallbackFontAssets = new List<TMP_FontAsset>();
            }
            if (!TMP_Settings.fallbackFontAssets.Contains(fontAsset))
            {
                TMP_Settings.fallbackFontAssets.Add(fontAsset);
            }
            EditorUtility.SetDirty(settings);
        }
        else
        {
            Debug.LogWarning("[PretendardFontSetup] TMP_Settings.instance is null; skipped global fallback registration.");
        }

        AssetDatabase.SaveAssets();
    }
}

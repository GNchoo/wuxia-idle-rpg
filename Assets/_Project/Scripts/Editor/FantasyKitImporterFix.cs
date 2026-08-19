using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IdleMvp.EditorTools
{
    /// <summary>
    /// One-time importer pass for the Mobile Fantasy Idle UI Kit:
    /// sets real 9-slice borders (the kit ships with none) so themed widgets
    /// can stretch without distorting corners/caps like the demo screenshots.
    /// Border vector = (left, bottom, right, top) in source pixels.
    /// Syncs BOTH TextureImporterSettings.spriteBorder AND spritesheet[].border
    /// (Multiple mode uses the sheet entry for runtime slicing).
    /// </summary>
    public static class FantasyKitImporterFix
    {
        const string Root = "Assets/FantasyIdleGameGUI/Resources/Sprites/";

        static readonly Dictionary<string, Vector4> Borders = new Dictionary<string, Vector4>
        {
            // Buttons
            { "Buttons/Btn_Mint.png",        new Vector4(35, 35, 35, 35) },
            { "Buttons/Btn_Green.png",       new Vector4(35, 35, 35, 35) },
            { "Buttons/Btn_Red.png",         new Vector4(35, 35, 35, 35) },
            { "Buttons/Btn_Round_Gray.png",  new Vector4(35, 25, 35, 25) },
            { "Buttons/Btn_Violet_S.png",    new Vector4(35, 25, 35, 25) },

            // Popups
            { "Popups/Popup_Bg.png",         new Vector4(48, 48, 48, 48) },
            { "Popups/Popup_Title.png",      new Vector4(54, 44, 54, 44) },

            // Frames
            { "Frame/Frame_List_Gray.png",     new Vector4(30, 30, 30, 30) },
            { "Frame/Frame_List_Redbrown.png", new Vector4(30, 30, 30, 30) },
            { "Frame/Frame_List_Yellow.png",   new Vector4(30, 30, 30, 30) },
            { "Frame/Frame_Round_Black.png",   new Vector4(40, 30, 40, 30) },
            { "Frame/Frame_Text_01.png",       new Vector4(30, 20, 30, 20) },
            { "Frame/Frame_Green_Text.png",    new Vector4(25, 22, 25, 22) },
            { "Frame/Frame_Img.png",           new Vector4(35, 35, 35, 35) },
            { "Frame/Frame_Cyan.png",          new Vector4(22, 20, 22, 20) },
            { "Frame/Frame_Stage.png",         new Vector4(55, 40, 55, 40) },
            { "Frame/Frame_Profile.png",       new Vector4(28, 28, 28, 28) },
            { "Frame/Frame_Edge_Gray.png",     new Vector4(40, 40, 40, 40) },
            { "Frame/Frame_Edge_Green.png",    new Vector4(40, 40, 40, 40) },
            { "Frame/Frame_Edge_Blue.png",     new Vector4(40, 40, 40, 40) },
            { "Frame/Frame_Edge_Violet.png",   new Vector4(40, 40, 40, 40) },
            { "Frame/Frame_Edge_Yellow.png",   new Vector4(40, 40, 40, 40) },
            { "Frame/Frame_Edge_Red.png",      new Vector4(40, 40, 40, 40) },
            { "Frame/Frame_Edge_None.png",     new Vector4(40, 40, 40, 40) },
            { "Frame/Frame_Edge_Selete.png",   new Vector4(40, 40, 40, 40) },

            // Components
            { "Components/Tab_On.png",       new Vector4(35, 30, 35, 30) },
            { "Components/Tab_Off.png",      new Vector4(35, 30, 35, 30) },
            { "Components/Bar_Back.png",     new Vector4(15, 12, 15, 12) },
            { "Components/Bar_Back_S.png",   new Vector4(12, 10, 12, 10) },
            { "Components/Bar_Front.png",    new Vector4(12, 10, 12, 10) },
        };

        [MenuItem("IdleMvp/Fix Fantasy Kit Sprite Borders")]
        public static void Run()
        {
            int done = 0, missing = 0;
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var kv in Borders)
                {
                    string path = Root + kv.Key;
                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer == null)
                    {
                        Debug.LogWarning("[FantasyKitImporterFix] Missing: " + path);
                        missing++;
                        continue;
                    }

                    var settings = new TextureImporterSettings();
                    importer.ReadTextureSettings(settings);
                    settings.spriteMeshType = SpriteMeshType.FullRect;
                    settings.spriteBorder = kv.Value;
                    importer.SetTextureSettings(settings);

                    // Multiple sprite mode: runtime uses spritesheet[].border, not only settings.spriteBorder.
                    var sheet = importer.spritesheet;
                    if (sheet != null && sheet.Length > 0)
                    {
                        for (int i = 0; i < sheet.Length; i++)
                        {
                            var sp = sheet[i];
                            sp.border = kv.Value;
                            sheet[i] = sp;
                        }
                        importer.spritesheet = sheet;
                    }

                    importer.SaveAndReimport();
                    done++;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }
            Debug.Log($"[FantasyKitImporterFix] Borders applied (settings+sheet): {done}, missing: {missing}");
        }
    }
}

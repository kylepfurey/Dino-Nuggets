using System.IO;
using UnityEditor;

namespace DN
{
    public static class SpreadsheetImporter
    {
        [MenuItem("Dino Nuggets/Update Spreadsheets")]
        public static void UpdateSpreadsheets()
        {
            GSpreadSheetsToJson google = new();
            google.Init();                                      // Field <spreadSheetKey> needs to be initialized
            google.DownloadToJson();                            // Method DownloadToJson() needs to be public
            EditorUtility.ClearProgressBar();
            foreach (var sheetName in google.ranges)            // Local variable <ranges> needs to be exposed as a public field
            {
                string path = google.outputDir + sheetName;     // Field <outputDir> needs to be initialized and public
                string objFile = path + ".asset";
                string jsonFile = path + ".txt";
                var sheet = AssetDatabase.LoadAssetAtPath<Spreadsheet>(objFile);
                sheet.Deserialize(File.ReadAllText(jsonFile));
                File.Delete(jsonFile);
            }
            AssetDatabase.Refresh();
        }
    }
}

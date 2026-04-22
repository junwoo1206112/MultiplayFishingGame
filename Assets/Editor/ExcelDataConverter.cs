using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using MultiplayFishing.Data.Models;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace MultiplayFishing.Editor
{
    public class ExcelDataConverter : EditorWindow
    {
        private const string ExcelPath = "Assets/ExcelData/FishData.xlsx";
        private const string ResourcePath = "Assets/Resources/Data/Fish";

        [MenuItem("Tools/Excel/Create Blank Fish Data")]
        public static void CreateBlankExcel()
        {
            if (File.Exists(ExcelPath))
            {
                if (!EditorUtility.DisplayDialog("Warning", "엑셀 파일이 이미 존재합니다. 덮어쓰시겠습니까?", "Yes", "No"))
                    return;
            }

            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("FishList");

            IRow headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("ID");
            headerRow.CreateCell(1).SetCellValue("Name");
            headerRow.CreateCell(2).SetCellValue("Rank");
            headerRow.CreateCell(3).SetCellValue("Chance (%)");
            headerRow.CreateCell(4).SetCellValue("Price (Gold)");
            headerRow.CreateCell(5).SetCellValue("Length (cm)");

            // 예시 로우
            IRow exampleRow = sheet.CreateRow(1);
            exampleRow.CreateCell(0).SetCellValue("FISH_001");
            exampleRow.CreateCell(1).SetCellValue("Mackerel");
            exampleRow.CreateCell(2).SetCellValue("D");
            exampleRow.CreateCell(3).SetCellValue(25.0f);
            exampleRow.CreateCell(4).SetCellValue(100);
            exampleRow.CreateCell(5).SetCellValue(30.5f);

            using (FileStream file = new FileStream(ExcelPath, FileMode.Create, FileAccess.Write))
            {
                workbook.Write(file);
            }

            AssetDatabase.Refresh();
            Debug.Log($"Blank Excel created at: {ExcelPath}");
        }

        [MenuItem("Tools/Excel/Convert Fish Data to SO")]
        public static void ConvertExcelToSO()
        {
            if (!File.Exists(ExcelPath))
            {
                Debug.LogError($"Excel file not found at: {ExcelPath}");
                return;
            }

            if (!Directory.Exists(ResourcePath))
            {
                Directory.CreateDirectory(ResourcePath);
            }

            using (FileStream file = new FileStream(ExcelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                IWorkbook workbook = new XSSFWorkbook(file);
                ISheet sheet = workbook.GetSheetAt(0);

                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    IRow row = sheet.GetRow(i);
                    if (row == null || row.GetCell(0) == null) continue;

                    string id = row.GetCell(0).StringCellValue;
                    string fileName = $"{id}.asset";
                    string fullPath = Path.Combine(ResourcePath, fileName);

                    FishDataSO fishData = AssetDatabase.LoadAssetAtPath<FishDataSO>(fullPath);
                    if (fishData == null)
                    {
                        fishData = ScriptableObject.CreateInstance<FishDataSO>();
                        AssetDatabase.CreateAsset(fishData, fullPath);
                    }

                    fishData.id = id;
                    fishData.fishName = row.GetCell(1)?.StringCellValue ?? "Unknown";
                    fishData.rank = row.GetCell(2)?.StringCellValue ?? "E";
                    fishData.catchChance = (float)(row.GetCell(3)?.NumericCellValue ?? 0f);
                    fishData.sellPrice = (int)(row.GetCell(4)?.NumericCellValue ?? 0);
                    fishData.lengthCm = (float)(row.GetCell(5)?.NumericCellValue ?? 0f);

                    EditorUtility.SetDirty(fishData);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Excel data converted to ScriptableObjects successfully.");
        }
    }
}

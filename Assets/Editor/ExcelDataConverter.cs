using UnityEngine;
using UnityEditor;
using System.IO;
using System;
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

        [MenuItem("Tools/Excel/1. Patch Creative Content (Desc & EXP)")]
        public static void PatchCreativeContent()
        {
            if (!File.Exists(ExcelPath)) return;

            try
            {
                IWorkbook workbook;
                using (FileStream file = new FileStream(ExcelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    workbook = new XSSFWorkbook(file);
                }

                ISheet sheet = workbook.GetSheetAt(0);
                IRow headerRow = sheet.GetRow(0);

                // 헤더 확장 (ID, Name, Rank, Chance, Price, Min, Max, Description, EXP)
                headerRow.CreateCell(7).SetCellValue("Description");
                headerRow.CreateCell(8).SetCellValue("EXP Reward");

                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    IRow row = sheet.GetRow(i);
                    if (row == null || row.GetCell(0) == null) continue;

                    string id = GetStringValue(row.GetCell(0)).ToLower();
                    string rank = GetStringValue(row.GetCell(2)).ToUpper();

                    // 1. 임의의 한글 설명 할당
                    string creativeDesc = GetFishDescription(id);
                    row.CreateCell(7).SetCellValue(creativeDesc);

                    // 2. 등급별 경험치 자동 할당
                    int exp = GetDefaultExpReward(rank);
                    row.CreateCell(8).SetCellValue(exp);
                    
                    // 3. 만약 최소/최대 크기가 없다면 이것도 패치
                    if (GetNumericValue(row.GetCell(5)) <= 0)
                    {
                        var (min, max) = GetDefaultSizeRange(rank, id);
                        row.CreateCell(5).SetCellValue(min);
                        row.CreateCell(6).SetCellValue(max);
                    }
                }

                using (FileStream file = new FileStream(ExcelPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    workbook.Write(file);
                }

                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("패치 완료", "모든 물고기에 한글 설명과 경험치 데이터가 추가되었습니다!", "확인");
            }
            catch (IOException)
            {
                EditorUtility.DisplayDialog("Error", "FishData.xlsx 파일을 닫아주세요.", "OK");
            }
        }

        [MenuItem("Tools/Excel/2. Convert Excel to SO Assets")]
        public static void ConvertExcelToSO()
        {
            if (!File.Exists(ExcelPath)) return;

            using (FileStream file = new FileStream(ExcelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                IWorkbook workbook = new XSSFWorkbook(file);
                ISheet sheet = workbook.GetSheetAt(0);

                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    IRow row = sheet.GetRow(i);
                    if (row == null || row.GetCell(0) == null) continue;

                    string id = GetStringValue(row.GetCell(0));
                    string assetPath = Path.Combine(ResourcePath, $"{id}.asset");

                    FishDataSO fishData = AssetDatabase.LoadAssetAtPath<FishDataSO>(assetPath);
                    if (fishData == null)
                    {
                        fishData = ScriptableObject.CreateInstance<FishDataSO>();
                        AssetDatabase.CreateAsset(fishData, assetPath);
                    }

                    fishData.id = id;
                    fishData.fishName = GetStringValue(row.GetCell(1));
                    fishData.rank = GetStringValue(row.GetCell(2));
                    fishData.catchChance = GetNumericValue(row.GetCell(3));
                    fishData.sellPrice = (int)GetNumericValue(row.GetCell(4));
                    fishData.minSize = GetNumericValue(row.GetCell(5));
                    fishData.maxSize = GetNumericValue(row.GetCell(6));
                    fishData.description = GetStringValue(row.GetCell(7));
                    fishData.expReward = (int)GetNumericValue(row.GetCell(8));

                    EditorUtility.SetDirty(fishData);
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("Excel sync complete.");
            }
        }

        private static int GetDefaultExpReward(string rank)
        {
            return rank switch {
                "S" => 5000, "A" => 1000, "B" => 250, "C" => 80, "D" => 20, _ => 10
            };
        }

        private static string GetFishDescription(string id)
        {
            if (id.Contains("alligator")) return "강의 지배자. 날카로운 이빨을 조심하세요.";
            if (id.Contains("axolotl")) return "웃는 얼굴의 귀여운 도롱뇽. 보고만 있어도 기분이 좋아집니다.";
            if (id.Contains("bass")) return "입이 정말 커서 무엇이든 집어삼키는 탐욕스러운 물고기입니다.";
            if (id.Contains("shark")) return "바다의 최상위 포식자. 잡았다는 것 자체가 기적입니다!";
            if (id.Contains("goldfish")) return "어항에서 탈출한 걸까요? 반짝이는 비늘이 아름답습니다.";
            if (id.Contains("ufo")) return "이건 물고기가 아닙니다! 외계에서 날아온 비행 물체네요.";
            if (id.Contains("prehistoric")) return "수억 년 전 고대 바다를 헤엄치던 살아있는 화석입니다.";
            if (id.Contains("whale")) return "지구상에서 가장 거대한 생명체. 낚싯대가 버티는 게 신기하군요.";
            if (id.Contains("clownfish")) return "산호초 사이를 헤엄치는 귀여운 물고기. 영화 속 주인공 같아요.";
            if (id.Contains("octopus")) return "지능이 매우 높고 여덟 개의 다리를 자유자재로 사용합니다.";
            
            return "평범하지만 특별한 당신만의 물고기입니다.";
        }

        private static string GetStringValue(ICell cell)
        {
            if (cell == null) return "";
            if (cell.CellType == CellType.String) return cell.StringCellValue;
            if (cell.CellType == CellType.Numeric) return cell.NumericCellValue.ToString();
            return cell.ToString();
        }

        private static float GetNumericValue(ICell cell)
        {
            if (cell == null) return 0f;
            if (cell.CellType == CellType.Numeric) return (float)cell.NumericCellValue;
            if (cell.CellType == CellType.String && float.TryParse(cell.StringCellValue, out float result)) return result;
            return 0f;
        }

        private static (float min, float max) GetDefaultSizeRange(string rank, string id)
        {
            if (id.Contains("prehistoric") || id.Contains("ancient")) return (300f, 1500f);
            if (id.Contains("ufo") || id.Contains("alien")) return (10f, 1000f);

            return rank switch {
                "S" => (200f, 800f), "A" => (100f, 250f), "B" => (50f, 120f),
                "C" => (20f, 60f), "D" => (5f, 25f), _ => (10f, 50f)
            };
        }
    }
}

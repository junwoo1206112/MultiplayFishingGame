using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace MultiplayFishing.Editor
{
    public class ShopDataPopulator : EditorWindow
    {
        private const string ExcelPath = "Assets/ExcelData/FishData.xlsx";
        private const string RodsResourcePath = "Assets/Resources/Data/Rods";
        private const string BaitsResourcePath = "Assets/Resources/Data/Baits";

        private class RodEntry
        {
            public string id;
            public string name;
            public string rank;
            public int price;
            public float castDistanceBonus;
            public float catchChanceBonus;
            public float durability;
            public string desc;
        }

        private class BaitEntry
        {
            public string id;
            public string name;
            public string rank;
            public int price;
            public string attractionFishType;
            public float catchChanceBonus;
            public string desc;
        }

        [MenuItem("Tools/Excel/3. Populate Shop Data (Rods & Baits)")]
        public static void PopulateShopData()
        {
            if (!File.Exists(ExcelPath))
            {
                EditorUtility.DisplayDialog("Error", "FishData.xlsx not found!", "OK");
                return;
            }

            IWorkbook workbook;
            using (FileStream file = new FileStream(ExcelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                workbook = new XSSFWorkbook(file);
            }

            AddRodsSheet(workbook);
            AddBaitsSheet(workbook);

            using (FileStream file = new FileStream(ExcelPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                workbook.Write(file);
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("완료", "Rods/Baits 시트가 FishData.xlsx에 추가되었습니다!", "확인");
        }

        private static void AddRodsSheet(IWorkbook workbook)
        {
            ISheet sheet = workbook.CreateSheet("Rods");

            IRow headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("ID");
            headerRow.CreateCell(1).SetCellValue("Name");
            headerRow.CreateCell(2).SetCellValue("Rank");
            headerRow.CreateCell(3).SetCellValue("Price");
            headerRow.CreateCell(4).SetCellValue("CastDistanceBonus");
            headerRow.CreateCell(5).SetCellValue("CatchChanceBonus");
            headerRow.CreateCell(6).SetCellValue("Durability");
            headerRow.CreateCell(7).SetCellValue("Description");

            List<RodEntry> rods = GetRodData();
            for (int i = 0; i < rods.Count; i++)
            {
                IRow row = sheet.CreateRow(i + 1);
                row.CreateCell(0).SetCellValue(rods[i].id);
                row.CreateCell(1).SetCellValue(rods[i].name);
                row.CreateCell(2).SetCellValue(rods[i].rank);
                row.CreateCell(3).SetCellValue(rods[i].price);
                row.CreateCell(4).SetCellValue(rods[i].castDistanceBonus);
                row.CreateCell(5).SetCellValue(rods[i].catchChanceBonus);
                row.CreateCell(6).SetCellValue(rods[i].durability);
                row.CreateCell(7).SetCellValue(rods[i].desc);
            }
        }

        private static void AddBaitsSheet(IWorkbook workbook)
        {
            ISheet sheet = workbook.CreateSheet("Baits");

            IRow headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("ID");
            headerRow.CreateCell(1).SetCellValue("Name");
            headerRow.CreateCell(2).SetCellValue("Rank");
            headerRow.CreateCell(3).SetCellValue("Price");
            headerRow.CreateCell(4).SetCellValue("AttractionFishType");
            headerRow.CreateCell(5).SetCellValue("CatchChanceBonus");
            headerRow.CreateCell(6).SetCellValue("Description");

            List<BaitEntry> baits = GetBaitData();
            for (int i = 0; i < baits.Count; i++)
            {
                IRow row = sheet.CreateRow(i + 1);
                row.CreateCell(0).SetCellValue(baits[i].id);
                row.CreateCell(1).SetCellValue(baits[i].name);
                row.CreateCell(2).SetCellValue(baits[i].rank);
                row.CreateCell(3).SetCellValue(baits[i].price);
                row.CreateCell(4).SetCellValue(baits[i].attractionFishType);
                row.CreateCell(5).SetCellValue(baits[i].catchChanceBonus);
                row.CreateCell(6).SetCellValue(baits[i].desc);
            }
        }

        private static List<RodEntry> GetRodData()
        {
            return new List<RodEntry>
            {
                new RodEntry { id = "rod_basic",      name = "기본 낚싯대",   rank = "★",     price = 0,     castDistanceBonus = 0,   catchChanceBonus = 0,   durability = 100, desc = "누구나 사용할 수 있는 기본 낚싯대입니다." },
                new RodEntry { id = "rod_carbon",     name = "카본 낚싯대",   rank = "★★",    price = 3000,  castDistanceBonus = 2,   catchChanceBonus = 3,   durability = 150, desc = "가볍고 튼튼한 카본 소재의 낚싯대입니다." },
                new RodEntry { id = "rod_fiberglass", name = "유리섬유 낚싯대", rank = "★★★",   price = 8000,  castDistanceBonus = 5,   catchChanceBonus = 5,   durability = 200, desc = "유리섬유 소재로 제작된 중급 낚싯대입니다." },
                new RodEntry { id = "rod_titanium",   name = "티타늄 낚싯대",  rank = "★★★★",  price = 20000, castDistanceBonus = 10,  catchChanceBonus = 8,   durability = 300, desc = "초경량 티타늄 합금으로 제작된 고급 낚싯대입니다." },
                new RodEntry { id = "rod_legendary",  name = "전설의 낚싯대",  rank = "★★★★★", price = 50000, castDistanceBonus = 15,  catchChanceBonus = 15,  durability = 500, desc = "전설 속에만 존재한다는 궁극의 낚싯대입니다." },
            };
        }

        private static List<BaitEntry> GetBaitData()
        {
            return new List<BaitEntry>
            {
                new BaitEntry { id = "bait_basic",    name = "기본 미끼",   rank = "★",     price = 0,    attractionFishType = "all",        catchChanceBonus = 0,  desc = "아무 물고기나 잡을 수 있는 기본 미끼입니다." },
                new BaitEntry { id = "bait_worm",     name = "지렁이",      rank = "★★",    price = 500,  attractionFishType = "freshwater", catchChanceBonus = 5,  desc = "민물고기에게 특히 효과적인 지렁이 미끼입니다." },
                new BaitEntry { id = "bait_shrimp",   name = "새우",        rank = "★★",    price = 600,  attractionFishType = "saltwater",  catchChanceBonus = 5,  desc = "바닷물고기에게 특히 효과적인 새우 미끼입니다." },
                new BaitEntry { id = "bait_lure",     name = "루어",        rank = "★★★",   price = 2000, attractionFishType = "all",        catchChanceBonus = 10, desc = "모든 물고기에게 효과가 좋은 인공 미끼입니다." },
                new BaitEntry { id = "bait_golden",   name = "황금 미끼",   rank = "★★★★",  price = 8000, attractionFishType = "rare",       catchChanceBonus = 20, desc = "희귀 물고기를 유인하는 황금빛 미끼입니다." },
                new BaitEntry { id = "bait_ancient",  name = "고대의 미끼",  rank = "★★★★★", price = 20000, attractionFishType = "legendary",   catchChanceBonus = 30, desc = "고대 바다 생물을 끌어당기는 신비로운 미끼입니다." },
            };
        }
    }
}

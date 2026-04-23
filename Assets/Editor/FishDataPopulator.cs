using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace MultiplayFishing.Editor
{
    public class FishDataPopulator : EditorWindow
    {
        private const string ExcelPath = "Assets/ExcelData/FishData.xlsx";

        private class FishEntry
        {
            public string id;
            public string name;
            public string rank;
            public float chance;
            public int price;
            public float length;
            public string desc;

            public FishEntry(string id, string name, string rank, float chance, int price, float length, string desc)
            {
                this.id = id; this.name = name; this.rank = rank; this.chance = chance;
                this.price = price; this.length = length; this.desc = desc;
            }
        }

        [MenuItem("Tools/Excel/Populate 52 Webfishing Data")]
        public static void PopulateExcel()
        {
            List<FishEntry> fishList = new List<FishEntry>();

            // --- 민물고기 (Freshwater: fish1~32) ---
            fishList.Add(new FishEntry("fish_alligator", "악어", "S", 0.1f, 8000, 350, "늪지의 공포, 거대한 이빨을 가진 지배자입니다."));
            fishList.Add(new FishEntry("fish_axolotl", "아홀로틀", "S", 0.05f, 9999, 25, "웃고 있는 비밀의 수호자, 핑크빛 신비입니다."));
            fishList.Add(new FishEntry("fish_bass", "큰입배스", "D", 8.0f, 100, 45, "흔하지만 손맛이 좋은 민물의 강자입니다."));
            fishList.Add(new FishEntry("fish_bluegill", "블루길", "D", 10.0f, 80, 20, "파란색 아가미를 가진 작고 흔한 물고기입니다."));
            fishList.Add(new FishEntry("fish_bowfin", "보우핀", "C", 4.0f, 250, 60, "살아있는 화석이라 불리는 튼튼한 물고기입니다."));
            fishList.Add(new FishEntry("fish_bullshark", "황소상어", "A", 0.5f, 3500, 220, "강까지 거슬러 올라오는 아주 위험한 상어입니다."));
            fishList.Add(new FishEntry("fish_carp", "잉어", "C", 5.0f, 300, 70, "강인한 생명력의 상징, 영리한 민물고기입니다."));
            fishList.Add(new FishEntry("fish_catfish", "메기", "D", 7.0f, 150, 80, "긴 수염을 가진 바닥의 청소부입니다."));
            fishList.Add(new FishEntry("fish_crab", "게", "D", 6.0f, 120, 15, "옆으로 걷는 집게손의 달인입니다."));
            fishList.Add(new FishEntry("fish_crappie", "크래피", "D", 9.0f, 90, 25, "작지만 군집 생활을 하는 흔한 민물고기입니다."));
            fishList.Add(new FishEntry("fish_crayfish", "가재", "D", 8.5f, 110, 10, "바위 틈에 숨어 사는 집게벌레입니다."));
            fishList.Add(new FishEntry("fish_drum", "민어", "C", 3.5f, 400, 50, "소리를 내어 대화하는 신기한 물고기입니다."));
            fishList.Add(new FishEntry("fish_frog", "개구리", "D", 10.0f, 50, 8, "폴짝폴짝 뛰어다니는 귀여운 양서류입니다."));
            fishList.Add(new FishEntry("fish_gar", "가아", "B", 1.5f, 1200, 150, "악어 같은 입을 가진 고대의 포식자입니다."));
            fishList.Add(new FishEntry("fish_bass_golden", "골든 배스", "S", 0.1f, 7777, 50, "행운을 상징하는 황금빛 배스입니다."));
            fishList.Add(new FishEntry("fish_goldfish", "금붕어", "D", 5.0f, 300, 15, "연못에서 탈출한 듯한 예쁜 물고기입니다."));
            fishList.Add(new FishEntry("fish_guppy", "구피", "D", 12.0f, 40, 5, "작고 화려한 지느러미를 가진 물고기입니다."));
            fishList.Add(new FishEntry("fish_salmon_king", "왕연어", "A", 0.8f, 2500, 120, "연어 중의 왕, 거대한 크기를 자랑합니다."));
            fishList.Add(new FishEntry("fish_koi", "비단잉어", "B", 2.0f, 1500, 65, "아름다운 무늬를 가진 정원의 보석입니다."));
            fishList.Add(new FishEntry("fish_leech", "거머리", "E", 10.0f, 10, 5, "피를 빨아먹는 끈질긴 녀석입니다."));
            fishList.Add(new FishEntry("fish_mooneye", "문아이", "C", 3.0f, 350, 30, "달처럼 큰 눈을 가진 은색 물고기입니다."));
            fishList.Add(new FishEntry("fish_muskellunge", "머스키", "B", 1.2f, 1800, 130, "민물의 늑대라 불리는 거대한 포식자입니다."));
            fishList.Add(new FishEntry("fish_perch", "퍼치", "D", 8.0f, 130, 35, "줄무늬가 특징인 대중적인 민물고기입니다."));
            fishList.Add(new FishEntry("fish_pike", "강꼬치고기", "B", 2.5f, 900, 100, "화살처럼 빠른 공격력을 가진 물고기입니다."));
            fishList.Add(new FishEntry("fish_pupfish", "펍피쉬", "E", 0.2f, 5000, 4, "사막의 오아시스에 사는 아주 희귀한 작은 물고기입니다."));
            fishList.Add(new FishEntry("fish_trout_rainbow", "무지개송어", "B", 3.0f, 750, 55, "옆면에 아름다운 무지개색 띠가 있습니다."));
            fishList.Add(new FishEntry("fish_salmon", "연어", "C", 4.5f, 550, 80, "고향을 찾아 돌아오는 맛있는 물고기입니다."));
            fishList.Add(new FishEntry("fish_snail", "달팽이", "E", 15.0f, 20, 3, "느릿느릿 기어다니는 껍질집 주인입니다."));
            fishList.Add(new FishEntry("fish_sturgeon", "철갑상어", "A", 0.4f, 4500, 250, "귀한 캐비어를 품고 있는 커다란 물고기입니다."));
            fishList.Add(new FishEntry("fish_toad", "두꺼비", "D", 7.0f, 80, 12, "울퉁불퉁한 피부를 가진 듬직한 양서류입니다."));
            fishList.Add(new FishEntry("fish_turtle", "거북이", "C", 2.0f, 1200, 40, "오래 사는 지혜의 상징, 딱딱한 등껍질을 가졌습니다."));
            fishList.Add(new FishEntry("fish_walleye", "월아이", "B", 3.8f, 450, 60, "밤눈이 밝은 빛나는 눈의 물고기입니다."));

            // --- 바닷물고기 (Saltwater: fish33~42) ---
            fishList.Add(new FishEntry("fish_angelfish", "엔젤피쉬", "A", 4.0f, 600, 20, "천사처럼 우아한 지느러미를 가졌습니다."));
            fishList.Add(new FishEntry("fish_bluefish", "블루피쉬", "D", 7.5f, 180, 50, "성질이 사나운 바다의 파이터입니다."));
            fishList.Add(new FishEntry("fish_clownfish", "흰동가리", "D", 5.5f, 450, 10, "말미잘 속에 숨어 사는 귀여운 니모입니다."));
            fishList.Add(new FishEntry("fish_coelacanth", "실러캔스", "S", 0.08f, 12000, 180, "수억 년 전부터 살아온 바다의 살아있는 화석입니다."));
            fishList.Add(new FishEntry("fish_dogfish", "돔발상어", "C", 3.2f, 550, 100, "개처럼 무리를 지어 다니는 작은 상어입니다."));
            fishList.Add(new FishEntry("fish_eel", "장어", "D", 2.2f, 1100, 110, "미끌미끌하고 힘이 넘치는 스테미나의 왕입니다."));
            fishList.Add(new FishEntry("fish_flounder", "가자미", "D", 8.2f, 220, 40, "바닥에 딱 붙어 사는 위장의 달인입니다."));
            fishList.Add(new FishEntry("fish_grouper", "그루퍼", "B", 1.8f, 1600, 150, "거대한 입으로 무엇이든 삼키는 바다의 대식가입니다."));
            fishList.Add(new FishEntry("fish_herring", "청어", "D", 11.0f, 70, 30, "떼 지어 다니는 은빛 바다의 보석입니다."));
            fishList.Add(new FishEntry("fish_krill", "크릴", "E", 20.0f, 5, 2, "고래들의 훌륭한 식사이자 바다의 기초입니다."));

            // --- Ocean (ocean_fish1~3) ---
            fishList.Add(new FishEntry("fish_lionfish", "쏠배감펭", "B", 2.0f, 1300, 35, "화려한 가시 속에 독을 품은 아름다운 물고기입니다."));
            fishList.Add(new FishEntry("fish_lobster", "바닷가재", "A", 1.0f, 3200, 45, "고급스러운 맛과 강력한 집게를 가졌습니다."));
            fishList.Add(new FishEntry("fish_manowar", "만오워", "A", 0.7f, 5500, 30, "푸른 바다의 위험한 독침 풍선입니다."));

            // --- Deep Sea (deep_sea_fish1~3) ---
            fishList.Add(new FishEntry("fish_mantaray", "가오리", "B", 1.5f, 2200, 250, "바다를 나는 듯이 헤엄치는 거대한 날개입니다."));
            fishList.Add(new FishEntry("fish_mantaray_golden", "황금 가오리", "S", 0.05f, 15000, 300, "바다의 보물, 찬란한 황금빛을 내뿜습니다."));
            fishList.Add(new FishEntry("fish_seaturtle", "바다거북", "S", 0.15f, 9000, 120, "대양을 횡단하는 경이로운 여행자입니다."));

            // --- Prehistoric (prehistoric_fish1~3) ---
            fishList.Add(new FishEntry("fish_anomalocaris", "아노말로카리스", "S", 0.05f, 25000, 100, "고대 캄브리아기 바다의 정점 포식자입니다."));
            fishList.Add(new FishEntry("fish_helicoprion", "헬리코프리온", "S", 0.05f, 30000, 400, "나선형 이빨을 가진 기괴한 고대 상어입니다."));
            fishList.Add(new FishEntry("fish_leedsichthys", "리드시크티스", "S", 0.02f, 45000, 1600, "역사상 가장 컸던 거대 물고기입니다."));

            // --- Special (alien_creature2) ---
            fishList.Add(new FishEntry("fish_ufo", "UFO", "S", 0.01f, 99999, 500, "이건 물고기가 아닌 것 같은데... 외계에서 왔을까요?"));

            WriteToExcel(fishList);
        }

        private static void WriteToExcel(List<FishEntry> fishList)
        {
            if (!Directory.Exists("Assets/ExcelData")) Directory.CreateDirectory("Assets/ExcelData");

            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("FishList");

            IRow headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("ID");
            headerRow.CreateCell(1).SetCellValue("Name");
            headerRow.CreateCell(2).SetCellValue("Rank");
            headerRow.CreateCell(3).SetCellValue("Chance (%)");
            headerRow.CreateCell(4).SetCellValue("Price (Gold)");
            headerRow.CreateCell(5).SetCellValue("Length (cm)");
            headerRow.CreateCell(6).SetCellValue("Description");

            for (int i = 0; i < fishList.Count; i++)
            {
                IRow row = sheet.CreateRow(i + 1);
                row.CreateCell(0).SetCellValue(fishList[i].id);
                row.CreateCell(1).SetCellValue(fishList[i].name);
                row.CreateCell(2).SetCellValue(fishList[i].rank);
                row.CreateCell(3).SetCellValue(fishList[i].chance);
                row.CreateCell(4).SetCellValue(fishList[i].price);
                row.CreateCell(5).SetCellValue(fishList[i].length);
                row.CreateCell(6).SetCellValue(fishList[i].desc);
            }

            using (FileStream file = new FileStream(ExcelPath, FileMode.Create, FileAccess.Write))
            {
                workbook.Write(file);
            }

            AssetDatabase.Refresh();
            Debug.Log($"<color=green><b>[Success]</b> Webfishing fish data ({fishList.Count} items) populated at: {ExcelPath}</color>");
        }
    }
}

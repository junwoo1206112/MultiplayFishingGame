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
        private const string FishResourcePath = "Assets/Resources/Data/Fish";
        private const string RodResourcePath = "Assets/Resources/Data/Rods";
        private const string BaitResourcePath = "Assets/Resources/Data/Baits";

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

                // 헤더 확장 (ID, Name, Rank, Chance, Price, Min, Max, Description, EXP, Weight, RequiredSpam)
                headerRow.CreateCell(7).SetCellValue("Description");
                headerRow.CreateCell(8).SetCellValue("EXP Reward");
                headerRow.CreateCell(9).SetCellValue("Weight");
                headerRow.CreateCell(10).SetCellValue("Required Spam");

                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    IRow row = sheet.GetRow(i);
                    if (row == null || row.GetCell(0) == null) continue;

                    string id = GetStringValue(row.GetCell(0)).ToLower();
                    string rank = GetStringValue(row.GetCell(2));
                    float minSize = GetNumericValue(row.GetCell(5));
                    float maxSize = GetNumericValue(row.GetCell(6));

                    // 1. 임의의 한글 설명 할당
                    string creativeDesc = GetFishDescription(id);
                    row.CreateCell(7).SetCellValue(creativeDesc);

                    // 2. 등급별 경험치 자동 할당
                    int exp = GetDefaultExpReward(rank);
                    row.CreateCell(8).SetCellValue(exp);
                    
                    // 3. 만약 최소/최대 크기가 없다면 이것도 패치
                    if (minSize <= 0)
                    {
                        var (min, max) = GetDefaultSizeRange(rank, id);
                        minSize = min;
                        maxSize = max;
                        row.CreateCell(5).SetCellValue(min);
                        row.CreateCell(6).SetCellValue(max);
                    }

                    // 4. 무게 자동 할당 (크기에 비례, 등급별 보정)
                    float weight = GetDefaultWeight(rank, (minSize + maxSize) / 2f);
                    row.CreateCell(9).SetCellValue(weight);

                    // 5. 무게 기반 연타 횟수 계산 및 저장
                    int spam = CalculateRequiredSpam(rank, weight);
                    row.CreateCell(10).SetCellValue(spam);
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
                    string assetPath = Path.Combine(FishResourcePath, $"{id}.asset");

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
                    fishData.weight = GetNumericValue(row.GetCell(9));
                    fishData.requiredSpam = (int)GetNumericValue(row.GetCell(10));

                    EditorUtility.SetDirty(fishData);
                }
                // Excel에 없는 .asset 파일 정리 (고스트 데이터 제거)
                HashSet<string> validIds = new HashSet<string>();
                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    IRow row = sheet.GetRow(i);
                    if (row == null || row.GetCell(0) == null) continue;
                    validIds.Add(GetStringValue(row.GetCell(0)));
                }

                string[] existingAssets = Directory.GetFiles(FishResourcePath, "*.asset");
                foreach (string assetPath in existingAssets)
                {
                    string id = Path.GetFileNameWithoutExtension(assetPath);
                    if (!validIds.Contains(id))
                    {
                        AssetDatabase.DeleteAsset(assetPath.Replace("\\", "/"));
                        Debug.Log($"[ExcelDataConverter] Removed ghost fish: {id}");
                    }
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("Excel sync complete.");
            }
        }

        private static int CalculateRequiredSpam(string rank, float weight)
        {
            // 테스트를 위해 1~10 범위로 조정
            // 기본 1회 + 로그 기반 무게 보너스(0~4회) + 등급 보너스(1~5회)
            float weightBonus = (float)Math.Log10(weight + 1) * 1.5f;
            int rankBonus = rank.Length; // 별 개수당 1회
            
            return Mathf.Clamp(Mathf.RoundToInt(1 + weightBonus + rankBonus), 1, 10);
        }

        private static float GetDefaultWeight(string rank, float avgSize)
        {
            // 기본적인 무게 공식: 크기의 제곱에 등급 보너스 곱함
            float baseWeight = (avgSize * avgSize) / 500f;
            
            float multiplier = rank switch {
                "★★★★★" => 5.0f,
                "★★★★" => 2.5f,
                "★★★" => 1.5f,
                "★★" => 1.1f,
                _ => 1.0f
            };

            return (float)Math.Round(baseWeight * multiplier, 2);
        }

        private static int GetDefaultExpReward(string rank)
        {
            return rank switch {
                "★★★★★" => 5000, "★★★★" => 1000, "★★★" => 250, "★★" => 80, "★" => 20, _ => 10
            };
        }

        private static string GetFishDescription(string id)
        {
            if (id.Contains("ufo")) return "이건 물고기가 아닙니다! 외계에서 날아온 미확인 비행 물체. 반짝이는 금속성 비늘을 가졌습니다.";
            if (id.Contains("anomalocaris") || id.Contains("prehistoric")) return "수억 년 전 고대 바다의 포식자. 살아있는 화석이라 불리기에 손색없는 존재입니다.";
            if (id.Contains("helicoprion")) return "톱니처럼 말려 있는 이빨이 인상적인 고대 상어. 독특한 턱 구조를 가졌습니다.";
            if (id.Contains("leedsichthys")) return "고대 바다의 거대 여과 섭식자. 몸집은 크지만 성격은 온순합니다.";
            if (id.Contains("coelacanth")) return "공룡 시대부터 살아남은 살아있는 화석. '바다의 공룡'이라 불립니다.";

            if (id.Contains("whale")) return "지구 최대의 생명체. 덩치에 비해 크릴만 먹고 사는 온순한 거인입니다.";
            if (id.Contains("shark_greatwhite")) return "바다의 최상위 포식자. 한 입에 물어뜯는 힘은 세계 최강입니다.";
            if (id.Contains("shark_hammerhead")) return "T자형 머리가 특징인 특이한 상어. 360도 시야를 자랑합니다.";
            if (id.Contains("bullshark")) return "민물과 바닷물을 오르내리는 사나운 상어. 공격성이 매우 높습니다.";
            if (id.Contains("mantaray_golden")) return "황금빛 가오리. 햇빛에 반사되어 눈부시게 아름답습니다.";
            if (id.Contains("mantaray")) return "거대한 가오리가 우아하게 바다를 나는 듯 헤엄칩니다.";
            if (id.Contains("stingray")) return "납작한 몸에 독침을 가진 해양 생물. 모래 밑에 숨어 있기 좋아합니다.";
            if (id.Contains("manta_ray")) return "바다의 천사. 넓은 가슴지느러미를 펼치고 활공합니다.";
            if (id.Contains("marlin")) return "돛처럼 솟은 등지느러미와 창처럼 긴 주둥이가 매력적인 대형 어종입니다.";
            if (id.Contains("swordfish")) return "검처럼 긴 주둥이로 물고기를 찌르는 대양의 검사입니다.";
            if (id.Contains("sawfish")) return "톱 모양의 긴 주둥이가 특징. 먹이를 찾아 해저를 휘젓습니다.";
            if (id.Contains("sunfish")) return "세상에서 가장 무거운 뼈 물고기. 태양을 닮은 둥근 몸매가 귀엽습니다.";
            if (id.Contains("tuna")) return "대양의 스프린터. 시속 70km로 질주하는 고등어과의 거물입니다.";
            if (id.Contains("seaturtle") || id.Contains("sea_turtle")) return "고대부터 바다를 누빈 장수 거북. 등껍질에는 작은 생물들이 살고 있습니다.";

            if (id.Contains("octopus")) return "8개의 팔을 가진 바다의 천재. 단기 기억력과 문제 해결 능력이 탁월합니다.";
            if (id.Contains("lobster")) return "바다의 갑옷 기사. 큰 집게발로 먹이를 움켜잡습니다.";
            if (id.Contains("crab")) return "옆으로 걷는 귀여운 갑각류. 집게발이지만 꽤 아픕니다.";
            if (id.Contains("crayfish")) return "민물에 사는 작은 랍스터. 개울가 돌 밑에서 흔히 볼 수 있습니다.";
            if (id.Contains("shrimp")) return "투명한 몸이 매력적인 작은 갑각류. 많은 바다 생물의 먹이가 됩니다.";
            if (id.Contains("krill")) return "남극 바다의 작은 보석. 고래도 이 작은 생물을 먹고 삽니다.";

            if (id.Contains("lionfish")) return "화려한 줄무늬와 독가시를 가진 위험한 미녀. 절대 만지면 안 됩니다.";
            if (id.Contains("clownfish")) return "주황색과 흰색 줄무늬가 사랑스러운 산호초의 아이돌.";
            if (id.Contains("angelfish")) return "길게 늘어진 지느러미가 우아한 열대어. 수족관의 왕자님입니다.";
            if (id.Contains("bluefish")) return "등푸른 생선의 대명사. 맛과 영양 모두 최고입니다.";
            if (id.Contains("dogfish")) return "작은 상어의 일종. 귀여운 외모와 달리 사냥 본능은 강합니다.";

            if (id.Contains("eel")) return "뱀처럼 길고 미끈한 몸. 야행성이라 밤에 더 활발합니다.";
            if (id.Contains("flounder")) return "두 눈이 한쪽에 몰려 있는 납작 물고기. 해저 바닥에 완벽히 위장합니다.";
            if (id.Contains("grouper")) return "산호초의 잠복꾼. 바위 틈에 숨어있다가 한순간에 먹이를 덥썩.";
            if (id.Contains("herring")) return "북해의 은빛 물결. 대량으로 떼지어 다니며 바다를 은빛으로 물들입니다.";
            if (id.Contains("muskellunge")) return "민물의 폭군. 북미 호수에서 최상위 포식자로 군림합니다.";
            if (id.Contains("pike")) return "민물 호수의 은밀한 사냥꾼. 수초 사이에 숨어있다가 기습합니다.";
            if (id.Contains("walleye")) return "야간 투시경을 가진 민물고기. 어두운 물속에서도 먹이를 놓치지 않습니다.";
            if (id.Contains("bowfin")) return "공기 호흡이 가능한 원시 민물고기. 산소가 부족해도 끄떡없습니다.";
            if (id.Contains("gar")) return "악어처럼 긴 주둥이와 날카로운 이빨을 가진 민물의 공룡.";

            if (id.Contains("alligator")) return "강의 지배자. 악어와 닮은 외모지만 물고기입니다. 날카로운 이빨을 조심하세요.";
            if (id.Contains("axolotl")) return "영원히 어린 상태로 사는 귀여운 도롱뇽. 재생 능력이 놀랍습니다.";
            if (id.Contains("frog")) return "개구리? 아니요 물고기입니다. 물속과 육지를 넘나드는 양서류.";
            if (id.Contains("toad")) return "울퉁불퉁한 피부가 매력 포인트. 독이 있으니 조심히 다루세요.";
            if (id.Contains("turtle")) return "등딱지를 지닌 느긋한 민물 거북. 수중 생활에 완벽히 적응했습니다.";
            if (id.Contains("leech")) return "빨판을 가진 기생 생물. 좀 징그럽지만 생태계의 중요한 구성원입니다.";
            if (id.Contains("snail")) return "천천히 그러나 꾸준히. 나선형 껍질이 아름다운 복족류입니다.";
            if (id.Contains("manowar")) return "바다의 독가스 풍선. 긴 촉수에 치명적인 독이 있습니다.";

            if (id.Contains("bass_golden")) return "황금빛으로 빛나는 특별한 배스. 행운을 부르는 물고기라고 합니다.";
            if (id.Contains("bass")) return "입이 정말 커서 무엇이든 집어삼키는 탐욕스러운 민물 스타.";
            if (id.Contains("perch")) return "호수의 대표 어종. 초보 낚시꾼의 첫 번째 친구.";
            if (id.Contains("bluegill")) return "파란 아가미 덮개가 매력적인 선버시. 아이들이 가장 좋아하는 물고기.";
            if (id.Contains("crappie")) return "납작한 몸에 얼룩덜룩한 무늬. 낚시하기 재미있는 어종입니다.";
            if (id.Contains("drum")) return "독특한 꽥꽥 소리를 내는 물고기. 북을 치는 것 같다고 해서 붙여진 이름.";
            if (id.Contains("mooneye")) return "큰 눈이 달처럼 빛나는 야행성 물고기. 밤에 더 잘 잡힙니다.";
            if (id.Contains("pupfish")) return "사막의 물웅덩이에 사는 놀라운 생명체. 극한 환경에 적응했습니다.";
            if (id.Contains("carp")) return "민물의 거인. 튼튼하고 영리해서 낚시꾼의 단골 상대입니다.";
            if (id.Contains("catfish")) return "수염이 난 민물고기. 바닥을 더듬어 먹이를 찾습니다.";
            if (id.Contains("sturgeon")) return "철갑상어. 캐비어로 유명한 고대 어종, 비늘이 마치 갑옷 같습니다.";
            if (id.Contains("koi")) return "일본의 국민 물고기. 형형색색의 무늬가 마치 수중 그림입니다.";
            if (id.Contains("goldfish_salt")) return "바다에 사는 금붕어 사촌. 바닷물에서도 반짝입니다.";
            if (id.Contains("goldfish")) return "어항에서 탈출한 금붕어. 반짝이는 비늘이 아름답습니다.";
            if (id.Contains("guppy")) return "형형색색의 꼬리를 가진 가장 작은 열대어 중 하나.";

            if (id.Contains("salmon_king")) return "왕연어. 연어 중에서도 가장 크고 힘이 셉니다.";
            if (id.Contains("salmon")) return "태어난 강으로 돌아오는 신기한 물고기. 연어 회로도 유명하죠.";
            if (id.Contains("trout_rainbow")) return "무지개처럼 화려한 색채의 송어. 깨끗한 물에서만 삽니다.";
            if (id.Contains("_001") || id.Contains("fish_1")) return "작고 귀여운 민물고기. 초보 낚시꾼의 첫 번째 물고기로 안성맞춤입니다.";

            return "평범하지만 당신의 낚시 솜씨를 증명하는 특별한 물고기입니다.";
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
                "★★★★★" => (200f, 800f), "★★★★" => (100f, 250f), "★★★" => (50f, 120f),
                "★★" => (20f, 60f), "★" => (5f, 25f), _ => (10f, 50f)
            };
        }

        [MenuItem("Tools/Excel/3. Convert Rods Sheet to SO")]
        public static void ConvertRodsToSO()
        {
            if (!File.Exists(ExcelPath)) return;

            if (!Directory.Exists(RodResourcePath))
                Directory.CreateDirectory(RodResourcePath);

            using (FileStream file = new FileStream(ExcelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                IWorkbook workbook = new XSSFWorkbook(file);
                ISheet sheet = workbook.GetSheet("Rods");
                if (sheet == null)
                {
                    EditorUtility.DisplayDialog("Error", "Rods sheet not found in FishData.xlsx", "OK");
                    return;
                }

                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    IRow row = sheet.GetRow(i);
                    if (row == null || row.GetCell(0) == null) continue;

                    string id = GetStringValue(row.GetCell(0));
                    string assetPath = Path.Combine(RodResourcePath, $"{id}.asset");

                    RodDataSO rodData = AssetDatabase.LoadAssetAtPath<RodDataSO>(assetPath);
                    if (rodData == null)
                    {
                        rodData = ScriptableObject.CreateInstance<RodDataSO>();
                        AssetDatabase.CreateAsset(rodData, assetPath);
                    }

                    rodData.id = id;
                    rodData.rodName = GetStringValue(row.GetCell(1));
                    rodData.rank = GetStringValue(row.GetCell(2));
                    rodData.price = (int)GetNumericValue(row.GetCell(3));
                    rodData.castDistanceBonus = GetNumericValue(row.GetCell(4));
                    rodData.catchChanceBonus = GetNumericValue(row.GetCell(5));
                    rodData.durability = GetNumericValue(row.GetCell(6));
                    rodData.description = GetStringValue(row.GetCell(7));

                    EditorUtility.SetDirty(rodData);
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("Rods SO conversion complete.");
            }
        }

        [MenuItem("Tools/Excel/4. Convert Baits Sheet to SO")]
        public static void ConvertBaitsToSO()
        {
            if (!File.Exists(ExcelPath)) return;

            if (!Directory.Exists(BaitResourcePath))
                Directory.CreateDirectory(BaitResourcePath);

            using (FileStream file = new FileStream(ExcelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                IWorkbook workbook = new XSSFWorkbook(file);
                ISheet sheet = workbook.GetSheet("Baits");
                if (sheet == null)
                {
                    EditorUtility.DisplayDialog("Error", "Baits sheet not found in FishData.xlsx", "OK");
                    return;
                }

                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    IRow row = sheet.GetRow(i);
                    if (row == null || row.GetCell(0) == null) continue;

                    string id = GetStringValue(row.GetCell(0));
                    string assetPath = Path.Combine(BaitResourcePath, $"{id}.asset");

                    BaitDataSO baitData = AssetDatabase.LoadAssetAtPath<BaitDataSO>(assetPath);
                    if (baitData == null)
                    {
                        baitData = ScriptableObject.CreateInstance<BaitDataSO>();
                        AssetDatabase.CreateAsset(baitData, assetPath);
                    }

                    baitData.id = id;
                    baitData.baitName = GetStringValue(row.GetCell(1));
                    baitData.rank = GetStringValue(row.GetCell(2));
                    baitData.price = (int)GetNumericValue(row.GetCell(3));

                    string attractionStr = GetStringValue(row.GetCell(4));
                    if (string.IsNullOrEmpty(attractionStr) || attractionStr == "all")
                        baitData.attractionFishIds = Array.Empty<string>();
                    else
                        baitData.attractionFishIds = attractionStr.Split(',');

                    baitData.catchChanceBonus = GetNumericValue(row.GetCell(5));
                    baitData.description = GetStringValue(row.GetCell(6));

                    EditorUtility.SetDirty(baitData);
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("Baits SO conversion complete.");
            }
        }
    }
}

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MyDebugTool : EditorWindow
{
    [Header("Test Items")]
    private ItemData[] testItems;
    private int selectedItemIndex = 0;
    private Vector2 scrollPosition;
    
    [Header("Player Stats")]
    private int inputLevel = 1;
    private int inputGold = 1000;
    private int inputHp = 100;
    private int inputAttackPower = 10;
    
    [Header("Quick Actions")]
    private int quickAddCount = 1;
    
    /// <summary>
    /// 메뉴에서 창 열기
    /// </summary>
    [MenuItem("Tools/My Debug Tool")]
    public static void OpenWindow()
    {
        MyDebugTool window = GetWindow<MyDebugTool>("Debug Tool");
        window.minSize = new Vector2(400, 600);
        window.Show();
    }
    
    /// <summary>
    /// 창이 열릴 때 초기화
    /// </summary>
    private void OnEnable()
    {
        LoadTestItems();
    }
    
    /// <summary>
    /// GUI 그리기
    /// </summary>
    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        DrawHeader();
        DrawItemSection();
        DrawStatsSection();
        DrawQuickActions();
        DrawInventoryInfo();
        
        EditorGUILayout.EndScrollView();
    }
    
    /// <summary>
    /// 헤더 그리기
    /// </summary>
    private void DrawHeader()
    {
        EditorGUILayout.Space(10);
        
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 18;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        
        EditorGUILayout.LabelField("🛠️ My Debug Tool", titleStyle);
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space(10);
    }
    
    /// <summary>
    /// 아이템 섹션 그리기
    /// </summary>
    private void DrawItemSection()
    {
        EditorGUILayout.LabelField("📦 Item Management", EditorStyles.boldLabel);
        
        using (new EditorGUILayout.VerticalScope("box"))
        {
            // 테스트 아이템 새로고침
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("🔄 Refresh Items", GUILayout.Height(25)))
                {
                    LoadTestItems();
                }
                
                if (testItems != null)
                {
                    EditorGUILayout.LabelField($"({testItems.Length} items found)");
                }
            }
            
            EditorGUILayout.Space(5);
            
            // 아이템 선택
            if (testItems != null && testItems.Length > 0)
            {
                string[] itemNames = new string[testItems.Length];
                for (int i = 0; i < testItems.Length; i++)
                {
                    if (testItems[i] != null)
                    {
                        itemNames[i] = $"[{testItems[i].itemRarity}] {testItems[i].itemName}";
                    }
                    else
                    {
                        itemNames[i] = "Missing Item";
                    }
                }
                
                selectedItemIndex = EditorGUILayout.Popup("Select Item:", selectedItemIndex, itemNames);
                
                EditorGUILayout.Space(5);
                
                // 아이템 추가 버튼들
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("➕ Add Item", GUILayout.Height(30)))
                    {
                        AddSelectedItem();
                    }
                    
                    quickAddCount = EditorGUILayout.IntField(quickAddCount, GUILayout.Width(50));
                    
                    if (GUILayout.Button($"➕ Add {quickAddCount}x", GUILayout.Height(30)))
                    {
                        for (int i = 0; i < quickAddCount; i++)
                        {
                            AddSelectedItem();
                        }
                    }
                }
                
                EditorGUILayout.Space(5);
                
                // 빠른 액션들
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("🎲 Add Random", GUILayout.Height(25)))
                    {
                        AddRandomItem();
                    }
                    
                    if (GUILayout.Button("📦 Add All", GUILayout.Height(25)))
                    {
                        AddAllItems();
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No ItemData found! Create some ItemData assets first.", MessageType.Warning);
            }
            
            EditorGUILayout.Space(5);
            
            // 인벤토리 관리
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("🗑️ Clear Inventory", GUILayout.Height(25)))
                {
                    if (EditorUtility.DisplayDialog("Clear Inventory", "Are you sure you want to clear all items?", "Yes", "No"))
                    {
                        ClearInventory();
                    }
                }
                
                if (GUILayout.Button("🔧 Unequip All", GUILayout.Height(25)))
                {
                    UnequipAllItems();
                }
            }
        }
        
        EditorGUILayout.Space(10);
    }
    
    /// <summary>
    /// 스탯 섹션 그리기
    /// </summary>
    private void DrawStatsSection()
    {
        EditorGUILayout.LabelField("📊 Player Stats", EditorStyles.boldLabel);
        
        using (new EditorGUILayout.VerticalScope("box"))
        {
            inputLevel = EditorGUILayout.IntField("Level:", inputLevel);
            inputGold = EditorGUILayout.IntField("Gold:", inputGold);
            inputHp = EditorGUILayout.IntField("HP:", inputHp);
            inputAttackPower = EditorGUILayout.IntField("Attack Power:", inputAttackPower);
            
            EditorGUILayout.Space(5);
            
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("⚡ Set Stats", GUILayout.Height(30)))
                {
                    SetPlayerStats();
                }
                
                if (GUILayout.Button("🔄 Get Current", GUILayout.Height(30)))
                {
                    GetCurrentStats();
                }
            }
        }
        
        EditorGUILayout.Space(10);
    }
    
    /// <summary>
    /// 빠른 액션 섹션
    /// </summary>
    private void DrawQuickActions()
    {
        EditorGUILayout.LabelField("⚡ Quick Actions", EditorStyles.boldLabel);
        
        using (new EditorGUILayout.VerticalScope("box"))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("🎮 Start Game", GUILayout.Height(25)))
                {
                    EditorApplication.isPlaying = true;
                }
                
                if (GUILayout.Button("⏸️ Stop Game", GUILayout.Height(25)))
                {
                    EditorApplication.isPlaying = false;
                }
            }
            
            if (GUILayout.Button("📋 Print Inventory", GUILayout.Height(25)))
            {
                PrintInventoryStatus();
            }
        }
        
        EditorGUILayout.Space(10);
    }
    
    /// <summary>
    /// 인벤토리 정보 표시
    /// </summary>
    private void DrawInventoryInfo()
    {
        EditorGUILayout.LabelField("ℹ️ Inventory Info", EditorStyles.boldLabel);
        
        using (new EditorGUILayout.VerticalScope("box"))
        {
            if (Application.isPlaying)
            {
                Player player = CharacterManager.Player;
                if (player?.Inventory != null)
                {
                    int emptySlots = player.Inventory.GetEmptySlotCount();
                    int totalSlots = player.Inventory.InventorySize;
                    int usedSlots = totalSlots - emptySlots;
                    
                    EditorGUILayout.LabelField($"Slots Used: {usedSlots} / {totalSlots}");
                    
                    // 진행률 바
                    float fillRatio = (float)usedSlots / totalSlots;
                    Rect rect = EditorGUILayout.GetControlRect(false, 20);
                    EditorGUI.ProgressBar(rect, fillRatio, $"{usedSlots}/{totalSlots}");
                }
                else
                {
                    EditorGUILayout.HelpBox("Player or Inventory not found!", MessageType.Warning);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Game is not running. Start play mode to see inventory info.", MessageType.Info);
            }
        }
    }
    
    /// <summary>
    /// 테스트 아이템들 로드
    /// </summary>
    private void LoadTestItems()
    {
        string[] guids = AssetDatabase.FindAssets("t:ItemData");
        testItems = new ItemData[guids.Length];
        
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            testItems[i] = AssetDatabase.LoadAssetAtPath<ItemData>(path);
        }
        
        Debug.Log($"[Debug Tool] {testItems.Length} ItemData assets loaded");
    }
    
    /// <summary>
    /// 선택된 아이템 추가
    /// </summary>
    private void AddSelectedItem()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Error", "Game must be running!", "OK");
            return;
        }
        
        if (testItems == null || selectedItemIndex >= testItems.Length) return;
        
        ItemData item = testItems[selectedItemIndex];
        if (item == null) return;
        
        Player player = CharacterManager.Player;
        if (player?.Inventory != null)
        {
            bool success = player.Inventory.AddItem(item);
            Debug.Log($"[Debug Tool] {(success ? "Added" : "Failed to add")}: {item.itemName}");
        }
    }
    
    /// <summary>
    /// 랜덤 아이템 추가
    /// </summary>
    private void AddRandomItem()
    {
        if (testItems == null || testItems.Length == 0) return;
        
        selectedItemIndex = Random.Range(0, testItems.Length);
        AddSelectedItem();
    }
    
    /// <summary>
    /// 모든 아이템 추가
    /// </summary>
    private void AddAllItems()
    {
        if (!Application.isPlaying) return;
        
        Player player = CharacterManager.Player;
        if (player?.Inventory == null) return;
        
        int addedCount = 0;
        foreach (var item in testItems)
        {
            if (item != null && player.Inventory.AddItem(item))
            {
                addedCount++;
            }
        }
        
        Debug.Log($"[Debug Tool] Added {addedCount} items to inventory");
    }
    
    /// <summary>
    /// 인벤토리 클리어
    /// </summary>
    private void ClearInventory()
    {
        if (!Application.isPlaying) return;
        
        Player player = CharacterManager.Player;
        if (player?.Inventory == null) return;
        
        InventorySlot[] slots = player.Inventory.GetAllSlots();
        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].IsEmpty)
            {
                slots[i].Clear();
                // 이벤트 호출 제거 - static event는 직접 호출하지 않음
                // Inventory 클래스 내부에서 처리하도록 함
            }
        }
        
        Debug.Log("[Debug Tool] Inventory cleared");
    }
    
    /// <summary>
    /// 모든 아이템 장착 해제
    /// </summary>
    private void UnequipAllItems()
    {
        if (!Application.isPlaying) return;
        
        Player player = CharacterManager.Player;
        if (player?.Inventory == null) return;
        
        InventorySlot[] slots = player.Inventory.GetAllSlots();
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].isEquipped)
            {
                player.Inventory.UseItem(i); // 장착 해제
            }
        }
        
        Debug.Log("[Debug Tool] All items unequipped");
    }
    
    /// <summary>
    /// 플레이어 스탯 설정
    /// </summary>
    private void SetPlayerStats()
    {
        if (!Application.isPlaying) return;
        
        Player player = CharacterManager.Player;
        if (player?.StatHandler == null) return;
        
        player.StatHandler.SetBaseStat(StatType.Level, inputLevel);
        player.StatHandler.SetBaseStat(StatType.Gold, inputGold);
        player.StatHandler.SetBaseStat(StatType.Hp, inputHp);
        player.StatHandler.SetBaseStat(StatType.AttackPower, inputAttackPower);
        
        Debug.Log($"[Debug Tool] Stats set - Level: {inputLevel}, Gold: {inputGold}, HP: {inputHp}, Attack: {inputAttackPower}");
    }
    
    /// <summary>
    /// 현재 스탯 가져오기
    /// </summary>
    private void GetCurrentStats()
    {
        if (!Application.isPlaying) return;
        
        Player player = CharacterManager.Player;
        if (player?.StatHandler == null) return;
        
        inputLevel = player.StatHandler.GetStat(StatType.Level);
        inputGold = player.StatHandler.GetStat(StatType.Gold);
        inputHp = player.StatHandler.GetStat(StatType.Hp);
        inputAttackPower = player.StatHandler.GetStat(StatType.AttackPower);
        
        Debug.Log("[Debug Tool] Current stats loaded");
    }
    
    /// <summary>
    /// 인벤토리 상태 출력
    /// </summary>
    private void PrintInventoryStatus()
    {
        if (!Application.isPlaying) return;
        
        Player player = CharacterManager.Player;
        if (player?.Inventory == null) return;
        
        InventorySlot[] slots = player.Inventory.GetAllSlots();
        int itemCount = 0;
        
        Debug.Log("=== Inventory Status ===");
        
        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].IsEmpty)
            {
                ItemData item = slots[i].itemData;
                string equipped = slots[i].isEquipped ? " [EQUIPPED]" : "";
                Debug.Log($"Slot {i}: {item.itemName} ({item.itemRarity}){equipped}");
                itemCount++;
            }
        }
        
        Debug.Log($"Total Items: {itemCount}/{slots.Length}");
    }
}
#endif
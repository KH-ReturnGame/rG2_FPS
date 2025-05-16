using System.Collections.Generic;
using UnityEngine;

public class ControlManager : MonoBehaviour
{
    // Singleton instance
    public static ControlManager Instance { get; private set; }
    
    // Dictionary to track unlocked controls
    private HashSet<KeyCode> unlockedControls = new HashSet<KeyCode>();
    
    // Default unlocked controls (can be empty if all controls start locked)
    [SerializeField] private KeyCode[] defaultUnlockedControls = { };
    
    // Control categories for better organization (optional)
    [System.Serializable]
    public class ControlCategory
    {
        public string categoryName;
        public KeyCode[] controls;
    }
    [SerializeField] private ControlCategory[] controlCategories;
    
    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        
        // Unlock default controls
        foreach (KeyCode control in defaultUnlockedControls)
        {
            unlockedControls.Add(control);
        }
    }
    
    // Method to unlock a new control
    public void UnlockControl(KeyCode control)
    {
        if (!unlockedControls.Contains(control))
        {
            unlockedControls.Add(control);
            Debug.Log($"ControlManager: Unlocked control {control}");
            
            // Optionally - trigger an event or UI update
            // OnControlUnlocked?.Invoke(control);
        }
    }
    
    // Method to check if a control is unlocked
    public bool IsControlUnlocked(KeyCode control)
    {
        return unlockedControls.Contains(control);
    }
    
    // Method to get all unlocked controls
    public KeyCode[] GetUnlockedControls()
    {
        KeyCode[] result = new KeyCode[unlockedControls.Count];
        unlockedControls.CopyTo(result);
        return result;
    }
    
    // Method to check unlocked controls in a category
    public int GetUnlockedCountInCategory(string categoryName)
    {
        foreach (ControlCategory category in controlCategories)
        {
            if (category.categoryName == categoryName)
            {
                int count = 0;
                foreach (KeyCode control in category.controls)
                {
                    if (unlockedControls.Contains(control))
                    {
                        count++;
                    }
                }
                return count;
            }
        }
        return 0;
    }
    
    // Method to check if a category is fully unlocked
    public bool IsCategoryFullyUnlocked(string categoryName)
    {
        foreach (ControlCategory category in controlCategories)
        {
            if (category.categoryName == categoryName)
            {
                foreach (KeyCode control in category.controls)
                {
                    if (!unlockedControls.Contains(control))
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        return false;
    }
    
    // Reset all controls to default (useful for game restart)
    public void ResetToDefault()
    {
        unlockedControls.Clear();
        foreach (KeyCode control in defaultUnlockedControls)
        {
            unlockedControls.Add(control);
        }
        Debug.Log("ControlManager: Reset all controls to default");
    }
}
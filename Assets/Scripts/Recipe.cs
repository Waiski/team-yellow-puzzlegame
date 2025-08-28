
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;


[System.Serializable]
public class RecipeItem
{
	public string itemName;
	public int requiredCount;
}

public class Recipe : MonoBehaviour
{
    public List<RecipeItem> requiredItems = new();

    public GameController gameController;
    public Text recipeText;

    private Dictionary<string, int> collectedItems = new();

    private void Awake()
    {
        // Initialize collectedItems dictionary with all required item names
        foreach (var item in requiredItems)
        {
            if (!collectedItems.ContainsKey(item.itemName))
                collectedItems[item.itemName] = 0;
        }
        UpdateRecipeText();
    }

    public void AddCollectedItems(string itemName, int count)
    {
        if (collectedItems.ContainsKey(itemName))
        {
            collectedItems[itemName] += count;
            if (CheckWinCondition())
            {
                recipeText.text = "Recipe: Completed!";
                gameController.GameWon();
            } else
            {
                UpdateRecipeText();
            }
        }
    }

    private void UpdateRecipeText()
    {
        string text = "Recipe:";
        foreach (var item in requiredItems)
        {
            int collected = collectedItems.ContainsKey(item.itemName) ? collectedItems[item.itemName] : 0;
            int remaining = Mathf.Max(item.requiredCount - collected, 0);
            text += $" {item.itemName}: {remaining} left";
        }
        recipeText.text = text;
    }

    private bool CheckWinCondition()
    {
        foreach (var item in requiredItems)
        {
            int collected = collectedItems.ContainsKey(item.itemName) ? collectedItems[item.itemName] : 0;
            if (collected < item.requiredCount) return false;
        }
        return true;
    }
}

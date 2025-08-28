using UnityEngine;
using UnityEngine.UI;

public class GameplayUIController : MonoBehaviour
{
    [SerializeField] Image[] _goalImages;
    [SerializeField] GameObject[] _completed;
    [SerializeField] Recipe _recipe;
    
    void Start()
    {
        for (var i = 0; i < _recipe.requiredItems.Count; i++)
        {
            _goalImages[i].sprite = _recipe.requiredItems[i].Sprite;
        }
    }
    
    public void UpdateGoalRequirements(int i, int required)
    {
        _completed[i].SetActive(required <= 0);
    }
}

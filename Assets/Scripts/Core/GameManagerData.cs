using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "GameManager Data", menuName = "Core/Game Manager Data", order = 1)]
    public class GameManagerData : ScriptableObject
    {
        [Scene] public string uiSceneName;
        [Scene] public string[] LevelsSceneName;

    }
}
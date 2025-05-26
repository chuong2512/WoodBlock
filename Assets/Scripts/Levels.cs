using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Levels : ScriptableObject,IEnumerable<ILevel>
{
    public const string DEFAULT_NAME = nameof(Levels);

    [SerializeField]private List<Level> _levels = new List<Level>();

    public IEnumerator<ILevel> GetEnumerator() => _levels.Select(level => (ILevel)level).OrderBy(lvl=>lvl.LevelNo).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public ILevel this[int lvlNo] => _levels.FirstOrDefault(level => level.LevelNo == lvlNo);

    public bool HasLevel(int lvlNo) => this[lvlNo].Shapes != null;

    public void AddOrUpdateLevel(ILevel level)
    {
        var index = _levels.FindIndex(l => l.LevelNo == level.LevelNo);
        if(index!=-1)
        {
            _levels.RemoveAt(index);
        
        _levels.Insert(index, new Level(level));
        }
        else
        {
            _levels.Add(new Level(level));
        }

        _levels = _levels.OrderBy(l => l.LevelNo).ToList();
        MarkAsDirty();
    }

    private void MarkAsDirty()
    {

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    public void RemoveLevel(int lvlNo)
    {
        _levels.RemoveAll(level => level.LevelNo == lvlNo);
        MarkAsDirty();
    }


#if UNITY_EDITOR
    [UnityEditor.MenuItem("MyGames/Levels")]
    public static void OpenPlayerSkin()
    {
        GamePlayEditorManager.OpenScriptableAtDefault<Levels>();
    }
#endif
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;
using System.IO;

public class Database : ScriptableObject
{
    public static Database Instance
    {
        get
        {
            if(instance == null)
            {
                var db = Resources.Load<Database>("Database");
                if(db == null)
                {
                    // 리소스 폴더에 데이터베이스가 없다면 Asset을 생성한다.
                    db = CreateInstance<Database>();                          // scriptable 오브젝트를 생성하라!
                    string rootPath = Path.Combine(Application.dataPath, "Resources");  // Resources폴더 경로.
                    if (!Directory.Exists(rootPath))
                        Directory.CreateDirectory(rootPath);

                    // root경로가 unity프로젝트이기 때문에 os기준인 rootPath를 사용하면 안된다.
                    AssetDatabase.CreateAsset(db, "Assets/Resources/database.asset");   // 인스턴스 객체를 path경로에 .asset으로 (디스크)써라
                    AssetDatabase.Refresh();                                            // Asset폴더를 새로고침 하세요! (안그러면 바로 안 보임)
                }

                instance = db;
            }

            return instance;
        }
    }

    static Database instance;
    public Database()
    {
        ammoData = new AmmoData();
    }


    public AmmoData ammoData;
}

[Serializable]
public class AmmoData
{
    [Serializable]
    public class Entry
    {
        public string name;     // 명단 이름
        public int id;          // 명단 id
        public Entry(string name, int id)
        {
            this.name = name;
            this.id = id;
        }
    }

    public Entry[] entries;

    public AmmoData()
    {
        entries = new Entry[0];
    }

    public void AddEntry(string name)
    {
        if(Array.FindIndex(entries, (e => e.name == name)) >= 0)
        {
            Debug.Log($"entry에 동일한 name이 존재합니다 : {name}");
            return;
        }

        var ids = entries.Select(e => e.id);                // 기존 entry의 id집합.
        var range = Enumerable.Range(0, ids.Max() + 2);     // 0부터 가장 큰 id + 1까지의 배열.
        int id = range.Except(ids).First();                 // free id 검색.

        ArrayUtility.Add(ref entries, new Entry(name, id)); // 배열에 새 값 추가.
    }
    public void RemoveEntry(Entry entry)
    {
        ArrayUtility.Remove(ref entries, entry);
    }
}



// 에디터 상의 Inspector, Scene, Game등은 모두 EditorWindow다.
public class DatabaseEditor : EditorWindow
{
    Database db;
    SerializedObject target;

    [MenuItem("Game/Database")]
    static void OpenWindow()
    {
        GetWindow<DatabaseEditor>();
    }
    private void OnEnable()
    {
        db = Database.Instance;
        target = new SerializedObject(db);
    }

    private void OnGUI()
    {
        GUILayout.Label("탄약 DB");
        AmmoData.Entry delete = null;
        foreach(AmmoData.Entry e in db.ammoData.entries)
        {
            GUILayout.BeginHorizontal();
            GUILayout.TextField(e.name);
            if(GUILayout.Button("-", GUILayout.Width(64)))
                delete = e;
            GUILayout.EndHorizontal();
        }
        if (delete != null)
            ArrayUtility.Remove(ref db.ammoData.entries, delete);

        if (GUILayout.Button("Add Ammo Type"))
        {
            db.ammoData.AddEntry("new Ammo");
            EditorUtility.SetDirty(db);
        }
    }
}
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
                    // ���ҽ� ������ �����ͺ��̽��� ���ٸ� Asset�� �����Ѵ�.
                    db = CreateInstance<Database>();                          // scriptable ������Ʈ�� �����϶�!
                    string rootPath = Path.Combine(Application.dataPath, "Resources");  // Resources���� ���.
                    if (!Directory.Exists(rootPath))
                        Directory.CreateDirectory(rootPath);

                    // root��ΰ� unity������Ʈ�̱� ������ os������ rootPath�� ����ϸ� �ȵȴ�.
                    AssetDatabase.CreateAsset(db, "Assets/Resources/database.asset");   // �ν��Ͻ� ��ü�� path��ο� .asset���� (��ũ)���
                    AssetDatabase.Refresh();                                            // Asset������ ���ΰ�ħ �ϼ���! (�ȱ׷��� �ٷ� �� ����)
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
        public string name;     // ��� �̸�
        public int id;          // ��� id
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
            Debug.Log($"entry�� ������ name�� �����մϴ� : {name}");
            return;
        }

        var ids = entries.Select(e => e.id);                // ���� entry�� id����.
        var range = Enumerable.Range(0, ids.Max() + 2);     // 0���� ���� ū id + 1������ �迭.
        int id = range.Except(ids).First();                 // free id �˻�.

        ArrayUtility.Add(ref entries, new Entry(name, id)); // �迭�� �� �� �߰�.
    }
    public void RemoveEntry(Entry entry)
    {
        ArrayUtility.Remove(ref entries, entry);
    }
}



// ������ ���� Inspector, Scene, Game���� ��� EditorWindow��.
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
        GUILayout.Label("ź�� DB");
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
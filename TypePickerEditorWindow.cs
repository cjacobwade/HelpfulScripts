// Taken from kenlane22's comment here: http://answers.unity3d.com/questions/30382/editor-assembly.html

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
 
 
public class TypePickerEditorWindow : EditorWindow 
{
    Vector2 scrollPos, scrollPos2;
    List<bool> checkBools = new List<bool>();
    string filter = "";
 
    class NameAndType
    {
        public NameAndType( string n, Type t ) { name = n; type = t; }
        public string name;
        public Type type;
    }
 
    List<NameAndType> typeList = new List<NameAndType>();   
    Type m_currType = null;
 
    void Awake()
    {
 
        // List all C# class Types that are subclasses of Component
        typeList.Clear();
        foreach( Type type in GetAllSubTypes( typeof(Component) ) )
        {
            typeList.Add( new NameAndType( type.Name, type ) );
        }
        typeList.Sort( delegate(NameAndType a, NameAndType b) { return a.name.CompareTo(b.name); });
    }
 
    public static System.Type[] GetAllSubTypes(System.Type aBaseClass)
    {
        var result = new System.Collections.Generic.List<System.Type>();
        System.Reflection.Assembly[] AS = System.AppDomain.CurrentDomain.GetAssemblies();
        foreach (var A in AS)
        {
            System.Type[] types = A.GetTypes();
            foreach (var T in types)
            {
                if (T.IsSubclassOf(aBaseClass))
                    result.Add(T);
            }
        }
        return result.ToArray();
    }
 
    void OnGUI() 
    {
        GUILayout.Label( "Pick a Component type, then click buttons to select GameObjects." );
        GUILayout.BeginHorizontal();
        {
            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label( "Filter:", GUILayout.Width(45) );
                    filter = GUILayout.TextField( filter );
                }
                GUILayout.EndHorizontal();
                scrollPos2 = GUILayout.BeginScrollView( scrollPos2, GUILayout.Width(230)  );
                {
                    int n = typeList.Count;
                    for( int i = 0; i < n; i++ )
                    {
                        string nm = typeList[i].name;
                        GUI.enabled = (m_currType != typeList[i].type); // disable selected one
                        if     ( 
                              ( nm.ToUpper().Contains( filter.ToUpper() ) ) // filtered list wont bother drawning the filtered
                            && 
                                 ( GUILayout.Button( nm ) )  // button pushed!
                            )
                        {
                            m_currType = typeList[i].type;
                            // And set up checkboxes to all false
                            UnityEngine.Object[] ueoList = FindSceneObjectsOfType( m_currType );
                            checkBools.Clear();
                            for (int j = 0; j< ueoList.Length; j++) 
                                checkBools.Add( false );
                        }
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();
 
            GUILayout.BeginVertical();
            {
                if ( m_currType == null)
                {
                    GUILayout.Label( "Pick a type in the left column." );
                }
                else
                {
                    GUILayout.Label( "All GameObjects with the class: "+ m_currType.Name );
                    scrollPos = GUILayout.BeginScrollView( scrollPos );
                    {
                        int i = 0;
                        UnityEngine.Object[] ueoList = FindSceneObjectsOfType( m_currType );
                        foreach( UnityEngine.Object o in ueoList )
                        {
                            GUILayout.BeginHorizontal();
                            {
                                Component com = o as Component;
                                if ( (com != null) && (com.gameObject != null) )
                                { 
                                    //checkBools[i] = GUILayout.Toggle( checkBools[i],"", GUILayout.Width(20) );
                                    if (  GUILayout.Button( NameWithParent( com.gameObject ) )    )
                                    {
                                        Selection.activeGameObject = com.gameObject;
                                    }
                                    if (  GUILayout.Button( "Show", GUILayout.Width(50) )  )
                                    {
                                        GameObject g = Selection.activeGameObject;
                                        Selection.activeGameObject = com.gameObject;
                                        if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.FrameSelected();
                                        Selection.activeGameObject = g;
                                    }
                                }
                                i++;
                            }   
                            GUILayout.EndHorizontal();
                        }
                    }
                    GUILayout.EndScrollView();
                }
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal(); 
    }
 
    string NameWithParent( GameObject g )
    {
        return g.name;
        //string s;
        //if (g.transform.parent != null) return "~/"+ g.transform.parent.name +"/"+ g.name;
        //else                          return g.name;
    }
 
    [MenuItem ("Window/TypePickerEditorWindow")]
    [MenuItem ("GSE/TypePickerEditorWindow")]
    static public void Init() 
    {
        TypePickerEditorWindow editorWindow = GetWindow(typeof(TypePickerEditorWindow)) as TypePickerEditorWindow;
        editorWindow.autoRepaintOnSceneChange = true;
        editorWindow.Show();
    }
}

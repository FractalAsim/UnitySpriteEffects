using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;

enum DRAWOPTION {select, paint, paintover, erase};

public class TileWindow : EditorWindow
{

	private static float minimumGridSize = 0.05f;

	private static bool isEnabled;

	//For grid scrolling
	private Vector2 _scrollPos;

	


	private static bool isDraw;
	
	private static bool isObjmode;

	private static DRAWOPTION selected;
	
	private static int layerOrd;
	
	

	
	
	

	private static Sprite selectedSprite;
	private static GameObject activeGo;
	public GUIStyle textureStyle;
	public GUIStyle textureStyleAct;


	

	//ui vars
	//The tilemap currently viewing
	int tilemapselectindex;
	private static Sprite[] spritesoftilemap;
	//Grid stuff
	private static bool gridtoggle;
	private static Vector2 gridSize;
	






	private static bool addBoxCollider = false;
	private static GameObject parentGO;
	private static string tagName = "Untagged";

	[MenuItem("Tools/TilemapEditor")]
	private static void TilemapEditor()
	{
		EditorWindow.GetWindow(typeof (TileWindow));
	}

	public void OnInspectorUpdate()
	{
		// This will only get called 10 times per second.
		Repaint();
	}

	void OnEnable() 
	{
		isEnabled = true;
		Editor.CreateInstance(typeof(SceneViewEventHandler));
	}

	void OnDestroy() 
	{
		isEnabled = false;
	}
	
	public class SceneViewEventHandler : Editor
	{
		static SceneViewEventHandler()
		{
			SceneView.onSceneGUIDelegate += OnSceneGUI;
		}


		static void OnSceneGUI(SceneView aView) //Update of Tilehandler
		{
			Event e = Event.current;
			switch (e.type) 
			{
				case EventType.KeyDown:
					if (e.shift)
					{
						switch (e.keyCode) 
						{
							case KeyCode.Q:
								selected = DRAWOPTION.select;
								break;
							case KeyCode.W:
								selected = DRAWOPTION.paint;
								break;
							case KeyCode.E:
								selected = DRAWOPTION.paintover;
								break;
							case KeyCode.R:
								selected = DRAWOPTION.erase;
								break;
							case KeyCode.D:
								isDraw = !isDraw;
								break;
							case KeyCode.G:
								gridtoggle = !gridtoggle;
								break;
						}
					}
					break;
			}

			if (isEnabled)
			{
				if(isDraw)
				{
					if (selected != DRAWOPTION.select)
					{
						HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
						if ((e.type == EventType.MouseDrag || e.type == EventType.MouseDown) && e.button == 0 && selectedSprite != null)
						{
							Vector2 mousePos = Event.current.mousePosition;
							mousePos.y = SceneView.currentDrawingSceneView.camera.pixelHeight - mousePos.y;
							Vector3 mouseWorldPos = SceneView.currentDrawingSceneView.camera.ScreenPointToRay(mousePos).origin;
							mouseWorldPos.z = layerOrd;


							// Get the current gridbox pos where the mouse is at
							if (gridSize.x > minimumGridSize && gridSize.y > minimumGridSize)
							{
								mouseWorldPos.x = Mathf.Floor(mouseWorldPos.x / gridSize.x) * gridSize.x + gridSize.x / 2.0f;
								mouseWorldPos.y = Mathf.Ceil(mouseWorldPos.y / gridSize.y) * gridSize.y - gridSize.y / 2.0f;
							}



							if(isObjmode)
								mouseWorldPos.z = mouseWorldPos.y + (selectedSprite.bounds.size.y / -2.0f);



								
							GameObject[] allgo = GameObject.FindObjectsOfType(typeof (GameObject)) as GameObject[];
							int brk = 0;


							if (selected == DRAWOPTION.paint)
							{

								
								for (int i = 0; i < allgo.Length;i++)
								{
									//Check if the current gridbox where the mouse is at is occupied by a gameobject
									if (Mathf.Approximately(allgo[i].transform.position.x, mouseWorldPos.x) 
										&& Mathf.Approximately(allgo[i].transform.position.y, mouseWorldPos.y) 
										&& Mathf.Approximately(allgo[i].transform.position.z, mouseWorldPos.z))
									{
										brk++;
										break;
									}
								}

								// Not Occupied: Create new GameObpject
								if (brk == 0)
								{
									GameObject newGO = new GameObject(selectedSprite.name, typeof(SpriteRenderer));
									newGO.transform.position = mouseWorldPos;
									newGO.GetComponent<SpriteRenderer>().sprite = selectedSprite;
									newGO.tag = tagName;

									if (parentGO != null)
										newGO.transform.parent = parentGO.transform;

									if (addBoxCollider)
										newGO.AddComponent<BoxCollider2D>();
								}
							}


							else if (selected == DRAWOPTION.paintover)
							{
								for (int i = 0; i < allgo.Length;i++)
								{
									if (allgo[i].GetComponent<SpriteRenderer>() != null & Mathf.Approximately(allgo[i].transform.position.x, mouseWorldPos.x) && Mathf.Approximately(allgo[i].transform.position.y, mouseWorldPos.y) && Mathf.Approximately(allgo[i].transform.position.z, mouseWorldPos.z))
									{
										allgo[i].GetComponent<SpriteRenderer>().sprite = selectedSprite;
										brk++;
									}
								}
								if (brk == 0)
								{
									GameObject newgo = new GameObject(selectedSprite.name, typeof(SpriteRenderer));
									newgo.transform.position = mouseWorldPos;
									newgo.GetComponent<SpriteRenderer>().sprite = selectedSprite;
									newgo.tag = tagName;
									if (addBoxCollider)
										newgo.AddComponent<BoxCollider2D>();
								}
							}
							else if (selected == DRAWOPTION.erase)
							{
								for (int i = 0; i < allgo.Length;i++)
								{
									if (Mathf.Approximately(allgo[i].transform.position.x, mouseWorldPos.x) && Mathf.Approximately(allgo[i].transform.position.y, mouseWorldPos.y) && Mathf.Approximately(allgo[i].transform.position.z, mouseWorldPos.z))
										GameObject.DestroyImmediate(allgo[i]);
								}
							}
						}
					}
					else
					{
						HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
						Vector2 mousePos = Event.current.mousePosition;
						mousePos.y = SceneView.currentDrawingSceneView.camera.pixelHeight - mousePos.y;
						Vector3 mouseWorldPos = SceneView.currentDrawingSceneView.camera.ScreenPointToRay(mousePos).origin;
						mouseWorldPos.z = layerOrd;		

						if (e.type == EventType.MouseDown && e.button == 0)
						{
							Selection.activeGameObject = null;
							GameObject[] allgo = GameObject.FindObjectsOfType(typeof (GameObject)) as GameObject[];
							int brk = 0;
							for (int i = 0; i < allgo.Length;i++)
							{
								if (allgo[i].GetComponent<SpriteRenderer>() != null && allgo[i].GetComponent<SpriteRenderer>().bounds.Contains(mouseWorldPos))
								{
									brk++;
									activeGo = allgo[i];
									break;
								}
							}
							if (brk == 0)
								activeGo = null;

						}
						if (e.type == EventType.MouseDrag && e.button == 0 && activeGo != null)
						{
							if (gridSize.x > 0.05f && gridSize.y > 0.05f)
							{
								mouseWorldPos.x = Mathf.Floor(mouseWorldPos.x / gridSize.x) * gridSize.x + gridSize.x / 2.0f;
								mouseWorldPos.y = Mathf.Ceil(mouseWorldPos.y / gridSize.y) * gridSize.y - gridSize.y / 2.0f;
							}
							activeGo.transform.position = mouseWorldPos;
						}
					}
				}
			}
		}
	}

	[CustomEditor(typeof(GameObject))]
	public class SceneGUITest : Editor
	{
		[DrawGizmo(GizmoType.NotInSelectionHierarchy)]
		static void RenderCustomGizmo(Transform objectTransform, GizmoType gizmoType)
		{
			if (isEnabled && gridtoggle)
			{
				Vector3 minGrid = SceneView.currentDrawingSceneView.camera.ScreenPointToRay(new Vector2(0f, 0f)).origin;
				Vector3 maxGrid = SceneView.currentDrawingSceneView.camera.ScreenPointToRay(new Vector2(SceneView.currentDrawingSceneView.camera.pixelWidth, SceneView.currentDrawingSceneView.camera.pixelHeight)).origin;
				
				Gizmos.color = Color.white;
				//Draw column grids
				for (float i = Mathf.Round(minGrid.x / gridSize.x) * gridSize.x; i < Mathf.Round(maxGrid.x / gridSize.x) * gridSize.x + gridSize.x && gridSize.x > minimumGridSize; i+=gridSize.x)
				{
					Gizmos.DrawLine(new Vector3(i,minGrid.y,0.0f), new Vector3(i,maxGrid.y,0.0f));
				}

				//Draw row grids
				for (float j = Mathf.Round(minGrid.y / gridSize.y) * gridSize.y; j < Mathf.Round(maxGrid.y / gridSize.y) * gridSize.y + gridSize.y && gridSize.y > minimumGridSize; j+=gridSize.y)
				{
					Gizmos.DrawLine(new Vector3(minGrid.x,j,0.0f), new Vector3(maxGrid.x,j,0.0f));
				}

				SceneView.RepaintAll();
			}

			SceneView.RepaintAll();
		}

	}

	void OnGUI()
	{
		textureStyle = new GUIStyle(GUI.skin.button);
		textureStyle.margin = new RectOffset(2,2,2,2);
		textureStyle.normal.background = null;
		textureStyleAct = new GUIStyle(textureStyle);
		textureStyleAct.margin = new RectOffset(0,0,0,0);
		textureStyleAct.normal.background = textureStyle.active.background;

		// Creates a New Directory it it doesnt exist
		if (!Directory.Exists(Application.dataPath + "/Tilemaps/"))
		{
			AssetDatabase.CreateFolder("Assets", "Tilemaps");
			AssetDatabase.Refresh();
			Debug.Log("Created Tilemaps Directory,Put Tilemaps here");
		}

		EditorGUILayout.LabelField("Tile Map", GUILayout.Width(256));

		//Find all the Tilemaps under tilemaps folder PNG and JPG only
		string[] pngfiles = Directory.GetFiles(Application.dataPath + "/Tilemaps/", "*.png");
		string[] jpgfiles = Directory.GetFiles(Application.dataPath + "/Tilemaps/", "*.jpg");
		string[] tilemapList = new string[pngfiles.Length+jpgfiles.Length+1];
		for(int i = 0; i < pngfiles.Length; i++)
		{
			tilemapList[i] = pngfiles[i].Replace(Application.dataPath + "/Tilemaps/", "");
		}
		for(int i = pngfiles.Length; i < jpgfiles.Length; i++)
		{
			tilemapList[i] = jpgfiles[i].Replace(Application.dataPath + "/Tilemaps/", "");
		}
		tilemapList[pngfiles.Length+jpgfiles.Length] = "None";

		//Display the Tilemaps to choose from
		EditorGUI.BeginChangeCheck();
		tilemapselectindex = EditorGUILayout.Popup(tilemapselectindex, tilemapList, GUILayout.Width(256));
		if (EditorGUI.EndChangeCheck () || spritesoftilemap == null) 
		{
			gridtoggle = true;
			SceneView.RepaintAll();

			//Get the sprites of the selected tilemaplist available
			spritesoftilemap = AssetDatabase.LoadAllAssetsAtPath("Assets/Tilemaps/" + tilemapList[tilemapselectindex]).Select(x => x as Sprite).Where(x => x != null).ToArray();

			gridSize = new Vector2();

			//set gridsize to default of 1,1 if no sprites
			if(spritesoftilemap.Length == 0)
			{
				gridSize.x = 1;
				gridSize.y = 1;
			}

			//set the gridsize to the smallest sprite size
			foreach(Sprite sprite in spritesoftilemap)
			{
				if(gridSize.x == 0 || gridSize.y == 0 || sprite.bounds.size.x*sprite.bounds.size.y<gridSize.x*gridSize.y) 
				{
					gridSize.x = sprite.bounds.size.x;
					gridSize.y = sprite.bounds.size.y;
				}
			}
			
		}

		GUILayout.BeginHorizontal();
		gridtoggle = EditorGUILayout.Toggle(gridtoggle, GUILayout.Width(16));
		gridSize = EditorGUILayout.Vector2Field("Grid Size (0.05 minimum)", gridSize,  GUILayout.Width(236));
		GUILayout.EndHorizontal();

		EditorGUILayout.LabelField("Parent Object", GUILayout.Width(256));
		parentGO = (GameObject)EditorGUILayout.ObjectField(parentGO, typeof(GameObject),true,GUILayout.Width(256));

		GUILayout.BeginHorizontal();
		addBoxCollider = EditorGUILayout.Toggle(addBoxCollider, GUILayout.Width(16));
		EditorGUILayout.LabelField("Add Box Collider", GUILayout.Width(256));
		GUILayout.EndHorizontal();

		EditorGUILayout.LabelField("Layer Order", GUILayout.Width(256));

		GUILayout.BeginHorizontal();
		layerOrd = EditorGUILayout.IntField(layerOrd,  GUILayout.Width(126));
		isObjmode = EditorGUILayout.Toggle(isObjmode, GUILayout.Width(16));
		EditorGUILayout.LabelField("Layer based on Y", GUILayout.Width(110));
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		tagName = EditorGUILayout.TagField("Tag For Sprite",tagName, GUILayout.Width(236));
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		isDraw = EditorGUILayout.Toggle(isDraw, GUILayout.Width(16));
		selected = (DRAWOPTION)EditorGUILayout.EnumPopup(selected, GUILayout.Width(236));
		GUILayout.EndHorizontal();

		_scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
		GUILayout.BeginHorizontal();
		
		if(tilemapselectindex >= 0)
		{
			//coded settings
			float buttonwidthpad = 2f;
			float buttonheightpad = 2f;
			float selectedbuttonwidthpad = 5f;
			float selectedbuttonheightpad = 5f;

			float lastRect_x = 0f;
			foreach(Sprite sprite in spritesoftilemap)
			{
				//if the sprite is from the row below, end horizontal group and start a new one
				if (sprite.textureRect.x < lastRect_x)
				{
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
				}
				lastRect_x = sprite.textureRect.x;

				//Draws the selected sprite differently
				if (selectedSprite == sprite)
				{
					//Draw the selected button width a bit bigger and a bit higher using padding
					if(GUILayout.Button("", textureStyleAct, GUILayout.Width(sprite.textureRect.width + selectedbuttonwidthpad), GUILayout.Height(sprite.textureRect.height + selectedbuttonheightpad)))
					{
						selectedSprite = null;
					}
					
					//Draw the spirte ontop of the button (Note:Button cant support drawing textures with texcoords)
					Rect lastdrawnedrect = GUILayoutUtility.GetLastRect();
					GUI.DrawTextureWithTexCoords(
								new Rect(lastdrawnedrect.x + selectedbuttonwidthpad / 2,
										lastdrawnedrect.y + selectedbuttonheightpad / 2,
										lastdrawnedrect.width - selectedbuttonwidthpad,
										lastdrawnedrect.height - selectedbuttonheightpad),
								sprite.texture,
								new Rect(sprite.textureRect.x / sprite.texture.width,
										sprite.textureRect.y / sprite.texture.height,
										sprite.textureRect.width / sprite.texture.width,
										sprite.textureRect.height / sprite.texture.height));
				}
				//Draws the rest of the sprites
				else
				{
					//Draw using normal button padding
					if (GUILayout.Button(sprite.texture, textureStyle, GUILayout.Width(sprite.textureRect.width + buttonwidthpad), GUILayout.Height(sprite.textureRect.height + buttonheightpad)))
					{
						selectedSprite = sprite;
					}
						
					//Draw the spirte ontop of the button (Note:Button cant support drawing textures with texcoords)
					GUI.DrawTextureWithTexCoords(GUILayoutUtility.GetLastRect(),
												sprite.texture,
												new Rect(sprite.textureRect.x / sprite.texture.width,
															sprite.textureRect.y / sprite.texture.height,
															sprite.textureRect.width / sprite.texture.width,
															sprite.textureRect.height / sprite.texture.height));
				}
			}
			GUILayout.EndHorizontal();
		}

		EditorGUILayout.EndScrollView();

		//SceneView.RepaintAll();
	}
}

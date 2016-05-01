#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_5

//#define UNITY_5_2_1p2_FIX

#if UNITY_5_2_1p2_FIX || (!UNITY_4_6 && !UNITY_4_7 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2_0 && !UNITY_5_2_1)

// Unity 5.2.2 +
#define UGUI_VERSION_3

#elif UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1

// Unity 4.6 - 5.1
#define UGUI_VERSION_1

#else

// Unity 5.2 - 5.2.2
#define UGUI_VERSION_2

#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

namespace TextFx
{

	[AddComponentMenu("UI/TextFx Text", 12)]
	public class TextFxUGUI : Text , TextFxAnimationInterface
	{
#if UGUI_VERSION_3
		public static bool USING_UGUI_API_PATCH_FIX = true;
#else
		public static bool USING_UGUI_API_PATCH_FIX = false;
#endif


        public enum UGUI_MESH_EFFECT_TYPE
        {
            None,
            Shadow,
            Outline,
            Outline8
        }


#if UGUI_VERSION_1 || UGUI_VERSION_3
		// Interface class for accessing the required text vertex data in the required format
		public class UGUITextDataHandler : TextFxAnimationManager.GuiTextDataHandler
		{
			List<UIVertex> m_vertData;
            int m_numBaseLetters;
            int m_extraVertsPerLetter;
            int m_totalVertsPerLetter;

            public int NumVerts { get { return m_vertData.Count; } }
			public int ExtraVertsPerLetter { get { return m_extraVertsPerLetter; } }
			public int NumVertsPerLetter { get { return m_totalVertsPerLetter; } }
			
			public UGUITextDataHandler(List<UIVertex> vertData, int numBaseLetterMeshes)
			{
				m_vertData = vertData;
                m_numBaseLetters = numBaseLetterMeshes;

                int numVerts = NumVerts;

                if (numVerts > m_numBaseLetters * 4)
                {
                    // There are some extra mesh verts (used by effects)
                    m_extraVertsPerLetter = (numVerts - (m_numBaseLetters * 4)) / m_numBaseLetters;
                }
                else
                    m_extraVertsPerLetter = 0;

                m_totalVertsPerLetter = m_numBaseLetters > 0 ? numVerts / m_numBaseLetters : 0;
            }
			
			public Vector3[] GetLetterBaseVerts(int letterIndex)
			{
                Vector3[] verts = new Vector3[4];
                int vertOffset = m_numBaseLetters * m_extraVertsPerLetter;
				
				for (int idx = 0; idx < 4; idx++)
				{
					if (vertOffset + (letterIndex * 4) + idx < m_vertData.Count)
						// Fill in the base mesh verts 
						verts[idx] = m_vertData[vertOffset + (letterIndex * 4) + idx].position;
					else
						verts[idx] = Vector3.zero;
                }

                return verts;
            }
			
			public Color[] GetLetterBaseCols(int letterIndex)
			{
                Color[] cols = new Color[4];

                int vertOffset = m_numBaseLetters * m_extraVertsPerLetter;

                for (int idx = 0; idx < 4; idx++)
                {
					if (vertOffset + (letterIndex * 4) + idx < m_vertData.Count)
						// Fill in the base mesh cols 
						cols[idx] = m_vertData[vertOffset + (letterIndex * 4) + idx].color;
					else
						cols[idx] = Color.clear;
                }

                return cols;
            }

			public Vector3[] GetLetterExtraVerts(int letterIndex)
			{
                Vector3[] extraVerts = null;

                if (m_extraVertsPerLetter > 0)
                {
                    extraVerts = new Vector3[m_extraVertsPerLetter];

                    int offset = letterIndex * m_extraVertsPerLetter;

                    for(int idx=0; idx < m_extraVertsPerLetter; idx++)
                    {
                        extraVerts[idx] = m_vertData[offset + idx].position;
                    }
                }

                return extraVerts;
            }
			
			public Color[] GetLetterExtraCols(int letterIndex)
			{
                Color[] extraCols = null;

                if (m_extraVertsPerLetter > 0)
                {
                    extraCols = new Color[m_extraVertsPerLetter];

                    int offset = letterIndex * m_extraVertsPerLetter;

                    for (int idx = 0; idx < m_extraVertsPerLetter; idx++)
                    {
                        extraCols[idx] = m_vertData[offset + idx].color;
                    }
                }

                return extraCols;
            }
		}
#elif UGUI_VERSION_2
		// Interface class for accessing the required text vertex data in the required format
		public class UGUITextDataHandler : TextFxAnimationManager.GuiTextDataHandler
		{
			Mesh m_mesh;
			
			public UGUITextDataHandler(Mesh mesh)
			{
				m_mesh = mesh;
			}
			
			public int NumVerts { get { return m_mesh.vertexCount; } }
			public int ExtraVertsPerLetter { get { return 0; } }
			public int NumVertsPerLetter { get { return 4; } }
			
			public Vector3[] GetLetterBaseVerts(int letterIndex)
			{
				return new Vector3[]{m_mesh.vertices[letterIndex * 4], m_mesh.vertices[letterIndex * 4 + 1], m_mesh.vertices[letterIndex * 4 + 2], m_mesh.vertices[letterIndex * 4 + 3]};
			}
			
			public Color[] GetLetterBaseCols(int letterIndex)
			{
				return new Color[]{m_mesh.colors[letterIndex * 4], m_mesh.colors[letterIndex * 4 + 1], m_mesh.colors[letterIndex * 4 + 2], m_mesh.colors[letterIndex * 4 + 3]};
			}

			public Vector3[] GetLetterExtraVerts(int letterIndex)
			{
				return null;
			}
			
			public Color[] GetLetterExtraCols(int letterIndex)
			{
				return null;
			}
		}
#endif




		// Editor TextFx conversion menu options
#if UNITY_EDITOR
		[MenuItem ("Tools/TextFx/Convert UGUI Text to TextFx", false, 200)]
		static void ConvertToTextFX ()
		{
			GameObject activeGO = Selection.activeGameObject;
			Text uguiText = activeGO.GetComponent<Text> ();
			TextFxUGUI textfxUGUI = activeGO.GetComponent<TextFxUGUI> ();

			if(textfxUGUI != null)
				return;

			GameObject tempObject = new GameObject("temp");
			textfxUGUI = tempObject.AddComponent<TextFxUGUI>();

			TextFxUGUI.CopyComponent(uguiText, textfxUGUI);

			DestroyImmediate (uguiText);

			TextFxUGUI newUGUIEffect = activeGO.AddComponent<TextFxUGUI> ();

			TextFxUGUI.CopyComponent (textfxUGUI, newUGUIEffect);

			DestroyImmediate (tempObject);

			Debug.Log (activeGO.name + "'s Text component converted into a TextFxUGUI component");
		}
		
		[MenuItem ("Tools/TextFx/Convert UGUI Text to TextFx", true)]
		static bool ValidateConvertToTextFX ()
		{
			if(Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<Text>() != null)
				return true;
			else
				return false;
		}


		static void CopyComponent(Text textFrom, Text textTo)
		{
			textTo.text = textFrom.text;
			textTo.font = textFrom.font;
			textTo.fontSize = textFrom.fontSize;
			textTo.fontStyle = textFrom.fontStyle;
			textTo.lineSpacing = textFrom.lineSpacing;
			textTo.supportRichText = textFrom.supportRichText;
			textTo.alignment = textFrom.alignment;
			textTo.resizeTextForBestFit = textFrom.resizeTextForBestFit;
			textTo.color = textFrom.color;
			textTo.material = textFrom.material;
			textTo.enabled = textFrom.enabled;
		}
#endif

		// Animation Interface Properties

		public string AssetNameSuffix { get { return "_UGUI"; } }
		public float MovementScale { get { return 26f; } }
		public int LayerOverride { get { return 5; } }			// Renders objects on the UI layer
		public TEXTFX_IMPLEMENTATION TextFxImplementation { get { return TEXTFX_IMPLEMENTATION.UGUI; } }

		[HideInInspector, SerializeField]
		TextFxAnimationManager m_animation_manager;
		public TextFxAnimationManager AnimationManager { get { return m_animation_manager; } }

		[HideInInspector, SerializeField]
		GameObject m_gameobject_reference;
		public GameObject GameObject { get { if( m_gameobject_reference == null) m_gameobject_reference = gameObject; return m_gameobject_reference; } }

		public UnityEngine.Object ObjectInstance { get { return this; } }

		public System.Action OnMeshUpdateCall { get; set; }

        public UGUI_MESH_EFFECT_TYPE m_effect_type = UGUI_MESH_EFFECT_TYPE.None;
        public Vector2 m_effect_offset = new Vector2(1, 1);
        public Color m_effect_colour = Color.black;


		// TextFxUGUI specific variables 

#if UGUI_VERSION_1 || UGUI_VERSION_3
		[HideInInspector] //, SerializeField]		// UIVertex list doesn't seem to serialise even if marked to 'SerializeField'
		List<UIVertex> m_cachedVerts;
        List<UIVertex> m_currentMeshVerts = new List<UIVertex>();
#elif UGUI_VERSION_2
		[HideInInspector, SerializeField]
		VertexHelper m_cachedVertsHelper;
		[HideInInspector, SerializeField]
		List<Vector3> m_currentMeshVerts = new List<Vector3>();
		[HideInInspector, SerializeField]
		List<Color32> m_currentMeshCols = new List<Color32>();
#endif

		bool m_textFxUpdateGeometryCall = false;	// Set to highlight Geometry update calls triggered by TextFx
		bool m_textFxAnimDrawCall = false;		// Denotes whether the subsequent OnFillVBO call has been triggered by TextFx or not


		protected override void OnEnable()
		{
			base.OnEnable ();

			if (m_animation_manager == null)
				m_animation_manager = new TextFxAnimationManager ();
			
			m_animation_manager.SetParentObjectReferences (gameObject, transform, this);
		}

		protected override void Start()
		{
			if(!Application.isPlaying)
			{
				return;
			}

			m_animation_manager.OnStart ();
		}

		void Update()
		{
			if(!Application.isPlaying || !m_animation_manager.Playing)
			{
				return;
			} 

			m_animation_manager.UpdateAnimation ();

			// Call to update mesh rendering
			TextFxUpdateGeometry ();
		}

        public void ForceUpdateGeometry()
        {
            UpdateGeometry();
#if UNITY_EDITOR
			EditorUtility.SetDirty(GameObject);
#endif
		}

		// Interface Method: To redraw the mesh with the current animated mesh state
		public void UpdateTextFxMesh()
		{
			// Call to update mesh rendering
			TextFxUpdateGeometry ();
		}

		// Interface Method: To set the text of the text renderer
		public void SetText(string new_text)
		{
			text = new_text;
		}

        // Interface Method: Returns the number of verts used for the current rendered text
        public int NumMeshVerts
        {
            get
            {
                return m_currentMeshVerts.Count;
            }
        }

        // Interface Method: Access to a specified vert from the current state of the rendered text
        public Vector3 GetMeshVert(int index)
        {
            if (m_currentMeshVerts.Count <= index)
            {
                Debug.LogWarning("Requested vertex index '" + index + "' is out of range");
                return Vector3.zero;
            }

#if UGUI_VERSION_1 || UGUI_VERSION_3
			return m_currentMeshVerts[index].position;
#elif UGUI_VERSION_2
            return m_currentMeshVerts[index];
#endif
		}

		// Interface Method: Access to a specified vert colour from the current state of the rendered text
		public Color GetMeshColour(int index)
        {
            if (m_currentMeshVerts.Count <= index)
            {
                Debug.LogWarning("Requested vert colour index '" + index + "' is out of range");
                return Color.white;
            }

#if UGUI_VERSION_1 || UGUI_VERSION_3
			return m_currentMeshVerts[index].color;
#elif UGUI_VERSION_2
            return m_currentMeshCols[index];
#endif
		}

		// Wrapper to catch all TextFx related calls to UI.Text's UpdateGeometry.
		void TextFxUpdateGeometry()
		{
			m_textFxUpdateGeometryCall = true;

			UpdateGeometry ();
		}

		protected override void UpdateGeometry()
		{
			if(m_textFxUpdateGeometryCall)
			{
				// TextFx Triggering Geometry Update
				m_textFxAnimDrawCall = true;
			}
			else
			{
				// System calling Geometry Update
				m_textFxAnimDrawCall = false;
			}

			m_textFxUpdateGeometryCall = false;

			base.UpdateGeometry ();
		}


#if UGUI_VERSION_3
        void AddEffectMeshes(VertexHelper vHelper)
        {
            if (m_effect_type == UGUI_MESH_EFFECT_TYPE.None)
                return;

            int numMeshes = vHelper.currentVertCount / 4;
            int numExtraMeshesPerLetter = 1;
            switch(m_effect_type)
            {
                case UGUI_MESH_EFFECT_TYPE.Outline:
                    numExtraMeshesPerLetter = 4; break;
                case UGUI_MESH_EFFECT_TYPE.Outline8:
                    numExtraMeshesPerLetter = 8; break;
            }

            List<UIVertex[]> baseLetterQuads = new List<UIVertex[]>();
            List<UIVertex[]> meshEffectQuads = new List<UIVertex[]>();
            UIVertex vert = new UIVertex();

            UIVertex[] baseletterQuad = new UIVertex[4];
            UIVertex[] meshEffectQuad = new UIVertex[4];

			// Scale the offset amount based on the font size
			Vector2 scaledEffectOffset = (m_effect_offset * fontSize) / 65f;

			for (int mIdx = 0; mIdx < numMeshes; mIdx++)
            {
                for (int effectMeshIdx = 0; effectMeshIdx < numExtraMeshesPerLetter; effectMeshIdx++)
                {
                    if (effectMeshIdx == 0)
                        baseletterQuad = new UIVertex[4];

					meshEffectQuad = new UIVertex[4];

                    for (int idx = 0; idx < 4; idx++)
                    {
                        vHelper.PopulateUIVertex(ref vert, (mIdx * 4) + idx);

                        if (effectMeshIdx == 0)
                        {
							// Store the original state of the baseVert to be added at the end of the VertexHelper mesh later
							baseletterQuad[idx] = vert;
                        }

                        int xMod = 0, yMod = 0;
						if (effectMeshIdx == 0) { xMod = 1; yMod = 1; }
						else if (effectMeshIdx == 1) { xMod = -1; yMod = 1; }
						else if (effectMeshIdx == 2) { xMod = 1; yMod = -1; }
						else if (effectMeshIdx == 3) { xMod = -1; yMod = -1; }
						else if (effectMeshIdx == 4) { xMod = -1; yMod = 0; }
						else if (effectMeshIdx == 5) { xMod = 1; yMod = 0; }
						else if (effectMeshIdx == 6) { xMod = 0; yMod = 1; }
						else if (effectMeshIdx == 7) { xMod = 0; yMod = -1; }


						meshEffectQuad[idx] = vert;
                        meshEffectQuad[idx].position += new Vector3(scaledEffectOffset.x * xMod, scaledEffectOffset.y * yMod, 0);
                        meshEffectQuad[idx].color = m_effect_colour;
                    }

                    if (effectMeshIdx == 0)
                        baseLetterQuads.Add(baseletterQuad);

					meshEffectQuads.Add(meshEffectQuad);
                }
            }


            vHelper.Clear();

            // Add in any additional mesh effect quads
            for (int mIdx = 0; mIdx < numMeshes; mIdx++)
            {
                for (int effectMeshIdx = 0; effectMeshIdx < numExtraMeshesPerLetter; effectMeshIdx++)
                {
                    meshEffectQuads[mIdx * (numExtraMeshesPerLetter) + effectMeshIdx][0].color = m_effect_colour;
                    meshEffectQuads[mIdx * (numExtraMeshesPerLetter) + effectMeshIdx][1].color = m_effect_colour;
                    meshEffectQuads[mIdx * (numExtraMeshesPerLetter) + effectMeshIdx][2].color = m_effect_colour;
                    meshEffectQuads[mIdx * (numExtraMeshesPerLetter) + effectMeshIdx][3].color = m_effect_colour;
					
					vHelper.AddUIVertexQuad(meshEffectQuads[mIdx * (numExtraMeshesPerLetter) + effectMeshIdx]);
                }
            }

			// Add in the original base mesh verts to the very end of the mesh verts list
            for (int mIdx = 0; mIdx < numMeshes; mIdx++)
			{
				vHelper.AddUIVertexQuad(baseLetterQuads[mIdx]);
			}
		}
#endif


#if UGUI_VERSION_1 || UGUI_VERSION_3

        /// <summary>
        /// Draw the Text.
        /// </summary>

#if UGUI_VERSION_3
        protected override void OnPopulateMesh(VertexHelper vHelper)
#else
		protected override void OnFillVBO(List<UIVertex> vbo)
#endif
		{
			if (font == null)
			{
				m_textFxAnimDrawCall = false;
				return;
			}


			if (!m_textFxAnimDrawCall || m_cachedVerts == null)
			{
#if UGUI_VERSION_3
				if (m_cachedVerts == null)
					m_cachedVerts = new List<UIVertex>();

                base.OnPopulateMesh(vHelper);
#else
		
				base.OnFillVBO (vbo);
#endif




#if UGUI_VERSION_3

				int numLetterMeshes = vHelper.currentVertCount / 4;

                // Add in any effect mesh additions
                AddEffectMeshes(vHelper);


                // Update UIVertex cache
                m_cachedVerts.Clear();

                UIVertex new_vert = new UIVertex();
                for (int idx = 0; idx < vHelper.currentVertCount; idx++)
                {
                    vHelper.PopulateUIVertex(ref new_vert, idx);

                    m_cachedVerts.Add(new_vert);
                }

                // Update current mesh state values
                m_currentMeshVerts = m_cachedVerts.GetRange(0, m_cachedVerts.Count);
#else

				int numLetterMeshes = vbo.Count / 4;

                // Update caches
				m_cachedVerts = vbo.GetRange(0, vbo.Count);

				// Update current mesh state values
                m_currentMeshVerts = vbo.GetRange(0, vbo.Count);

#endif


                // Call to update animation letter setups
				m_animation_manager.UpdateText(text, new UGUITextDataHandler(m_cachedVerts, numLetterMeshes), white_space_meshes: true);


				if (Application.isPlaying
				   || m_animation_manager.Playing					// Animation is playing, so will now need to render this new mesh in the current animated state
#if UNITY_EDITOR
				   || TextFxAnimationManager.ExitingPlayMode		// To stop text being auto rendered in to default position when exiting play mode in editor.
#endif
				   )
				{
					// The verts need to be set to their current animated states
					// So recall OnFillVBO, getting it to populate with whatever available animate mesh data for this frame
					m_textFxAnimDrawCall = true;

                    // Update animated mesh values to latest
                    m_animation_manager.UpdateMesh(true, true, delta_time: 0);

#if UGUI_VERSION_3
					OnPopulateMesh(vHelper);
#else
		
					vbo.Clear();
					
					OnFillVBO(vbo );
#endif

					return;
				}
				else
				{
					m_animation_manager.PopulateDefaultMeshData(true);
				}
			}
			else
			{
				// TextFx render call. Use cached text mesh data
				UIVertex new_vert = new UIVertex();

#if UGUI_VERSION_3
				vHelper.Clear();
				if (m_currentMeshVerts == null)
					m_currentMeshVerts = new List<UIVertex>();
				else
				    m_currentMeshVerts.Clear();

				// Add each cached vert into the VBO buffer. Verts seem to need to be added one by one using Add(), can't just copy the list over
				for (int idx = 0; idx < m_cachedVerts.Count; idx += 4)
				{
					vHelper.AddUIVertexQuad(new UIVertex[] { m_cachedVerts[idx], m_cachedVerts[idx + 1], m_cachedVerts[idx + 2], m_cachedVerts[idx + 3] });

					m_currentMeshVerts.Add(m_cachedVerts[idx]);
					m_currentMeshVerts.Add(m_cachedVerts[idx+1]);
					m_currentMeshVerts.Add(m_cachedVerts[idx+2]);
					m_currentMeshVerts.Add(m_cachedVerts[idx+3]);

					if (m_animation_manager.MeshVerts != null && idx < m_animation_manager.MeshVerts.Length)
					{
						for (int qidx = 0; qidx < 4; qidx++)
						{
							vHelper.PopulateUIVertex(ref new_vert, idx + qidx);
							new_vert.position = m_animation_manager.MeshVerts[idx + qidx];
							new_vert.color = m_animation_manager.MeshColours[idx + qidx];
							vHelper.SetUIVertex(new_vert, idx + qidx);

							m_currentMeshVerts[idx + qidx] = new_vert;
                        }
					}
				}
#else
				// Add each cached vert into the VBO buffer. Verts seem to need to be added one by one using Add(), can't just copy the list over
				for (int idx=0; idx < m_cachedVerts.Count; idx++)
				{
					vbo.Add( m_cachedVerts[idx]);

					if(m_animation_manager.MeshVerts != null && idx < m_animation_manager.MeshVerts.Length)
					{
						new_vert = vbo[vbo.Count - 1];
						new_vert.position = m_animation_manager.MeshVerts[idx];
						new_vert.color = m_animation_manager.MeshColours[idx];
						vbo[vbo.Count - 1] = new_vert;
					}
				}

                // Update current mesh state values
                m_currentMeshVerts = vbo.GetRange(0, vbo.Count);
#endif

#if UNITY_EDITOR
				// Set object dirty to trigger sceneview redraw/update. Calling SceneView.RepaintAll() doesn't work for some reason.
				EditorUtility.SetDirty( GameObject );
#endif
			}

			m_textFxAnimDrawCall = false;

			if (OnMeshUpdateCall != null)
				OnMeshUpdateCall();
		}

#elif UGUI_VERSION_2

		protected override void OnPopulateMesh(Mesh toFill)
		{
			if (font == null)
			{
				m_textFxAnimDrawCall = false;
				return;
			}

			if (!m_textFxAnimDrawCall || m_cachedVertsHelper == null)
			{
				// Native unity render call

				base.OnPopulateMesh (toFill);

				// Update caches
				m_cachedVertsHelper = new VertexHelper(toFill);

                // Update current mesh state values
                m_currentMeshVerts.Clear();
                m_currentMeshVerts.AddRange(toFill.vertices);
                m_currentMeshCols.Clear();
                m_currentMeshCols.AddRange(toFill.colors32);

                // Call to update animation letter setups
                m_animation_manager.UpdateText (text, new UGUITextDataHandler(toFill), white_space_meshes: true);


				if (Application.isPlaying 
					|| m_animation_manager.Playing					// Animation is playing, so will now need to render this new mesh in the current animated state
#if UNITY_EDITOR
				   || TextFxAnimationManager.ExitingPlayMode		// To stop text being auto rendered in to default position when exiting play mode in editor.
#endif
				   )
				{
					// The verts need to be set to their current animated states
					// So recall OnPopulateMesh, getting it to populate with whatever available animate mesh data for this frame
					m_textFxAnimDrawCall = true;

					// Update animated mesh values to latest
                    m_animation_manager.UpdateMesh(true, true, delta_time: 0);

					OnPopulateMesh( toFill );
					return;
				}
				else
				{
					m_animation_manager.PopulateDefaultMeshData(true);
				}
			}
			else
			{
				// TextFx render call. Use cached text mesh data

				// Fill mesh with the cached text state
				m_cachedVertsHelper.FillMesh(toFill);

				if(m_animation_manager.MeshVerts != null)
				{
					m_currentMeshVerts.Clear();
					m_currentMeshVerts.AddRange(toFill.vertices);
					m_currentMeshCols.Clear();
					m_currentMeshCols.AddRange(toFill.colors32);

					// Loop through each vert setting its new animated state
					for(int idx=0; idx < toFill.vertexCount; idx++)
					{
						if(idx < m_animation_manager.MeshVerts.Length)
						{
							m_currentMeshVerts[idx] = m_animation_manager.MeshVerts[idx];
							m_currentMeshCols[idx] = (Color32) m_animation_manager.MeshColours[idx];
						}
					}

					toFill.SetVertices(m_currentMeshVerts);
					toFill.SetColors(m_currentMeshCols);

					toFill.RecalculateBounds();
				}

				
#if UNITY_EDITOR
				// Set object dirty to trigger sceneview redraw/update. Calling SceneView.RepaintAll() doesn't work for some reason.
				EditorUtility.SetDirty( GameObject );
#endif
			}
			
			m_textFxAnimDrawCall = false;

			if (OnMeshUpdateCall != null)
				OnMeshUpdateCall();
		}

#endif

	}
}
#endif

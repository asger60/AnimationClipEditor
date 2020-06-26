using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class AnimationClipEditorWindow : EditorWindow
    {
        private string _animFile;
        private AnimationClip _clip;
        private float _curveAngle;


        class BindingEdit
        {
            public EditorCurveBinding binding;
            public bool[] editKeys;
        }

        private List<BindingEdit> _bindingEdits;

        [MenuItem("Window/AnimationClipEditorWindow")]
        static void Init()
        {
            AnimationClipEditorWindow window = (AnimationClipEditorWindow) GetWindow(typeof(AnimationClipEditorWindow));
            window.Show();
        }

        void OnGUI()
        {
            //var c = EditorGUILayout.ObjectField(_clip, typeof(AnimationClip), false);

            if (_clip == null)
            {
                //AssetDatabase.GetAssetPath(c);
                EditorGUILayout.LabelField("Path to animation file (remember .anim)");
                _animFile = EditorGUILayout.TextField( _animFile);
                if (GUILayout.Button("Load File"))
                {
                    _clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(@"Assets/" + _animFile);
                    Refresh();
                }

                return;
            }

            if (GUILayout.Button("Refresh"))
            {
                Refresh();
            }
            EditorGUILayout.Space();
            _curveAngle = EditorGUILayout.Slider(_curveAngle, 0, 1);

            if (_bindingEdits == null) return;
            foreach (var bindingEdit in _bindingEdits)
            {
                EditorGUILayout.LabelField(bindingEdit.binding.propertyName);
                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Edit Key");
                for (int i = 0; i < bindingEdit.editKeys.Length; i++)
                {
                    bindingEdit.editKeys[i] =
                        GUILayout.Toggle(bindingEdit.editKeys[i], i.ToString(), GUILayout.Width(28));
                }

                GUILayout.EndHorizontal();

                EditBinding(bindingEdit);
            }
        }

        void Refresh()
        {
            var _bindings = AnimationUtility.GetCurveBindings(_clip);
            _bindingEdits = new List<BindingEdit>();
            foreach (var binding in _bindings)
            {
                var curve = AnimationUtility.GetEditorCurve(_clip, binding);
                _bindingEdits.Add(new BindingEdit()
                    {binding = binding, editKeys = new bool[curve.keys.Length]});
            }
        }

        private void EditBinding(BindingEdit bindingEdit)
        {
            var binding = bindingEdit.binding;
            var curve = AnimationUtility.GetEditorCurve(_clip, binding);

            Keyframe[] newFrames = new Keyframe[curve.keys.Length];

            for (int i = 0; i < newFrames.Length; i++)
            {
                newFrames[i] = curve.keys[i];
            }

            for (int i = 0; i < bindingEdit.editKeys.Length; i++)
            {
                if (bindingEdit.editKeys[i])
                {
                    Keyframe newKey = new Keyframe(curve.keys[i].time, curve.keys[i].value)
                    {
                        weightedMode = WeightedMode.Both, outWeight = _curveAngle, outTangent = 10,
                        inWeight = _curveAngle, inTangent = 10
                    };
                    newFrames[i] = newKey;
                }
            }


            AnimationUtility.SetEditorCurve(_clip, binding, new AnimationCurve(newFrames));
        }
    }
}
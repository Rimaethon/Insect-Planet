// Designed by KINEMATION, 2023

using System.Collections.Generic;
using Kinemation.FPSFramework.Runtime.Core.Components;
using Kinemation.FPSFramework.Runtime.FPSAnimator;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using AnimatorController = UnityEditor.Animations.AnimatorController;

namespace Kinemation.FPSFramework.Editor.Core
{
    public class FPSAnimValidatorEditorWindow : EditorWindow
    {
        private GameObject character;
        private CoreAnimComponent coreAnimComponent;
        
        private ReorderableList weaponList;
        private bool showWeaponList = true;
        
        private ReorderableList animList;
        private bool showAnimList = true;
        
        private int selectedTabIndex = 0;
        private string[] tabNames = { "Character", "Weapon", "Animations" };
        
        private Vector2 scrollView;

        private static GUIStyle boldLabel;
        private static GUIStyle headerLabel;
        private static GUIStyle logLabel;

        struct LogMessage
        {
            public string error;
            public string log;
            public bool isExpanded;
            public int verbosity;
        }
        
        struct ValidatorLog
        {
            public string label;
            public bool isVisible;
            public Object refObj;

            public List<LogMessage> log;

            public ValidatorLog(string label)
            {
                this.label = label;
                isVisible = true;
                log = new List<LogMessage>();
                refObj = null;
            }

            public void Reset()
            {
                log = log ?? new List<LogMessage>();
                log.Clear();
                isVisible = false;
            }

            public void Print()
            {
                if (!isVisible) return;

                bool bSuccess = log.Count == 0;
                
                EditorGUILayout.Space();

                if (refObj == null)
                {
                    GUILayout.Label(label, boldLabel);
                }
                else
                {
                    EditorGUILayout.ObjectField("", refObj, typeof(Object), true);
                }
                
                if (bSuccess)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField("Success! " + label + " is good to go.", logLabel);
                    EditorGUILayout.EndVertical();
                    return;
                }
                
                for(int i = 0; i < log.Count; i++)
                {
                    var item = log[i];
                    
                    string arrow = !item.isExpanded ? "▼" : "▲";
                    string buttonLabel = arrow + " " + item.error;

                    string iconName;

                    switch (item.verbosity)
                    {
                        case 0:
                            iconName = "console.infoicon";
                            break;
                        case 1:
                            iconName = "console.warnicon";
                            break;
                        default:
                            iconName = "console.erroricon";
                            break;
                    }

                    GUIContent content =
                        EditorGUIUtility.IconContent(iconName);
                    content.text = buttonLabel;
                    
                    var customButtonStyle = new GUIStyle(EditorStyles.toolbarButton)
                    {
                        alignment = TextAnchor.MiddleLeft,
                    };

                    if (GUILayout.Button(content, customButtonStyle))
                    {
                        item.isExpanded = !item.isExpanded;
                    }
                    
                    if (item.isExpanded)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.LabelField(item.log, logLabel);
                        EditorGUILayout.EndVertical();
                    }

                    log[i] = item;
                    EditorGUILayout.Space();
                }
            }
        }

        private ValidatorLog ikRigLog;
        private ValidatorLog animatorLog;
        private ValidatorLog animGraphLog;
        private List<ValidatorLog> weaponLog = new List<ValidatorLog>();
        private List<ValidatorLog> animLog = new List<ValidatorLog>();

        [MenuItem("Window/FPS Animation Framework/FPS Animation Validator")]
        public static void ShowWindow()
        {
            GetWindow<FPSAnimValidatorEditorWindow>("FPS Animation Validator");
        }
        
        private void OnEnable()
        {
            // Weapon list initialization
            
            weaponList = new ReorderableList(new List<GameObject>(), typeof(GameObject), true, 
                true, false, false);
            
            weaponList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Weapons List");
            };

            weaponList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var weapon = weaponList.list[index] as GameObject;
                weaponList.list[index] = EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, 
                    EditorGUIUtility.singleLineHeight), weapon, typeof(GameObject), true);
            };

            weaponList.onAddCallback = (ReorderableList list) =>
            {
                list.list.Add(null);
            };

            weaponList.onRemoveCallback = (ReorderableList list) =>
            {
                list.list.RemoveAt(list.index);
            };
            
            // Anim list initialization
            
            animList = new ReorderableList(new List<AnimationClip>(), typeof(AnimationClip), true, 
                true, false, false);
            
            animList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Animation List");
            };

            animList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var anim = animList.list[index] as AnimationClip;
                animList.list[index] = EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, 
                    EditorGUIUtility.singleLineHeight), anim, typeof(AnimationClip), true);
            };

            animList.onAddCallback = (ReorderableList list) =>
            {
                list.list.Add(null);
            };

            animList.onRemoveCallback = (ReorderableList list) =>
            {
                list.list.RemoveAt(list.index);
            };
            
            boldLabel = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                wordWrap = true
            };
            
            headerLabel = new GUIStyle(EditorStyles.label)
            {
                fontSize = 14,
                wordWrap = true
            };
            
            logLabel = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                wordWrap = true
            };
        }

        private void OnValidate()
        {
            if (character == null)
            {
                ikRigLog.Reset();
            }
        }

        private void ValidateIKRig()
        {
            var data = coreAnimComponent.ikRigData;
            
            LogMessage message = new LogMessage();
            message.error = message.log = string.Empty;
            message.error = "Null reference!";
            
            if (data.rootBone == null) message.log += "Root is null! \n";
            if (data.pelvis == null) message.log += "Pelvis is null! \n";
            if (data.spineRoot == null) message.log += "Spine Root is null! \n";
            if (data.masterDynamic.obj == null) message.log += "Master Dynamic is null! \n";
            
            void LogLimb(DynamicBone bone, string boneName)
            {
                if (bone.obj == null) message.log += boneName + " obj is null! \n";
                if (bone.target == null) message.log += boneName + " target is null! \n";
                if (bone.hintTarget == null) message.log += boneName + " hint target is null! \n";
            }

            LogLimb(data.rightHand, "Right Hand");
            LogLimb(data.leftHand, "Left Hand");
            LogLimb(data.rightFoot, "Right Foot");
            LogLimb(data.leftFoot, "Left Foot");

            if (data.weaponBone == null) message.log += "Weapon Bone is null!\n";
            if (data.weaponBoneLeft == null) message.log += "Weapon Bone Left is null!\n";
            if (data.weaponBoneRight == null) message.log += "Weapon Bone Right is null!\n";
            
            if (!string.IsNullOrEmpty(message.log))
            {
                string helpMessage = "Make sure to manually check the unassigned references.\n"
                                     + "If you use a Generic model, the system might not recognize the names automatically.\n"
                                     + "Go to FPSAnimator/IK Rig Data and assign the missing bones.";

                message.log += "\n" + helpMessage;
                message.verbosity = 2;
                ikRigLog.log.Add(message);
            }
            
            ikRigLog.label = "IK Rig";
            ikRigLog.isVisible = true;
        }

        private void ValidateAnimator()
        {
            animatorLog.label = "Animator";
            animatorLog.isVisible = true;
            
            var animator = character.GetComponentInChildren<Animator>();

            if (animator == null)
            {
                animatorLog.log.Add(new LogMessage()
                {
                    error = "Animator is null!",
                    log = "Please, add Animator component to your character!",
                    verbosity = 2
                });
                
                return;
            }
            
            bool isHuman = animator.isHuman;
            
            if (animator.runtimeAnimatorController is AnimatorController controller)
            {
                AnimationClip[] clips = controller.animationClips;
                foreach (AnimationClip clip in clips)
                {
                    if (clip.isHumanMotion != isHuman)
                    {
                        string logMessage = string.Empty;
                        logMessage += "Rig type mismatch: " +
                                      "make sure than you don't use Humanoid animations with a Generic character!";

                        string clipType = clip.isHumanMotion ? "Humanoid" : "Generic";
                        string rigType = isHuman ? "Humanoid" : "Generic"; 
                        
                        logMessage += "\n" + clip.name + " is " + clipType + ", while character is " + rigType;
                        
                        animatorLog.log.Add(new LogMessage()
                        {
                            error = "Rig type mismatch!",
                            log = logMessage,
                            verbosity = 2
                        });
                        
                        break;
                    }
                }
            }
        }

        private void ValidateAnimGraph()
        {
            var graph = coreAnimComponent.animGraph;
            
            animGraphLog.label = "Anim Graph";
            animGraphLog.isVisible = true;
            
            if (graph == null)
            {
                animGraphLog.log.Add(new LogMessage()
                {
                    error = "Anim Graph is null!",
                    log = "Try removing and adding the FPSAnimator again."
                });
                return;
            }

            var mask = graph.GetUpperBodyMask();
            if (mask == null)
            {
                animGraphLog.log.Add(new LogMessage()
                {
                    error = "Upper Body Mask is null!",
                    log = "Make sure to specify it in the FPSAnimator/Anim Graph Tab."
                });
                return;
            }

            bool foundWeaponBone = false;
            for (int i = 0; i < mask.transformCount; i++)
            {
                if (mask.GetTransformPath(i).ToLower().Contains("weaponbone"))
                {
                    if (!mask.GetTransformActive(i))
                    {
                        animGraphLog.log.Add(new LogMessage()
                        {
                            error = "Weapon Bone is not active!",
                            log = "Go to your avatar mask and enable the rootBone/WeaponBone."
                        });
                    }
                    
                    foundWeaponBone = true;
                    break;
                }
            }

            if (!foundWeaponBone)
            {
                animGraphLog.log.Add(new LogMessage()
                {
                    error = "Missing Weapon Bone!",
                    log = "Make sure to click the `Setup Avatar Mask` button in the FPSAnimator/Anim Graph Tab."
                });
            }
        }
        
        private void ValidateCharacter()
        {
            ikRigLog.Reset();
            animatorLog.Reset();
            animGraphLog.Reset();

            foreach (var log in weaponLog)
            {
                log.Reset();
            }
            
            foreach (var log in animLog)
            {
                log.Reset();
            }

            if (character == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select the Character.", "OK");
                return;
            }

            coreAnimComponent = character.GetComponentInChildren<CoreAnimComponent>();
            if (coreAnimComponent == null)
            {
                EditorUtility.DisplayDialog("Error",
                    "Character does not have the FPS Animator component.", "OK");
                return;
            }
            
            ValidateIKRig();
            ValidateAnimator();
            ValidateAnimGraph();
            ValidateWeapon();
            ValidateAnimations();
        }

        private void ValidateWeapon()
        {
            if (weaponList.list.Count == 0)
            {
                return;
            }
            
            weaponLog.Clear();

            bool isHuman = character.GetComponentInChildren<Animator>().isHuman;
            string rigType = isHuman ? "Humanoid." : "Generic.";
            
            foreach (var weapon in weaponList.list)
            {
                var gunObject = weapon as GameObject;
                var gun = gunObject.GetComponent<FPSAnimWeapon>();
                
                EditorGUILayout.Space();

                ValidatorLog gunLog = new ValidatorLog(gunObject.name);
                gunLog.refObj = gunObject;

                if (gun.fireRate <= 0f)
                {
                    gunLog.log.Add(new LogMessage()
                    {
                        error = "Has invalid fire rate!",
                        log = "Make sure it's more than 0!",
                        verbosity = 2
                    });
                }
                
                if (gun.overlayPose == null)
                {
                    gunLog.log.Add(new LogMessage()
                    {
                        error = "Missing OverlayPose!",
                        log = "Specify the AnimSequence with static pose for your weapon!",
                        verbosity = 2
                    });
                }
                else
                {
                    string pose = gun.overlayPose.clip.isHumanMotion ? "Humanoid" : "Generic";

                    if (gun.overlayPose.clip.isHumanMotion != isHuman)
                    {
                        gunLog.log.Add(new LogMessage()
                        {
                            error = "OverlayPose type mismatch!",
                            log = "Uses " + pose + " pose, while Animator is " + rigType,
                            verbosity = 2
                        });
                    }
                }

                if (gun.recoilData == null)
                {
                    gunLog.log.Add(new LogMessage()
                    {
                        error = "Missing recoil data!",
                        log = "Assign the missing reference in your prefab.",
                        verbosity = 2
                    });
                }

                if (gun.weaponAsset == null)
                {
                    gunLog.log.Add(new LogMessage()
                    {
                        error = "Missing weapon anim asset!",
                        log = "Assign the missing reference in your prefab.",
                        verbosity = 1
                    });
                }
                else
                {
                    if (gun.weaponTransformData.pivotPoint == null)
                    {
                        gunLog.log.Add(new LogMessage()
                        {
                            error = "Missing pivot point!",
                            log = "Assign the missing reference in Your Prefab/WeaponTransformData/PivotPoint.",
                            verbosity = 2
                        });
                    }
                    
                    if (gun.weaponTransformData.aimPoint == null)
                    {
                        gunLog.log.Add(new LogMessage()
                        {
                            error = "Missing aim point!",
                            log = "Assign the missing reference in Your Prefab/WeaponTransformData/AimPoint.",
                            verbosity = 2
                        });
                    }
                }

                if (gun.aimOffsetTable == null)
                {
                    gunLog.log.Add(new LogMessage()
                    {
                        error = "Missing aim offset table!",
                        log = "Assign the missing reference in your prefab.",
                        verbosity = 2
                    });
                }
                
                weaponLog.Add(gunLog);
            }
        }

        private void ValidateAnimations()
        {
            if (animList.list.Count == 0)
            {
                return;
            }
            
            animLog.Clear();
            bool isHuman = character.GetComponentInChildren<Animator>().isHuman;
            string rigType = isHuman ? "Humanoid" : "Generic";

            foreach (var anim in animList.list)
            {
                var obj = anim as AnimationClip;

                EditorGUILayout.Space();
                
                ValidatorLog log = new ValidatorLog(obj.name);
                log.refObj = obj;

                if (obj.isHumanMotion != isHuman)
                {
                    string animType = obj.isHumanMotion ? " Humanoid." : " Generic.";
                    log.log.Add(new LogMessage()
                    {
                        error = "Rig type mismatch!",
                        log = "Character is " + rigType + ", while animation is" + animType,
                        verbosity = 2
                    });
                }

                var bindings = AnimationUtility.GetCurveBindings(obj);

                bool hasWeaponBone = false;
                foreach (var binding in bindings)
                {
                    if (binding.path.ToLower().Contains("weaponbone"))
                    {
                        hasWeaponBone = true;
                    }
                }

                if (!hasWeaponBone)
                {
                    log.log.Add(new LogMessage()
                    {
                        error = "WeaponBone is not animated!",
                        log = "Ignore if animation doesn't animate the weapon, use Transform Retarget tool otherwise.",
                        verbosity = 0
                    });
                }
                
                animLog.Add(log);
            }
        }
        
        private void DrawCharacterTab()
        {
            ikRigLog.Print();
            animatorLog.Print();
            animGraphLog.Print();
        }

        private void DrawWeaponTab()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            
            var skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
            var miniButtonStyle = skin.FindStyle("MiniButton");
            
            GUIContent content = new GUIContent(showWeaponList ? "Hide List" : "Show List", 
                "Toggles list visibility");
            
            if (GUILayout.Button(content, miniButtonStyle))
            {
                showWeaponList = !showWeaponList;
            }
            
            content = new GUIContent("Clear List", "Removes all the items");
            if (GUILayout.Button(content, miniButtonStyle))
            {
                weaponLog.Clear();
                weaponList.list.Clear();
            }
            
            EditorGUILayout.EndHorizontal();
            
            if (showWeaponList)
            {
                Event evt = Event.current;
                Rect drop_area = GUILayoutUtility.GetRect(0.0f, 40.0f, GUILayout.ExpandWidth(true));

                GUIStyle Style = new GUIStyle()
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white }
                };
                
                GUI.Box(drop_area, "Drag & Drop Weapons here!", Style);
                
                switch (evt.type)
                {
                    case EventType.DragUpdated:
                    case EventType.DragPerform:
                        if (!drop_area.Contains(evt.mousePosition))
                            break;

                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                        if (evt.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();

                            foreach (var dragged_object in DragAndDrop.objectReferences)
                            {
                                if (dragged_object is GameObject go)
                                {
                                    var comp = go.GetComponent<FPSAnimWeapon>();
                                    if (comp != null)
                                    {
                                        bool add = true;
                                        foreach (var obj in weaponList.list)
                                        {
                                            if (obj as GameObject == go)
                                            {
                                                add = false;
                                                break;
                                            }
                                        }

                                        if (add)
                                        {
                                            weaponList.list.Add(go);
                                        }
                                    }
                                }
                            }
                        }
                        Event.current.Use();
                        break;
                }
            }

            if (showWeaponList)
            {
                weaponList.DoLayoutList();
                foreach (var log in weaponLog)
                {
                    log.Print();
                }
            }
        }

        private void DrawAnimationsTab()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            
            var skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
            var miniButtonStyle = skin.FindStyle("MiniButton");
            
            GUIContent content = new GUIContent(showAnimList ? "Hide List" : "Show List", 
                "Toggles list visibility");
            
            if (GUILayout.Button(content, miniButtonStyle))
            {
                showAnimList = !showAnimList;
            }
            
            content = new GUIContent("Clear List", "Removes all the items");
            if (GUILayout.Button(content, miniButtonStyle))
            {
                animLog.Clear();
                animList.list.Clear();
            }
            
            EditorGUILayout.EndHorizontal();
            
            if (showAnimList)
            {
                Event evt = Event.current;
                Rect drop_area = GUILayoutUtility.GetRect(0.0f, 40.0f, GUILayout.ExpandWidth(true));

                GUIStyle Style = new GUIStyle()
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white }
                };
                
                GUI.Box(drop_area, "Drag & Drop Anims here!", Style);
                
                switch (evt.type)
                {
                    case EventType.DragUpdated:
                    case EventType.DragPerform:
                        if (!drop_area.Contains(evt.mousePosition))
                            break;

                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                        if (evt.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();

                            foreach (var dragged_object in DragAndDrop.objectReferences)
                            {
                                AnimationClip clip = null;
                                if (dragged_object is GameObject fbx)
                                {
                                    string assetPath = AssetDatabase.GetAssetPath(dragged_object);
                                    Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                                    foreach (Object asset in assets)
                                    {
                                        if (asset is AnimationClip)
                                        {
                                            clip = asset as AnimationClip;
                                            break;
                                        }
                                    }
                                }
                                else if (dragged_object is AnimationClip anim)
                                {
                                    clip = anim;
                                }

                                if (clip == null) return;
                                
                                bool add = true;
                                foreach (var obj in animList.list)
                                {
                                    if (obj as AnimationClip == clip)
                                    {
                                        add = false;
                                        break;
                                    }
                                }
                                
                                if (add)
                                {
                                    animList.list.Add(clip);
                                }
                            }
                        }
                        Event.current.Use();
                        break;
                }
            }

            if (showAnimList)
            {
                animList.DoLayoutList();
                foreach (var log in animLog)
                {
                    log.Print();
                }
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(new GUIStyle()
            {
                padding = new RectOffset(10, 10, 10, 10)
            });
            
            GUILayout.Label("Validator Settings", EditorStyles.boldLabel);
            character = (GameObject) EditorGUILayout.ObjectField("Character", character,
                typeof(GameObject), true);
            
            selectedTabIndex = GUILayout.Toolbar(selectedTabIndex, tabNames);

            scrollView = EditorGUILayout.BeginScrollView(scrollView);
            
            switch (selectedTabIndex)
            {
                case 0:
                    DrawCharacterTab();
                    break;

                case 1:
                    DrawWeaponTab();
                    break;

                case 2:
                    DrawAnimationsTab();
                    break;
            }
            
            if (GUILayout.Button("Validate"))
            {
                ValidateCharacter();
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
    }
}
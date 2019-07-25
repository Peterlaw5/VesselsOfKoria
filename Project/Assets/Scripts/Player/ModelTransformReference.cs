using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelTransformReference : MonoBehaviour
{
    [Header("Transforms reference points")]
    [Tooltip("Bone reference for upper body rotation for vertical aiming")]
    public Transform[] verticalRotationBones;
    public Transform[] verticalRotationsOutsideAnimator;
    [Tooltip("Attack/abilities bullets spawn points")]
    public Transform[] spawnPoints;
    [Header("First person head/legs hiding system")]
    public float hideScaleValue = 0.01f;

    [Tooltip("First person bones to hide for better animations")]
    public Transform[] bonesToHide;
    [Header("First person model offsets")]
    [Tooltip("First person model position offset")]
    public Vector3 clientRelativePosition = Vector3.zero;
    [Tooltip("First person model rotation offset")]
    public Vector3 clientRelativeRotation = Vector3.zero;
    [Header("Seeker animation")]
    public RuntimeAnimatorController clientAnimatorController;
}

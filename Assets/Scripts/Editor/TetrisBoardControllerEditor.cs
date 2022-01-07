using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TetrisBoardController))]
public class TetrisBoardControllerEditor : Editor
{
    #region Game Variables

    public static bool gameVariablesFoldout = true;

    //Board Variables

    public static bool boardVariablesFoldout = true;

    protected SerializedProperty boardWidth;
    protected SerializedProperty boardHeight;

    protected SerializedProperty spawnPos;

    //Movement variables

    public static bool movementVariablesFoldout = true;

    protected SerializedProperty gravityDropLimit;
    protected SerializedProperty softDropLimit;
    protected SerializedProperty horMovLimit;
    protected SerializedProperty lockedLimitTime;

    //Score and other variables

    public static bool scoreVariablesFoldout = true;

    protected SerializedProperty linesToLevelUp;
    protected SerializedProperty possibleScores;

    #endregion

    /*#region Bot Variables

    //Bot variables

    public static bool botVariablesFoldout = true;

    protected SerializedProperty playMode;
    protected SerializedProperty botVersion;

    protected SerializedProperty generation;

    protected SerializedProperty initialActionTime;
    protected SerializedProperty nextActionsTime;

    //Bot weights

    public static bool botWeightsFoldout = true;

    protected SerializedProperty holesWeight;
    protected SerializedProperty bumpinessWeight;
    protected SerializedProperty linesWeight;
    protected SerializedProperty linesHolesWeight;
    protected SerializedProperty humanizedWeight;

    //Genetic algorithm variables

    public static bool geneticAlgorithmVariablesFoldout = true;

    protected SerializedProperty populationSize;
    protected SerializedProperty mutationRate;

    protected SerializedProperty pieceLimitTraining;

    //Testing variables

    public static bool testingVariablesFoldout = true;

    protected SerializedProperty initialCalculationTime;
    protected SerializedProperty decreasingCalculationTimeFactor;

    #endregion*/

    protected SerializedProperty debugMode;

    GUIStyle header1Style;
    GUIStyle header2Style;

    private void OnEnable()
    {
        boardWidth = serializedObject.FindProperty("boardWidth");
        boardHeight = serializedObject.FindProperty("boardHeight");

        spawnPos = serializedObject.FindProperty("spawnPos");

        gravityDropLimit = serializedObject.FindProperty("gravityDropLimit");
        softDropLimit = serializedObject.FindProperty("softDropLimit");
        horMovLimit = serializedObject.FindProperty("horMovLimit");
        lockedLimitTime = serializedObject.FindProperty("lockedLimitTime");

        linesToLevelUp = serializedObject.FindProperty("linesToLevelUp");
        possibleScores = serializedObject.FindProperty("possibleScores");

        /*playMode = serializedObject.FindProperty("playMode");
        botVersion = serializedObject.FindProperty("botVersion");

        generation = serializedObject.FindProperty("generation");

        initialActionTime = serializedObject.FindProperty("initialActionTime");
        nextActionsTime = serializedObject.FindProperty("nextActionsTime");

        holesWeight = serializedObject.FindProperty("holesWeight");
        bumpinessWeight = serializedObject.FindProperty("bumpinessWeight");
        linesWeight = serializedObject.FindProperty("linesWeight");
        linesHolesWeight = serializedObject.FindProperty("linesHolesWeight");
        humanizedWeight = serializedObject.FindProperty("humanizedWeight");

        populationSize = serializedObject.FindProperty("populationSize");
        mutationRate = serializedObject.FindProperty("mutationRate");

        pieceLimitTraining = serializedObject.FindProperty("pieceLimitTraining");

        initialCalculationTime = serializedObject.FindProperty("initialCalculationTime");
        decreasingCalculationTimeFactor = serializedObject.FindProperty("decreasingCalculationTimeFactor");*/

        debugMode = serializedObject.FindProperty("debugMode");

        header1Style = new GUIStyle(EditorStyles.foldoutHeader) { fontSize = 15, fontStyle = FontStyle.Bold };
        header1Style.normal.textColor = Color.yellow;

        header2Style = new GUIStyle(EditorStyles.foldoutHeader) { fontSize = 13, fontStyle = FontStyle.Bold };
        header2Style.normal.textColor = Color.white;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        gameVariablesFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(gameVariablesFoldout, "Game variables", header1Style);

        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(5);

        if (gameVariablesFoldout)
        {
            boardVariablesFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(boardVariablesFoldout, "Board variables", header2Style);

            if(boardVariablesFoldout)
            {
                EditorGUILayout.PropertyField(boardWidth);
                EditorGUILayout.PropertyField(boardHeight);
                EditorGUILayout.PropertyField(spawnPos);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(10);

            movementVariablesFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(movementVariablesFoldout, "Movement variables", header2Style);

            if(movementVariablesFoldout)
            {
                EditorGUILayout.PropertyField(gravityDropLimit);
                EditorGUILayout.PropertyField(softDropLimit);
                EditorGUILayout.PropertyField(horMovLimit);
                EditorGUILayout.PropertyField(lockedLimitTime);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(10);

            scoreVariablesFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(scoreVariablesFoldout, "Score and other variables", header2Style);

            if(scoreVariablesFoldout)
            {
                EditorGUILayout.PropertyField(linesToLevelUp);
                EditorGUILayout.PropertyField(possibleScores);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        EditorGUILayout.Space(10);

        /*botVariablesFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(botVariablesFoldout, "Bot variables", header1Style);

        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(5);

        if (botVariablesFoldout)
        {
            EditorGUILayout.PropertyField(botVersion);
            EditorGUILayout.PropertyField(generation);
            EditorGUILayout.PropertyField(initialActionTime);
            EditorGUILayout.PropertyField(nextActionsTime);

            EditorGUILayout.Space(10);

            botWeightsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(botWeightsFoldout, "Bot weights", header2Style);

            if(botWeightsFoldout)
            {
                EditorGUILayout.PropertyField(holesWeight);
                EditorGUILayout.PropertyField(bumpinessWeight);
                EditorGUILayout.PropertyField(linesWeight);
                EditorGUILayout.PropertyField(linesHolesWeight);
                EditorGUILayout.PropertyField(humanizedWeight);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(10);

            geneticAlgorithmVariablesFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(geneticAlgorithmVariablesFoldout, "Genetic algorithm variables", header2Style);

            if(geneticAlgorithmVariablesFoldout)
            {
                EditorGUILayout.PropertyField(populationSize);
                EditorGUILayout.PropertyField(mutationRate);
                EditorGUILayout.PropertyField(pieceLimitTraining);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(10);

            testingVariablesFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(testingVariablesFoldout, "Testing variables", header2Style);

            if(testingVariablesFoldout)
            {
                EditorGUILayout.PropertyField(initialCalculationTime);
                EditorGUILayout.PropertyField(decreasingCalculationTimeFactor);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        EditorGUILayout.Space(10);*/

        EditorGUILayout.PropertyField(debugMode);

        serializedObject.ApplyModifiedProperties();
    }
}

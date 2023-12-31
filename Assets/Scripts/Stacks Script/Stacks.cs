﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Stacks : MonoBehaviour {

    public Color32[] stackColors = new Color32[4];
    public Material stackMat;

    public Text scoreText, gameOverScore, tapText;
    public GameObject gameOverPanel;
    public Image newHSImage;

    private const float BOUNDS_SIZE = 3.5f;
    private const float STACK_MOVE_SPEED = 10f;
    private const float ERROR_MARGIN = 0.1f;
    private const float STACK_BOUNDS_GAIN = 0.25F;
    private const int COMBO_START_GAIN = 1;

    private GameObject[] stacks;
    private Vector2 stackBounds = new Vector2(BOUNDS_SIZE, BOUNDS_SIZE);

    private int stackIndex;
    private int scoreCount = 0;
    private int combo = 0;

    private float tileTransition = 0.0f;
    private float tileSpeed = 1.8f;

    private bool isMovingOnX = true;
    private bool gameOver = false;

    private float secondaryPosition;

    private Vector3 desiredPosition;
    private Vector3 lastTilePosition;

    public AudioSource audioSource;
    public AudioClip cling, cutClip;

    public GameObject soundHandler;

    private void Awake() {
        soundHandler = GameObject.FindGameObjectWithTag("SoundHandler");
    }

    private void Start () {
        Time.timeScale = 1f;
        stacks = new GameObject[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            stacks[i] = transform.GetChild(i).gameObject;
            ColorMesh(stacks[i].GetComponent<MeshFilter>().mesh);
        }
        stackIndex = transform.childCount -1;

        soundHandler.SetActive(false);

        combo = 0;
        
    }

    void CreateRubble(Vector3 pos, Vector3 scale) {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        go.AddComponent<Rigidbody>();

        go.GetComponent<MeshRenderer>().material = stackMat;
        ColorMesh(go.GetComponent<MeshFilter>().mesh);
        audioSource.PlayOneShot(cutClip);
    }
	
	void Update () {
        if (Input.GetMouseButtonDown(0)) {
            tapText.gameObject.SetActive(false);
            if (PlaceTiles())
            {
                SpawnTiles();
                scoreCount++;
                scoreText.text = scoreCount.ToString();
            }
            else {
                EndGame();
            } 
        }

        MoveTile();

        transform.position = Vector3.Lerp(transform.position, desiredPosition, STACK_MOVE_SPEED * Time.deltaTime);

  }

    void MoveTile() {
        if (gameOver)
            return;
            tileTransition += Time.deltaTime * tileSpeed;

        if (isMovingOnX)
            stacks[stackIndex].transform.localPosition = new Vector3(Mathf.Sin(tileTransition) * BOUNDS_SIZE, scoreCount, secondaryPosition);

        else
            stacks[stackIndex].transform.localPosition = new Vector3(secondaryPosition, scoreCount, Mathf.Sin(tileTransition) * BOUNDS_SIZE);
    }

    void SpawnTiles() {
        lastTilePosition = stacks[stackIndex].transform.localPosition;
        stackIndex--;
        if (stackIndex < 0) 
            stackIndex = transform.childCount - 1;

            desiredPosition = (Vector3.down) * scoreCount;
            stacks[stackIndex].transform.localPosition = new Vector3(0, scoreCount, 0);
            stacks[stackIndex].transform.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);

            ColorMesh( stacks[stackIndex].GetComponent<MeshFilter>().mesh);        
    }

    private bool PlaceTiles() {
        Transform t = stacks[stackIndex].transform;

        if (isMovingOnX)
        {
            // X
            float deltaX = lastTilePosition.x - t.position.x;
            if (Mathf.Abs(deltaX) > ERROR_MARGIN)
            {
                
                combo = 0;
                stackBounds.x -= Mathf.Abs(deltaX);
                if (stackBounds.x <= 0)
                    return false;

                float middle = lastTilePosition.x + t.localPosition.x / 2;
                t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);
                CreateRubble
                    ( 
                        new Vector3((t.position.x > 0)
                            ? t.position.x + (t.localScale.x / 2)
                            : t.position.x - (t.localScale.x /2),
                            t.position.y ,
                            t.position.z),
                        new Vector3(Mathf.Abs(deltaX), 1, t.localScale.z)
                    );
                t.localPosition = new Vector3(middle - (lastTilePosition.x / 2), scoreCount, lastTilePosition.z);

            } else {
                if (combo > COMBO_START_GAIN) {
                    stackBounds.x += STACK_BOUNDS_GAIN;
                    if (stackBounds.x > BOUNDS_SIZE)
                        stackBounds.x = BOUNDS_SIZE;
                    float middle = lastTilePosition.x + t.localPosition.x / 2;
                    t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);
                    t.localPosition = new Vector3(middle - (lastTilePosition.x / 2), scoreCount, lastTilePosition.z);
                }
                combo++;
                audioSource.PlayOneShot(cling);
                t.localPosition = new Vector3(lastTilePosition.x, scoreCount, lastTilePosition.z);
            } 

        
        } else {

            // Z

            float deltaZ = lastTilePosition.z - t.position.z;

            if (Mathf.Abs(deltaZ) > ERROR_MARGIN)
            {
                
                combo = 0;
                stackBounds.y -= Mathf.Abs(deltaZ);
                if (stackBounds.y <= 0)
                    return false;

                float middle = lastTilePosition.z + t.localPosition.z / 2;
                t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);
                CreateRubble
                    (
                        new Vector3 (t.position.x,
                            t.position.y,
                            (t.position.z > 0)
                            ? t.position.z + (t.localScale.z / 2)
                            : t.position.z - (t.localScale.z / 2)),
                        new Vector3(t.localScale.x, 1, Mathf.Abs(deltaZ))
                    ); 
                t.localPosition = new Vector3(lastTilePosition.x, scoreCount, middle - (lastTilePosition.z / 2));
            
            } else {
                if (combo > COMBO_START_GAIN) {
                    
                    if (stackBounds.y > BOUNDS_SIZE)
                        stackBounds.y = BOUNDS_SIZE;
                    stackBounds.y += STACK_BOUNDS_GAIN;
                    float middle = lastTilePosition.z + t.localPosition.z / 2;
                    t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);
                    t.localPosition = new Vector3(lastTilePosition.x, scoreCount, middle - (lastTilePosition.z / 2));
                }
                combo++;
                audioSource.PlayOneShot(cling);
                t.localPosition = new Vector3(lastTilePosition.x, scoreCount, lastTilePosition.z);
            } 
        } 

        secondaryPosition = (isMovingOnX)
            ? t.localPosition.x
            : t.localPosition.z;
        isMovingOnX = !isMovingOnX;

        return true;
    
    }

    private void ColorMesh(Mesh mesh) {
        Vector3[] vertices = mesh.vertices;
        Color32[] colors = new Color32[vertices.Length];
        float f = Mathf.Sin(scoreCount * 0.25f);

        for (int i = 0; i < vertices.Length; i++)
            colors[i] = Lerp4(stackColors[0], stackColors[1], stackColors[2], stackColors[3], f);

        mesh.colors32 = colors;
    }

    private Color32 Lerp4 (Color32 a, Color32 b, Color32 c, Color32 d, float t) {
        if (t < 0.33f)
            return Color.Lerp(a, b, t / 0.33f);
        else if (t < 0.66f)
            return Color.Lerp(b, c, (t - 0.33f) / 0.33f);
        else
            return Color.Lerp(c, d, (t - 0.66f) / 0.66f);
    }

    private void EndGame() {
        Debug.Log("Game is Over!!!");
        gameOver = true;
        stacks[stackIndex].AddComponent<Rigidbody>();               
        IfGameIsOver();
        audioSource.Stop();
        StartCoroutine(GameOver());
    }

    IEnumerator GameOver() {
        yield return new WaitForSeconds(1.5f);
        scoreText.gameObject.SetActive(false);
        gameOverPanel.SetActive(true);
        gameOverScore.text = scoreCount.ToString();
    }

    private void IfGameIsOver() {
        if (scoreCount > Scores.instance.GetHighScore()) {
            Scores.instance.SetHighScore(scoreCount);
            newHSImage.gameObject.SetActive(true);
        }
    }

    public void RetryGame() {
        SceneManager.LoadScene("Gameplay");
    }

    public void Menu() {
        SceneManager.LoadScene("Main");
        soundHandler.SetActive(true);
    }
}

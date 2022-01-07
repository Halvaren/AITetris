using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugScript : MonoBehaviour
{
    bool paused = false;
    float time = 0.0f;

    private void Start()
    {
        StartCoroutine(CoroutinePrueba());
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.M))
        {
            paused = !paused;
        }
    }

    IEnumerator CoroutinePrueba()
    {
        Debug.Log("esto cuantas veces se ejecuta??");

        while (time < 100000f)
        {
            if (!paused)
            {
                Debug.Log("y esto??");

                time += Time.deltaTime;
            }

            yield return null;
        }
    }
}

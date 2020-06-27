using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreText : MonoBehaviour
{
    private Vector3 start;         //the starting point for lerping movement
    private Vector3 end;           //ending point

    public Vector3 offset = new Vector3(0f, 1f, 0f); //added relative distance to move
    public float randomX = 0f;   //randomize movement left/right +/- this value
    public float time = 1.5f;      //time to move. this with distance will determine speed

    public AnimationCurve acMove;  //animation curve to move transform
    public AnimationCurve acAlpha; //animation curve to move alpha

    public string SortingLayerName = "Default";

    private void Awake()
    {
        gameObject.GetComponent<MeshRenderer>().sortingLayerName = SortingLayerName;
    }

    void Start()
    {
        //get start position
        start = transform.position;

        //get offset
        end = start + offset;

        //move the x randomly left/right a little
        end.x += Random.Range(-randomX, randomX);

        //start the animation
        StartCoroutine(Animate(start, end));
    }

    IEnumerator Animate(Vector3 pos1, Vector3 pos2)
    {
        //current time
        float timer = 0.0f;

        //distance along animation curve, should be from 0 to 1
        float evalMove = 0.0f;
        float evalAlpha = 0.0f;

        //get the textmesh
        TextMesh textMesh = GetComponent<TextMesh>();

        //while we still have animation time
        while (timer <= time)
        {
            //get our distance for move/alpha from animation curve
            evalMove = acMove.Evaluate(timer / time);
            evalAlpha = 1f - acAlpha.Evaluate(timer / time);

            //lerp our position
            transform.position = Vector3.Lerp(pos1, pos2, evalMove);

            //get the current color
            Color color = textMesh.color;
            //set the alpha and color
            color.a = evalAlpha;
            textMesh.color = color;

            //add time passed so far
            timer += Time.deltaTime;
            //wait till next frame
            yield return null;
        }

        //animation done, remove game object
        Destroy(gameObject);
    }
}

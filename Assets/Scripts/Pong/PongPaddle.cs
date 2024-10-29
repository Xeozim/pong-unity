using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

// Base class for paddles (player and AI)
public class PongPaddle : MonoBehaviour
{
    new MeshRenderer renderer;
    [SerializeField] protected List<Collider> colliders;
    [SerializeField] protected Transform paddle;
    [SerializeField] protected PongSettings settings;

    protected void GetComponentReferences(){
        renderer = paddle.GetComponent<MeshRenderer>();
    }

    private void Awake()
    {
        GetComponentReferences();
    }

    protected void SettingsRefresh(){
        paddle.transform.localScale = new Vector3(settings.paddleWidth,settings.paddleHeight,1);
    }

    protected void OnGameOverStateUpdated(bool isGameOver)
    {
        renderer.enabled = !isGameOver;
        colliders.Select(c => c.enabled = !isGameOver);

        if (!isGameOver) {
            // Game was restarted, reset to the centre of the screen
            paddle.transform.position = new Vector3(paddle.transform.position.x,0,paddle.transform.position.z);
        }
    }
}

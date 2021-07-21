using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour
{
    private Light directLight;
    private PlayerController playerControllerScript;
    private SpriteRenderer background;
    private int plus = 1;
    private Color backgroungInitialColor;

    // Start is called before the first frame update
    void Start()
    {
        directLight = GameObject.Find("Directional Light").GetComponent<Light>();
        playerControllerScript = GameObject.Find("Player").GetComponent<PlayerController>();
        background = GameObject.Find("Background").GetComponent<SpriteRenderer>();
        backgroungInitialColor = background.color;
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerControllerScript.gameOver)
        {

            // Debug.Log(directLight.intensity);
            if (directLight.intensity == 0 && plus > 0) {
                plus = -1;
            }else if (directLight.intensity >= 1.3f && plus < 0) {
                plus = 1;
            }

            directLight.intensity -= 0.00008f * plus;
            RenderSettings.ambientIntensity -= 0.00008f * plus;
            RenderSettings.reflectionIntensity -= 0.00008f * plus;

            if(directLight.intensity > 0 && directLight.intensity < 1.0f) {
                if (plus > 0){
                    background.color = Color.Lerp(background.color, new Color(0, 0, 0), 0.0002f);
                }else{
                    background.color = Color.Lerp(background.color, backgroungInitialColor, 0.0002f);
                }
            }
            
             
        }
    }
}

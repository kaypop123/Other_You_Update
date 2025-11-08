using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeySetAnime : MonoBehaviour
{
    public GameObject key1;
    public GameObject key2;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Anime());
    }

    // Update is called once per frame
    private void OnDisable()
    {
        StopCoroutine(Anime());
    }
    IEnumerator Anime()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            key1.SetActive(true);
            key2.SetActive(false);

            yield return new WaitForSeconds(1f);
            key1.SetActive(false);
            key2.SetActive(true);
        }
    }

    public void StopCo()
    {
        StopCoroutine(Anime());
    }
}

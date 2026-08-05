using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class JudgementPool : MonoBehaviour
{
    private readonly List<SpriteRenderer> renderers = new();
    private GameObject prefab;
    private Transform poolParent;
    private Vector3 creationPosition;

    public int TotalCount => renderers.Count;

    public int ActiveCount
    {
        get
        {
            int count = 0;
            foreach (SpriteRenderer renderer in renderers)
            {
                if (renderer.gameObject.activeSelf)
                {
                    count++;
                }
            }

            return count;
        }
    }

    public void Initialize(
        GameObject judgementPrefab,
        Transform parent,
        Vector3 spawnPosition,
        int initialSize)
    {
        prefab = judgementPrefab;
        poolParent = parent;
        creationPosition = spawnPosition;
        renderers.Clear();

        for (int index = 0; index < initialSize; index++)
        {
            renderers.Add(Create());
        }
    }

    public SpriteRenderer Rent()
    {
        foreach (SpriteRenderer renderer in renderers)
        {
            if (!renderer.gameObject.activeSelf)
            {
                renderer.gameObject.SetActive(true);
                return renderer;
            }
        }

        SpriteRenderer expandedRenderer = Create();
        renderers.Add(expandedRenderer);
        expandedRenderer.gameObject.SetActive(true);
        return expandedRenderer;
    }

    public void ReturnAll()
    {
        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.gameObject.SetActive(false);
        }
    }

    private SpriteRenderer Create()
    {
        GameObject judgement = Instantiate(
            prefab,
            creationPosition,
            Quaternion.identity,
            poolParent);
        SpriteRenderer renderer = judgement.GetComponent<SpriteRenderer>();
        judgement.SetActive(false);
        return renderer;
    }
}

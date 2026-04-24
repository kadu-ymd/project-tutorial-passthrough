using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using Meta.XR.MRUtilityKit;
using System.Collections;

public class RuntimeNavmeshBuilder : MonoBehaviour
{
    private NavMeshSurface navMeshSurface;

    void Start()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();
        navMeshSurface.BuildNavMesh();

        MRUK.Instance.RegisterSceneLoadedCallback(BuildNavMesh);
    }

    public void BuildNavMesh()
    {
        StartCoroutine(BuildNavMeshRoutine());
    }

    public IEnumerator BuildNavMeshRoutine()
    {
        yield return new WaitForEndOfFrame();
        navMeshSurface.BuildNavMesh();
    }
}

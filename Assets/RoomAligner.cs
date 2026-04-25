using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Collections.Generic;

public class RoomAligner : MonoBehaviour
{
    [Header("Prefab Reconstruído")]
    [Tooltip("Seu prefab exportado da sala real (arraste aqui no Inspector)")]
    public GameObject reconstructedRoomPrefab;

    [Header("Configurações")]
    [Tooltip("Se true, alinha automaticamente quando MRUK carregar")]
    public bool alignOnStart = true;

    [Tooltip("Tempo mínimo de espera para o MRUK carregar (segundos)")]
    public float waitForMRUK = 1f;

    [Tooltip("Se true, destroi o room original após alinhar. Se false, apenas desativa")]
    public bool destroyOriginal = false;

    [Tooltip("Offset manual adicional após alinhamento automático")]
    public Vector3 manualOffset = Vector3.zero;

    [Tooltip("Se true, copia a rotação EXATA do room original. Se false, apenas alinha posição")]
    public bool copyExactRotation = true;

    // Referências em runtime
    private GameObject originalRoom;
    private GameObject reconstructedRoom;

    void Start()
    {
        if (alignOnStart)
            StartCoroutine(AlignWhenReady());
    }

    System.Collections.IEnumerator AlignWhenReady()
    {
        yield return new WaitForSeconds(waitForMRUK);

        while (MRUK.Instance == null)
            yield return null;

        while (MRUK.Instance.GetCurrentRoom() == null)
            yield return null;

        yield return null;

        AlignRooms();
    }

    public void AlignRooms()
    {
        var room = MRUK.Instance.GetCurrentRoom();
        if (room == null)
        {
            Debug.LogError("RoomAligner: MRUK room não encontrado!");
            return;
        }

        // 1. ENCONTRA O ROOM ORIGINAL
        originalRoom = FindOriginalRoom();

        if (originalRoom == null)
        {
            Debug.LogWarning("RoomAligner: Room original não encontrado. Criando sem referência.");
        }
        else
        {
            Debug.Log($"RoomAligner: Room original encontrado: {originalRoom.name}");
            Debug.Log($"RoomAligner: Posição original: {originalRoom.transform.position}, Rotação: {originalRoom.transform.rotation.eulerAngles}");
        }

        // 2. INSTANCIA O RECONSTRUÍDO
        reconstructedRoom = Instantiate(reconstructedRoomPrefab);
        reconstructedRoom.name = "ReconstructedRoom_Runtime";

        // 3. ALINHA POSIÇÃO E ROTAÇÃO
        if (originalRoom != null)
        {
            // OPÇÃO A: Copiar transform EXATO do original (mais simples, mais confiável)
            if (copyExactRotation)
            {
                reconstructedRoom.transform.SetPositionAndRotation(
                    originalRoom.transform.position,
                    originalRoom.transform.rotation
                );
            }
            else
            {
                // OPÇÃO B: Só posição, rotação zero
                reconstructedRoom.transform.position = originalRoom.transform.position;
            }
        }
        else
        {
            // Sem original: usa centro do room real do MRUK
            Vector3 realCenter = CalculateRoomCenter(room);
            reconstructedRoom.transform.position = realCenter;
        }

        // 4. AJUSTE FINO POR ANCHOR (para corrigir desvios de rotação individual)
        if (originalRoom != null && copyExactRotation)
        {
            AlignAnchorsByName(originalRoom, reconstructedRoom);
        }

        // 5. APLICA OFFSET MANUAL
        reconstructedRoom.transform.position += manualOffset;

        // 6. DESATIVA/DESTROI ORIGINAL
        if (originalRoom != null)
        {
            if (destroyOriginal)
                Destroy(originalRoom);
            else
                originalRoom.SetActive(false);
        }

        Debug.Log($"✅ Room alinhada! Pos: {reconstructedRoom.transform.position}, Rot: {reconstructedRoom.transform.rotation.eulerAngles}");
    }

    // NOVO: Alinha anchors individuais por nome para corrigir rotação torta
    void AlignAnchorsByName(GameObject original, GameObject reconstructed)
    {
        // Dicionário de anchors originais por nome
        Dictionary<string, Transform> originalAnchors = new Dictionary<string, Transform>();
        foreach (Transform child in original.GetComponentsInChildren<Transform>())
        {
            if (!originalAnchors.ContainsKey(child.name))
                originalAnchors.Add(child.name, child);
        }

        // Para cada anchor reconstruído, copia rotação do original correspondente
        foreach (Transform reconChild in reconstructed.GetComponentsInChildren<Transform>())
        {
            if (originalAnchors.TryGetValue(reconChild.name, out Transform originalAnchor))
            {
                // Copia rotação local do anchor original
                reconChild.localRotation = originalAnchor.localRotation;

                // Se quiser copiar posição local também (descomente):
                // reconChild.localPosition = originalAnchor.localPosition;

                Debug.Log($"Anchor alinhado: {reconChild.name} | Rot: {reconChild.localRotation.eulerAngles}");
            }
        }
    }

    GameObject FindOriginalRoom()
    {
        // Estratégia 1: Via MRUKRoom component
        MRUKRoom mrukRoom = MRUK.Instance.GetCurrentRoom();
        if (mrukRoom != null && mrukRoom.gameObject != null)
        {
            return mrukRoom.gameObject;
        }

        // Estratégia 2: Procurar por nomes comuns
        string[] commonNames = { "Room", "MRUKRoom", "GeneratedRoom", "SceneRoom", "Office_Small", "LivingRoom" };
        foreach (var name in commonNames)
        {
            GameObject found = GameObject.Find(name);
            if (found != null) return found;
        }

        // Estratégia 3: Procurar pelo primeiro MRUKAnchor
        MRUKAnchor[] anchors = FindObjectsOfType<MRUKAnchor>();
        if (anchors.Length > 0)
        {
            return anchors[0].transform.root.gameObject;
        }

        return null;
    }

    Vector3 CalculateRoomCenter(MRUKRoom room)
    {
        if (room.Anchors == null || room.Anchors.Count == 0)
            return Vector3.zero;

        Vector3 sum = Vector3.zero;
        int count = 0;

        foreach (var anchor in room.Anchors)
        {
            if (anchor != null)
            {
                sum += anchor.transform.position;
                count++;
            }
        }

        return count > 0 ? sum / count : Vector3.zero;
    }
}
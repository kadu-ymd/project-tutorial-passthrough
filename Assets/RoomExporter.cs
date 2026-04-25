using UnityEngine;
using Meta.XR.MRUtilityKit;

public class RoomExporter : MonoBehaviour
{
    [Header("Configurações")]
    public bool exportOnStart = true;
    public string prefabName = "MyExportedRoom";

    void Start()
    {
        if (exportOnStart)
            StartCoroutine(WaitAndExport());
    }

    System.Collections.IEnumerator WaitAndExport()
    {
        // Aguarda o MRUK carregar
        while (MRUK.Instance == null || MRUK.Instance.GetCurrentRoom() == null)
            yield return null;

        yield return null; // Mais um frame para garantir
        ExportRoom();
    }

    void ExportRoom()
    {
        var room = MRUK.Instance.GetCurrentRoom();
        if (room == null)
        {
            Debug.LogError("MRUK: Nenhum room carregado!");
            return;
        }

        // Criar GameObject raiz na cena ativa
        GameObject roomRoot = new GameObject("ExportedRoom_" + room.name);

        // Iterar sobre todos os anchors do room
        // CORREÇÃO: room.Anchors (não room.GetRoomAnchors())
        foreach (var anchor in room.Anchors)
        {
            // Criar primitiva representando o anchor
            GameObject anchorObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            anchorObj.transform.SetParent(roomRoot.transform);

            // CORREÇÃO: Usar transform do próprio anchor (ele é um MonoBehaviour)
            anchorObj.transform.position = anchor.transform.position;
            anchorObj.transform.rotation = anchor.transform.rotation;

            // CORREÇÃO: Obter escala correta baseada em PlaneRect ou VolumeBounds
            Vector3 scale = GetAnchorScale(anchor);
            anchorObj.transform.localScale = scale;

            // CORREÇÃO: anchor.Label é uma propriedade de instância
            anchorObj.name = anchor.Label.ToString();

            // Ajustar material para visualização
            Renderer rend = anchorObj.GetComponent<Renderer>();
            rend.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            rend.material.color = GetColorForLabel(anchor.Label);
        }

        Debug.Log($"Room exportada com {room.Anchors.Count} anchors");
        LogRoomData(room);
    }

    // CORREÇÃO: Método para obter escala correta do anchor
    Vector3 GetAnchorScale(MRUKAnchor anchor)
    {
        if (anchor.PlaneRect.HasValue)
        {
            // Para paredes, chão, teto: escala do plano
            var rect = anchor.PlaneRect.Value;
            return new Vector3(rect.size.x, rect.size.y, 0.01f); // 0.01 = espessura
        }
        else if (anchor.VolumeBounds.HasValue)
        {
            // Para volumes (sofá, mesa, etc.)
            var bounds = anchor.VolumeBounds.Value;
            return bounds.size;
        }

        // Fallback
        return Vector3.one;
    }

    void LogRoomData(MRUKRoom room)
    {
        foreach (var anchor in room.Anchors)
        {
            Debug.Log($"ANCHOR|{anchor.Label}|{anchor.transform.position}|{anchor.transform.rotation.eulerAngles}|{GetAnchorScale(anchor)}");
        }
    }

    // CORREÇÃO: anchor.Label é MRUKAnchor.SceneLabels (enum), não string
    Color GetColorForLabel(MRUKAnchor.SceneLabels label)
    {
        return label switch
        {
            MRUKAnchor.SceneLabels.WALL_FACE => new Color(0.8f, 0.8f, 0.9f),
            MRUKAnchor.SceneLabels.FLOOR => new Color(0.6f, 0.4f, 0.2f),
            MRUKAnchor.SceneLabels.CEILING => new Color(0.9f, 0.9f, 0.9f),
            MRUKAnchor.SceneLabels.WINDOW_FRAME => new Color(0.4f, 0.7f, 0.9f, 0.5f),
            MRUKAnchor.SceneLabels.DOOR_FRAME => new Color(0.5f, 0.3f, 0.1f),
            MRUKAnchor.SceneLabels.COUCH => new Color(0.6f, 0.3f, 0.3f),
            MRUKAnchor.SceneLabels.TABLE => new Color(0.5f, 0.4f, 0.3f),
            _ => Color.gray
        };
    }
}
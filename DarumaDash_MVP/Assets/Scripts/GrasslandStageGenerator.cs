// Daruma Dash - 草原ステージ用プレハブ生成コード（Grid + Tilemap）
// ノアによる自動出力テンプレート

using UnityEngine;
using UnityEngine.Tilemaps;

public class GrasslandStageGenerator : MonoBehaviour
{
    public Tilemap groundMap;
    public Tilemap obstacleMap;
    public TileBase grassTile;
    public TileBase[] obstacleTiles;

    public Vector2Int mapSize = new Vector2Int(15, 20);

    void Start()
    {
        GenerateGround();
        PlaceObstacles();
    }

    void GenerateGround()
    {
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                groundMap.SetTile(new Vector3Int(x, y, 0), grassTile);
            }
        }
    }

    void PlaceObstacles()
    {
        // 角に木、真ん中にテント風の遮蔽物（例）
        obstacleMap.SetTile(new Vector3Int(0, 0, 0), obstacleTiles[0]); // 木
        obstacleMap.SetTile(new Vector3Int(mapSize.x - 1, 0, 0), obstacleTiles[0]);
        obstacleMap.SetTile(new Vector3Int(0, mapSize.y - 1, 0), obstacleTiles[0]);
        obstacleMap.SetTile(new Vector3Int(mapSize.x - 1, mapSize.y - 1, 0), obstacleTiles[0]);

        // 中央の十字型遮蔽
        int centerX = mapSize.x / 2;
        int centerY = mapSize.y / 2;
        obstacleMap.SetTile(new Vector3Int(centerX, centerY, 0), obstacleTiles[1]);
        obstacleMap.SetTile(new Vector3Int(centerX + 1, centerY, 0), obstacleTiles[1]);
        obstacleMap.SetTile(new Vector3Int(centerX - 1, centerY, 0), obstacleTiles[1]);
        obstacleMap.SetTile(new Vector3Int(centerX, centerY + 1, 0), obstacleTiles[1]);
        obstacleMap.SetTile(new Vector3Int(centerX, centerY - 1, 0), obstacleTiles[1]);
    }
}

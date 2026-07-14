using UnityEngine;

// 유니티 에디터 우클릭 메뉴에 플레이어 데이터 생성 버튼을 추가합니다.
[CreateAssetMenu(fileName = "NewPlayerPiece", menuName = "ChessGame/PlayerPieceData")]
public class PlayerPieceSO : ChessPieceSO
{
    // 플레이어는 스폰 확률이나 지속 시간 데이터가 필요 없으므로,
    // ChessPieceSO의 공통 데이터(기물 종류)만 그대로 물려받고 여기는 비워둡니다.
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckersBoard : MonoBehaviour
{
	public Piece[,] pieces = new Piece[8,8];
	public GameObject whitePiecePrefab;
	public GameObject blackPiecePrefab;

	private Vector3 boardOffset = new Vector3(-4.0f, 0, -4.0f);
	private Vector3 pieceOffset = new Vector3(0.5f, 0, 0.5f);

	public bool isWhite;
	public bool isWhiteTurn; //change to isblackturn
	private bool hasKilled;

	private Piece selectedPiece;
	private List<Piece> forcedPieces;

	private Vector2 mouseOver;
	private Vector2 startDrag;
	private Vector2 endDrag;
	
	private void Start(){
		GenerateBoard();
		forcedPieces = new List<Piece>();
		isWhiteTurn = true;
	}

	private void Update(){
		UpdateMouseOver();

		Debug.Log(mouseOver);

		//If my turn
		//if((isWhite)?isWhiteTurn:!isWhiteTurn)
		
			int x = (int)mouseOver.x;
			int y = (int)mouseOver.y;

			if (selectedPiece != null) {
				UpdatePieceDrag(selectedPiece);
			}

			if (Input.GetMouseButtonDown(0)) {
				SelectPiece(x,y);
			}

			if (Input.GetMouseButtonUp(0)) {
				TryMove((int)startDrag.x, (int)startDrag.y, x, y);
			}
		
	}

	private void UpdateMouseOver(){
		if(!Camera.main) {
			Debug.Log("Unable to find main camera");
			return;
		}

		RaycastHit hit;
		if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),out hit, 25.0f, LayerMask.GetMask("Board"))) {
			mouseOver.x = (int)(hit.point.x - boardOffset.x);
			mouseOver.y = (int)(hit.point.z - boardOffset.z);
		} else {
			mouseOver.x = -1;
			mouseOver.y = -1;
		}
	}

	private void UpdatePieceDrag(Piece P) {
		if(!Camera.main) {
			Debug.Log("Unable to find main camera");
			return;
		}

		RaycastHit hit;
		if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),out hit, 25.0f, LayerMask.GetMask("Board"))) {
			P.transform.position = hit.point + Vector3.up;
		}
	}
	
	private void SelectPiece(int x, int y)
	{
		//Outofbounds
		if (x < 0 || x >= 8 || y < 0 || y >= 8)
			return;

		Piece p = pieces[x,y];
		if (p != null && p.isWhite == isWhiteTurn) {
			/*
			if(forcedPieces.Count == 0) {
				selectedPiece = p;
				startDrag = mouseOver;
			} else {
				//look for piece under forcedpieces list
				if(forcedPieces.Find(fp => fp == p) == null)
					return;
				selectedPiece = p;
				startDrag = mouseOver;
			}
			Debug.Log(selectedPiece.name);
			*/
			selectedPiece = p;
			startDrag = mouseOver;
		}
	}

	private void TryMove(int x1, int y1, int x2, int y2) {
		forcedPieces = ScanForPossibleMove();
		
		//Multiplayer support
		startDrag = new Vector2(x1 ,y1);
		endDrag = new Vector2(x2, y2);
		selectedPiece = pieces[x1, y1];

		//MovePiece(selectedPiece, x2, y2);

		//Check if we are out of bounds
		if(x2 < 0 ||  x2 >= 8 || y2 < 0 || y2 >= 8) { //(x2 < 0 || x2 >= 8 || y2 < 0 || y2 >= 8
			//move back to original
			if (selectedPiece != null) {
				MovePiece(selectedPiece, x1, y1);
			}
			startDrag = Vector2.zero;
			selectedPiece = null;
			return;
		}

		if (selectedPiece != null) {
			//if it hasnt moved
			if (endDrag == startDrag) {
				MovePiece(selectedPiece, x1, y1);
				startDrag = Vector2.zero;
				selectedPiece = null;
				return;
			}
			//check valid move
			if (selectedPiece.ValidMove(pieces, x1, y1, x2, y2)) {
				//check if killed anything
				//if this is jump

				if(Mathf.Abs(x2 - x1) == 2) {
					Piece p = pieces[(x1 + x2) / 2, (y1 + y2) / 2];
					if (p != null) {
						pieces[(x1 + x2) / 2, (y1 + y2) / 2] = null;
						Destroy(p.gameObject);
						hasKilled = true;
					}
				}

				//were we meant to kill anything?
				
				if(forcedPieces.Count != 0 && !hasKilled) {
					MovePiece(selectedPiece, x1, y1);
					startDrag = Vector2.zero;
					selectedPiece = null;
					return;
				}
				

				pieces[x2, y2] = selectedPiece;
				pieces[x1, y1] = null;
				MovePiece(selectedPiece, x2, y2);

				EndTurn();
			} else {
				MovePiece(selectedPiece, x1, y1);
				startDrag = Vector2.zero;
				selectedPiece = null;
				return;
			}
		}
		
	}

	private void EndTurn() {
		int x = (int)endDrag.x;
		int y = (int)endDrag.y;

		//promotions
		if(selectedPiece != null) {
			if(selectedPiece.isWhite && !selectedPiece.isKing && y == 7) {
				selectedPiece.isKing = true;
				selectedPiece.transform.Rotate(Vector3.right * 180);
			} else if (!selectedPiece.isWhite && !selectedPiece.isKing && y == 0) {
				selectedPiece.isKing = true;
				selectedPiece.transform.Rotate(Vector3.right * 180);
			}
		}
		selectedPiece = null;
		startDrag = Vector2.zero;

		//if(ScanForPossibleMove(selectedPiece, x, y).Count != 0 && hasKilled) {
			//return;
		//}

		isWhiteTurn = !isWhiteTurn;
		hasKilled = false;
		CheckVictory();
	}

	private void CheckVictory() {
		var ps = FindObjectsOfType<Piece>();
		bool hasWhite = false, hasBlack = false;
		for(int i = 0; i < ps.Length; i++) {
			if(ps[i].isWhite) {
				hasWhite = true;
			} else {
				hasBlack = true;
			}
		}

		if (!hasWhite)
			Victory(false);
		if (!hasBlack)
			Victory(true);
		
	}

	private void Victory(bool isWhite) {
		if(isWhite)
			Debug.Log("white won");
		else
			Debug.Log("black won");
	}
	/*
	private List<Piece> ScanForPossibleMove(Piece p, int x, int y) {
		forcedPieces = new List<Piece>();

		if(pieces[x,y].IsForceToMove(pieces, x, y))
			forcedPieces.Add(pieces[x,y]);

		return forcedPieces;
	}
	*/
	
	private List<Piece> ScanForPossibleMove() {
		forcedPieces = new List<Piece>();
		
		//check pieces
		for(int i = 0; i < 8; i++) {
			for(int j = 0; j < 8; j++) {
				if(pieces[i,j] != null && pieces[i,j].isWhite == isWhiteTurn) {
					if(pieces[i,j].IsForceToMove(pieces, i, j))
						forcedPieces.Add(pieces[i,j]);
				}
			}
		}
		//Debug.Log(forcedPieces);
		return forcedPieces;
	}
	
   private void GenerateBoard()
   {
		//Generate White Team
		for(int y = 0; y < 3; y++) {
			bool oddRow = (y % 2 ==0);
			for(int x = 0; x < 8; x+=2) {
				//Generate our piece
				GeneratePiece((oddRow)? x : x+1,y);
			}
		}

		//Generate Black Team
		for(int y = 7; y > 4; y--) {
			bool oddRow = (y % 2 ==0);
			for(int x = 0; x < 8; x+=2) {
				//Generate our piece
				GeneratePiece((oddRow)? x : x+1,y);
			}
		}

   }

   private void GeneratePiece(int x, int y) {
   		bool isPieceWhite = (y < 3)? true : false;
		GameObject go = Instantiate((isPieceWhite)? whitePiecePrefab : blackPiecePrefab) as GameObject;
		go.transform.SetParent(transform);
		Piece p = go.GetComponent<Piece>();
		pieces[x,y] = p;
		MovePiece(p, x, y);
		
   }

   private void MovePiece(Piece p, int x, int y) {
		p.transform.position = (Vector3.right * x) + (Vector3.forward * y) + boardOffset + pieceOffset;
   }	
}

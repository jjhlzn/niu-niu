using UnityEngine;  
using UnityEngine.UI;  
using UnityEngine.EventSystems;  
using System.Collections;  
using System.Collections.Generic;
using System;

public class UIEraserTexture : MonoBehaviour ,IPointerDownHandler,IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler{  
	[SerializeField]
	public  RawImage image;  

	public  int brushScale = 20;  
	bool isMove = false;  

	bool[][] matrix;

	Texture2D texRender;  
	RectTransform mRectTransform;  
	Canvas canvas;  

	Vector2 start = Vector2.zero;  
	Vector2 end = Vector2.zero;  

	private Queue<Vector2> queue;
	private List<Vector2> positions;

	void Awake(){ 
		//Time.timeScale = 0.0001f;
		queue = new Queue<Vector2> ();
		mRectTransform = GetComponent<RectTransform> ();  
		canvas = GameObject.Find("Canvas").GetComponent<Canvas>();  
	}  

	void Start () {  
		texRender = new Texture2D(image.mainTexture.width, image.mainTexture.height,TextureFormat.RGBA32,true);  
		Reset ();  
	}  

	public void OnPointerEnter (PointerEventData data) {
		//Debug.Log ("OnPointerEnter..."+data.position);  
		start = data.position;  
		isMove = true;  
	}

	public void OnPointerExit (PointerEventData data) {
		isMove = false;  
		//Debug.Log ("OnPointerUp..."+data.position);   
		AddPoints (start, data.position);
	}
		
	public void OnPointerDown(PointerEventData data){  
		//Debug.Log ("OnPointerDown..."+data.position);  
		start = data.position;  
		isMove = true;  
	}  

	public void OnPointerUp(PointerEventData data){  
		isMove = false;  
		//Debug.Log ("OnPointerUp..."+data.position);  
		AddPoints (start, data.position);
	}

	private int GetPointCount(float distance) {
		Debug.Log ("distance = " + distance);
		if (distance > 400) {
			return 100;
		} else if (distance > 200) {
			return 50;
		} else if (distance > 100) {
			return 20;
		} else if (distance > 50) {
			return 10;
		} 
		else if (distance < 10) {
			return 4;
		} else if (distance < 1) {
			return 1;
		}
		return 10;
	}

	private bool IsDrawed(Vector3 position) {
		position = ConvertSceneToUI (position);
		Rect rect = new Rect (position.x + texRender.width/2, position.y + texRender.height/2, brushScale, brushScale);

		for (int x = (int)rect.xMin; x < (int)rect.xMax; x++) {  
			for (int y = (int)rect.yMin; y < (int)rect.yMax; y++) {
				if (x< 0 || x >= matrix.Length) {
					return true;
				}
				if (y < 0 || y >= texRender.height ) {
					return true;
				}

				if ( !matrix[x][y] )
					return false;
			}
		}
		return true;

		/*
		Vector2 a = ConvertSceneToUI (position);
		int x = (int)(a.x + texRender.width / 2);
		int y = (int)(a.y + texRender.height /2); */

	}

	public void AddPoints(Vector3 start, Vector3 end) {
		float xDis = end.x - start.x;
		float yDis = end.y - start.y;

		int pointCount = GetPointCount(Math.Abs(xDis) > Math.Abs(yDis) ? Math.Abs(xDis) : Math.Abs(yDis));
		float stepX = xDis / pointCount;
		float stepY = yDis / pointCount;

		for (int i = 0; i < pointCount; i++) {
			Vector3 point = new Vector3 (start.x + i * stepX, start.y + i * stepY);
			if (!IsDrawed(point))
				queue.Enqueue(point);
		}
	}
		
	void Update(){  
		
		if (isMove) {
			AddPoints (start, Input.mousePosition);
			start  = Input.mousePosition;
		}  
		Draw ();
	}  


	void Draw() {
		while (queue.Count > 0 && this.gameObject.activeInHierarchy) {
			StartCoroutine( Draw (queue.Dequeue()));  
		}
	}

	Vector2 ConvertSceneToUI(Vector3 posi){  
		Vector2 postion;  
		if(RectTransformUtility.ScreenPointToLocalPointInRectangle(mRectTransform , posi, canvas.worldCamera, out postion)){  
			return postion;  
		}  
		return Vector2.zero;  
	}  

	IEnumerator Draw(Vector2 position)  
	{  
		Vector2 a = ConvertSceneToUI (position);
		Draw (new Rect (a.x + texRender.width/2, a.y + texRender.height/2, brushScale, brushScale));
		yield return new WaitForSeconds(0f);
	}  
		
	private int rightConnerX = 0, rightConnerY = 0;
	private int leftConnerX= 0, leftConnerY = 0;
	private int rightConnerCount = 0;
	private int leftConnerCount = 0;

	private int totalCuoCount = 1;
	private int totalCount = 1;
	private int totalConnerCount = 1;
	void Draw(Rect rect){  

		for (int x = (int)rect.xMin; x < (int)rect.xMax; x++) {  
			for (int y = (int)rect.yMin; y < (int)rect.yMax; y++) {
				
				if (x < 0 || x >= texRender.width || y < 0 || y >= texRender.height) {  
					return;
				}  

				Color color = Color.red;  
				color.a = 0;  
				if (!matrix [x] [y]) {
					if (x >= rightConnerX && y <= rightConnerY) {
						this.rightConnerCount++;
					} else if (x <= leftConnerX && y >= leftConnerY) {
						this.leftConnerCount++;
					}
					this.totalCuoCount++;
					texRender.SetPixel (x, y, color);
					matrix [x] [y] = true;
				}

			}  
		}  

		texRender.Apply();  

		image.material.SetTexture ("_RendTex",texRender);  

		if (CheckIsEnough ()) {
			this.image.gameObject.SetActive (false);
		}
	}  


	void Reset(){  
		Debug.Log ("texRender.width = " + texRender.width);
		Debug.Log ("texRender.height = " + texRender.height);
		matrix = new bool[texRender.width ][];
		for (int i = 0; i < texRender.width; i++) {  
			matrix [i] = new bool[texRender.height];
			for (int j = 0; j < texRender.height; j++) {  
				
				Color color = texRender.GetPixel (i,j);  
				color.a = 1;  
				texRender.SetPixel (i,j,color);  
				matrix [i] [j] = false;
			}  
		}  
		texRender.Apply ();  
		image.material.SetTexture ("_RendTex",texRender);  

		for (int i = 0 ; i < texRender.width; i++) {  
			for (int j = 0; j < texRender.height; j++) {  
				totalCount++;
			}  
		}

		for (int i = (int)(texRender.width * 0.8) ; i < texRender.width; i++) {  
			for (int j = (int)(texRender.height * 0.75); j < texRender.height; j++) {  
				totalConnerCount++;
			}  
		}

		rightConnerX = (int)(texRender.width * 0.8);
		rightConnerY = (int)(texRender.height * 0.25);

		leftConnerX =  (int)(texRender.width * 0.2);
		leftConnerY = (int)(texRender.height * 0.75); 

	}  


	private bool CheckIsEnough() {
		//Debug.Log ("rightConnerCount = " + (float) this.rightConnerCount / this.totalConnerCount);
		if ((float)rightConnerCount / totalConnerCount > 0.53f)
			return true;
		else if ((float)leftConnerCount / totalConnerCount > 0.53f)
			return true;
		else if ((float)totalCuoCount / totalCount > 0.60f) {
			return true;
		}
		return false;
	}
		

}  
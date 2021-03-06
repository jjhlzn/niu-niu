﻿using UnityEngine;  
using UnityEngine.UI;  
using UnityEngine.EventSystems;  
using System.Collections;  
using System.Collections.Generic;
using System;

public class UIEraserTexture : MonoBehaviour ,IPointerDownHandler,IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler{  
	[SerializeField]
	private  RawImage image;  

	[SerializeField]
	private CheckCardController checkCardController;

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
	public bool ReadyForErase = false;

	void Awake(){ 
		//Time.timeScale = 0.0001f;
		queue = new Queue<Vector2> ();
		mRectTransform = GetComponent<RectTransform> ();
		//mRectTransform.localScale = new Vector2 (0.5f, 0.5f);
		canvas = GameObject.Find("Canvas").GetComponent<Canvas>();  
	}  

	void Start () {  
		texRender = new Texture2D(image.mainTexture.width, image.mainTexture.height,TextureFormat.RGBA32,true);
		ReadyForErase = true;
		Debug.Log ("checkCardController != null : " + (checkCardController != null));
		if (checkCardController != null) 
			checkCardController.eraser = this;
		Init ();  
	}  

	public void OnPointerEnter (PointerEventData data) {
		if (!this.image.gameObject.activeInHierarchy || !ReadyForErase)
			return;
		//Debug.Log ("OnPointerEnter..."+data.position);  
		start = data.position;  
		isMove = true;  
	}

	public void OnPointerExit (PointerEventData data) {
		if (!this.image.gameObject.activeInHierarchy  || ReadyForErase)
			return;
		isMove = false;  
		//Debug.Log ("OnPointerUp..."+data.position);   
		AddPoints (start, data.position);
	}
		
	public void OnPointerDown(PointerEventData data){  
		if (!this.image.gameObject.activeInHierarchy  || ReadyForErase)
			return;
		//Debug.Log ("OnPointerDown..."+data.position);  
		start = data.position;  
		isMove = true;  
	}  

	public void OnPointerUp(PointerEventData data){  
		
		if (!this.image.gameObject.activeInHierarchy  || ReadyForErase)
			return;
		isMove = false;  
		//Debug.Log ("OnPointerUp..."+data.position);  
		AddPoints (start, data.position); 
	}

	private int GetPointCount(float distance) {
		//Debug.Log ("distance = " + distance);
		if (distance > 400) {
			return 80;
		} else if (distance > 200) {
			return 40;
		} else if (distance > 100) {
			return 20;
		} else if (distance > 50) {
			return 10;
		} else if (distance < 10) {
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
		StartCoroutine( Draw () );
	}  


	IEnumerator Draw() {
		
		while (queue.Count > 0 && this.gameObject.activeInHierarchy) {
			Draw (queue.Dequeue());
		}
		yield return new WaitForSeconds (0);
	}

	Vector2 ConvertSceneToUI(Vector3 posi){  
		Vector2 postion;  
		if(RectTransformUtility.ScreenPointToLocalPointInRectangle(mRectTransform, posi, canvas.worldCamera, out postion)){  
			return new Vector3(postion.x / 2, postion.y/2);  
		}  
		return Vector2.zero;  
	}  

	void Draw(Vector2 position)  
	{  
		Vector2 a = ConvertSceneToUI (position);
		Draw (new Rect (a.x + texRender.width/2, a.y + texRender.height/2, brushScale, brushScale));
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

		if (CheckIsEnough () && !hasShowCard) {
			hasShowCard = true;
			this.image.gameObject.SetActive (false);
			if (checkCardController != null) {
				Debug.Log ("invoke checkCardController.ShowCardForChuoPai");
				checkCardController.ShowCardForChuoPai ();
			}

		}
	}  

	private bool hasShowCard = false;

	private bool CheckIsEnough() {
		//Debug.Log ("rightConnerCount = " + (float) this.rightConnerCount / this.totalConnerCount);
		if ((float)rightConnerCount / totalConnerCount > 0.56f)
			return true;
		else if ((float)leftConnerCount / totalConnerCount > 0.56f)
			return true;
		else if ((float)totalCuoCount / totalCount > 0.28f) {
			return true;
		}
		return false;
	}

	bool isConner(int x, int y) {
		double r = 26;  //45

		if (x < r && y < (r - x)) {
			//圆心（r, r)
			double delta = Math.Pow (x - r, 2) + Math.Pow (y - r, 2) - Math.Pow (r, 2);
			return delta > r;
		} else if (x < r && y > (this.texRender.height - r)) {
			//圆心（r, this.texRender.height - r)
			double delta = Math.Pow (x - r, 2) + Math.Pow (y - this.texRender.height + r, 2) - Math.Pow (r, 2);
			return delta > r;
		} else if (x > (this.texRender.width - r) && y < r) {
			//圆心（this.texRender.width - r, r)
			double delta = Math.Pow (x - this.texRender.width + r , 2) + Math.Pow (y - r, 2) - Math.Pow (r, 2);
			return delta > r;
		} else if (x > (this.texRender.width - r) && y > (this.texRender.height - r)) {
			//圆心（this.texRender.width - r, this.texRender.height - r)
			double delta = Math.Pow (x - this.texRender.width + r , 2) + Math.Pow (y - this.texRender.height + r, 2) - Math.Pow (r, 2);
			return delta > r;
		} else {
			return false;
		}

	}

	void Init(){  
		Debug.Log ("texRender.width = " + texRender.width);
		Debug.Log ("texRender.height = " + texRender.height);
		matrix = new bool[texRender.width ][];
		for (int i = 0; i < texRender.width; i++) {  
			matrix [i] = new bool[texRender.height];
			for (int j = 0; j < texRender.height; j++) {  
				Color color = texRender.GetPixel (i,j);  
				if (isConner (i, j)) {
					color.a = 0;  
					matrix [i] [j] = true;
				} else {
					color.a = 1;
					matrix [i] [j] = false;
				}
				texRender.SetPixel (i,j,color);  

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

	public void Reset(){  
		//yield return new WaitForSeconds (1f);
	    rightConnerCount = 0;
		leftConnerCount = 0;
		totalCuoCount = 0;
		ReadyForErase = false;

		isMove = false;
		queue.Clear ();

		hasShowCard = false;

		image.gameObject.SetActive(true);
		for (int i = 0; i < texRender.width; i++) {  
			for (int j = 0; j < texRender.height; j++) {  
				Color color = texRender.GetPixel (i,j);  
				if (isConner (i, j)) {
					color.a = 0;  
					matrix [i] [j] = true;
				} else {
					color.a = 1;
					matrix [i] [j] = false;
				}
				texRender.SetPixel (i,j,color); 
			}  
		}  
		texRender.Apply ();  
		image.material.SetTexture ("_RendTex",texRender); 
	}



		

}  
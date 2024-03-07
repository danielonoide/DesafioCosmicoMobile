using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
//using System.Numerics;



public partial class Escenario : Node2D
{
	private class TouchInfo
	{
		public Vector2 Start { get; set; }
		public Vector2 Current { get; set; }
	}

	Dictionary<int,  Vector2> touches = new();  //start, current

	float prevRadius = 0;
	float currRadius = 0;
	int touchesLastFrame = 0;

	float LeftLimitZoom
	{
		get=>
		leftLimit+( (cameraSize.x*camera.Zoom.x)/2 ); 
	}

	float RightLimitZoom {get=>rightLimit-( (cameraSize.x*camera.Zoom.x)/2 );}
	float TopLimitZoom {get=>topLimit+( (cameraSize.y*camera.Zoom.y)/2 );}
	float BottomLimitZoom {get=>bottomLimit-( (cameraSize.y*camera.Zoom.y)/2 );}

	bool touchPressed = false;


    public void SetCamera(GloboTeledirigido globoTeledirigido)
	{
		RemoveChild(camera);
		camera.Position=Vector2.Zero;
		globoTeledirigido.AddChild(camera);
	}

	private void OnRemoteBalloonRemoved(GloboTeledirigido globoTeledirigido)
	{
		Vector2 prevPos=globoTeledirigido.GlobalPosition;
		globoTeledirigido.RemoveChild(camera);
		camera.GlobalPosition=prevPos;
		AddChild(camera);
	}

	void Zoom()
	{
		if(camera.Zoom.x>maxZoom)
		{
			float newZoom=(float)Math.Round(camera.Zoom.x-zoom, 1);
			camera.Zoom=new Vector2(newZoom, newZoom); 	
		}
	}

	void UnZoom()
	{
		if(camera.Zoom.x<minZoom)
		{
			float newZoom=(float)Math.Round(camera.Zoom.x+zoom, 1);
			camera.Zoom=new Vector2(newZoom, newZoom); 	
			return;
		}

		if(camera.Zoom.x==minZoom)
		{
			camera.Zoom=new Vector2(realMinZoom, realMinZoom); 	
		}
	}

	protected void _on_Zoom_pressed()
	{
		Zoom();
	}


	protected void _on_UnZoom_pressed()
	{
		UnZoom();
	}

	public override void _Input(InputEvent @event)
	{
		if(@event is InputEventMouseButton evento)
		{
			if (evento.Pressed && evento.ButtonIndex==(int)ButtonList.WheelDown) 
			{
				UnZoom();
			}
			if (evento.Pressed && evento.ButtonIndex==(int)ButtonList.WheelUp)
			{
				Zoom();		
			}	
			
			if(evento.ButtonIndex==(int)ButtonList.Right)
			{
				if(evento.Pressed)
				{
					rightClick=true;
				}
				else
				{
					rightClick=false;
				}
			}
		}
		
		if(@event is InputEventMouseMotion mouseMotion)
		{
			if (rightClick)
			{
				camera.SmoothingEnabled = false;
				Vector2 newPosition = camera.Position - mouseMotion.Relative * camera.Zoom;
				newPosition.x = Mathf.Clamp(newPosition.x, LeftLimitZoom, RightLimitZoom);
				newPosition.y = Mathf.Clamp(newPosition.y, TopLimitZoom, BottomLimitZoom);

				camera.Position=newPosition;
			}
			else
			{
				camera.SmoothingEnabled = true;
			}
		}

		//Mobile
		if(@event is InputEventScreenTouch screenTouch)
		{
			if(screenTouch.Pressed)
			{
				touchPressed = true;
				touches[screenTouch.Index] = screenTouch.Position;   //RELATIVO A LA CAMARA
			}
			else
			{
				touchPressed = false;
				touches.Remove(screenTouch.Index);

				if(touches.Count<2)
				{
					currRadius = 0;
					prevRadius = 0;
				}
			}

		}

		if(@event is InputEventScreenDrag screenDrag && Globals.MobileDevice && !GetTree().HasGroup("GloboTeledirigido"))
		{
			touches[screenDrag.Index] = screenDrag.Position;
			if(touchPressed && (!ProjectileLauncher.selected || screenDrag.Index>0))
			{
				camera.SmoothingEnabled = false;
				Vector2 newPosition = camera.Position - screenDrag.Relative * camera.Zoom;

				newPosition.x = Mathf.Clamp(newPosition.x, LeftLimitZoom, RightLimitZoom);
				newPosition.y = Mathf.Clamp(newPosition.y, TopLimitZoom, BottomLimitZoom);

				camera.Position = newPosition;
			}
			else
			{
				camera.SmoothingEnabled = true;
			}

			if(!ProjectileLauncher.selected)
			{
				UpdatePinchGesture();
			}

		}

		//simulate multitouch
/* 		if(@event is InputEventKey eventKey && eventKey.Scancode == (int)KeyList.A)
		{
			//touches[100] = camera.ToLocal(GetGlobalMousePosition());
			touches[100] = GetViewport().GetMousePosition();
		} */

		//simulate touch real
/*  		if(@event is InputEventKey eventKey && eventKey.Scancode == (int)KeyList.A)
		{
			Vector2 screenSize = GetViewportRect().Size;

			// Crea un nuevo evento de pantalla táctil en el centro de la pantalla
			InputEventScreenTouch touchEvent = new()
			{
				Position = screenSize / 2,  // Centrado en la pantalla
				Pressed = eventKey.Pressed,  // Simula toque presionado
				Index = 0  // Puedes cambiar esto según tus necesidades
			};

			// Envía el evento al sistema de entrada
			Input.ParseInputEvent(touchEvent);
		} */
		
	}

	Vector2 GetCenter(ICollection<Vector2> vectors)
	{
		Vector2 center = new();
		foreach(Vector2 vector in vectors)
		{
			center += vector;
		}

		center/=vectors.Count;

		return center;
	}

	void UpdatePinchGesture()
	{
		prevRadius = currRadius;

		Vector2 center = GetCenter(touches.Values);

		currRadius = (touches.Values.First() - center).Length();

		if(prevRadius == 0)
		{
			return;
		}

		float zoomFactor = (prevRadius - currRadius) / prevRadius;
		float finalZoom = camera.Zoom.x + zoomFactor;
		finalZoom = Mathf.Clamp(finalZoom, maxZoom, realMinZoom);
		camera.Zoom = new Vector2(finalZoom, finalZoom);

		//si tienes configurado el modo de estiramiento en 2D, entonces el tamaño del viewport se sobreescribe, por lo que las unidades
		//de movimiento del touch son cambiadas y cagan los calculos, por lo que hay que obtener el tamaño real del viewport sobreescrito
		//para hacer los cálculos correctos

		var vpSize = GetViewport().Size;
		if(GetViewport().IsSizeOverrideEnabled())
		{
			vpSize = GetViewport().GetSizeOverride();   
		}


		var oldDist = (center- (vpSize/2f)) * (camera.Zoom-new Vector2(zoomFactor, zoomFactor));
        var newDist = (center - (vpSize / 2f ) )* camera.Zoom;


		var camNeedMove = oldDist-newDist;
		camera.Position += camNeedMove;

	}


}
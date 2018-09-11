using UnityEngine;
using UnityEngine.EventSystems;

public class EventTriggerListener : MonoBehaviour,
    IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler,
    IPointerUpHandler, ISelectHandler, IUpdateSelectedHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
//public class EventTriggerListener : EventTrigger
{
    public delegate void VoidDelegate (GameObject go);
    public delegate void DragDelegate(float deltaX, float deltaY);
    public VoidDelegate onClick;
    public VoidDelegate onDown;
    public VoidDelegate onEnter;
    public VoidDelegate onExit;
    public VoidDelegate onUp;
    public VoidDelegate onSelect;
    public VoidDelegate onUpdateSelect;
    public VoidDelegate onBeginDrag;
    public VoidDelegate onDrag;
    public VoidDelegate onEndDrag;
    public DragDelegate onDeltaDrag;

	static public EventTriggerListener get(Transform trans)
	{
        EventTriggerListener listener = trans.GetComponent<EventTriggerListener>();
        if (listener == null) listener = trans.gameObject.AddComponent<EventTriggerListener>();
		return listener;
	}

    //public static void addClickEvent(this GameObject obj, LuaFunction func)
    //{
    //    EventTriggerListener.get(obj).onClick = delegate
    //    {
    //        func.Call();
    //        //func.Call(obj);
    //    };
    //}

    //public static void addPointerDownEvent(this GameObject obj, LuaFunction func)
    //{
    //    EventTriggerListener.get(obj).onDown = delegate
    //    {
    //        func.Call();
    //        //func.Call(obj);
    //    };
    //}

    //public static void addPointerEnterEvent(this GameObject obj, LuaFunction func)
    //{
    //    EventTriggerListener.get(obj).onEnter = delegate
    //    {
    //        func.Call();
    //        //func.Call(obj);
    //    };
    //}

    //public static void addPointerExitEvent(this GameObject obj, LuaFunction func)
    //{
    //    EventTriggerListener.get(obj).onExit = delegate
    //    {
    //        func.Call();
    //        //func.Call(obj);
    //    };
    //}

    //public static void addPointerUpEvent(this GameObject obj, LuaFunction func)
    //{
    //    EventTriggerListener.get(obj).onUp = delegate
    //    {
    //        func.Call();
    //        //func.Call(obj);
    //    };
    //}

    //public static void addSelectEvent(this GameObject obj, LuaFunction func)
    //{
    //    EventTriggerListener.get(obj).onSelect = delegate
    //    {
    //        func.Call();
    //        //func.Call(obj);
    //    };
    //}

    //public static void addUpdateSelectEvent(this GameObject obj, LuaFunction func)
    //{
    //    EventTriggerListener.get(obj).onUpdateSelect = delegate
    //    {
    //        func.Call();
    //        //func.Call(obj);
    //    };
    //}

    //public static void addBeginDragEvent(this GameObject obj, LuaFunction func)
    //{
    //    EventTriggerListener.get(obj).onBeginDrag = delegate
    //    {
    //        func.Call();
    //        //func.Call(obj);
    //    };
    //}

    //public static void addEndDragEvent(this GameObject obj, LuaFunction func)
    //{

    //    EventTriggerListener.get(obj).onEndDrag = delegate
    //    {
    //        func.Call();
    //        //func.Call(obj);
    //    };
    //}

	public void OnPointerClick(PointerEventData eventData)
	{
		if(onClick != null) onClick(gameObject);
	}
    public void OnPointerDown(PointerEventData eventData)
    {
		if(onDown != null) onDown(gameObject);
	}
    public void OnPointerEnter(PointerEventData eventData)
    {
		if(onEnter != null) onEnter(gameObject);
	}
	public void OnPointerExit (PointerEventData eventData){
		if(onExit != null) onExit(gameObject);
	}
	public void OnPointerUp (PointerEventData eventData){
		if(onUp != null) onUp(gameObject);
	}
	public void OnSelect (BaseEventData eventData){
		if(onSelect != null) onSelect(gameObject);
	}
	public void OnUpdateSelected (BaseEventData eventData){
		if(onUpdateSelect != null) onUpdateSelect(gameObject);
	}
	public void OnBeginDrag(PointerEventData eventData){
		if(onBeginDrag != null) onBeginDrag(gameObject);
	}

    public void OnDrag(PointerEventData data)
    {
        if (onDrag != null) onDrag(gameObject);
        if (onDeltaDrag != null) onDeltaDrag(data.delta.x, data.delta.y);
    }

	public void OnEndDrag(PointerEventData eventData)
	{
        if (onEndDrag != null) onEndDrag(gameObject);
	}
}

//using Input;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TakeItUtilities
{
    public class UIUtils : MonoBehaviour
    {
        //Always returns actual size of rect
        public static Vector2 GetActualSize(RectTransform rect)
        {
            Vector2 size = rect.sizeDelta;

            if (size == Vector2.zero)
            {
                size = new Vector2(rect.rect.width, rect.rect.height);
            }

            if (size == Vector2.zero)
            {
                var v = new Vector3[4];
                rect.GetWorldCorners(v);

                size = new Vector2(v[3].x - v[0].x, v[1].y - v[0].y);
            }

            return size;
        }

        public static void ClearContent(Transform transform)
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }

        //Clamps position to screen, allows to clamp with different resolutions and settings
        public static void ClampPositionToScreen(RectTransform rt)
        {
            Vector2 size = GetActualSize(rt);
            ClampPositionToScreen(rt, size);
        }
        public static void ClampPositionToScreen(RectTransform rt, CanvasScaler scaler)
        {
            Vector2 size = GetActualSize(rt);
            ClampRectPositionToScreen(rt, scaler, size);
        }
        public static void ClampPositionToScreen(RectTransform rt, Vector2 rtSize)
        {
            ClampPositionToScreen(rt, new Vector2(Screen.width, Screen.height), rtSize);
        }
        public static void ClampRectPositionToScreen(RectTransform rt, CanvasScaler scaler, Vector2 rtSize)
        {
            ClampPositionToScreen(rt, scaler.referenceResolution, rtSize);
        }
        public static void ClampPositionToScreen(RectTransform rt, Vector2 screenSize, Vector2 rtSize)
        {
            float maxX = (screenSize.x / 2) - rtSize.x;
            float maxY = (screenSize.y / 2) - rtSize.y;

            Vector2 pos = rt.anchoredPosition;

            pos = new Vector2(Mathf.Clamp(pos.x, -maxX, maxX), Mathf.Clamp(pos.y, -maxY, maxY));

            rt.anchoredPosition = pos;
        }
        public static Vector2 ClampPositionToScreen(Vector2 pos, Vector2 screenSize)
        {
            return ClampPositionToScreen(pos, screenSize, Vector2.zero);
        }
        public static Vector2 ClampPositionToScreen(Vector2 pos, Vector2 screenSize, Vector2 rtSize)
        {
            float maxX = (screenSize.x / 2) - rtSize.x;
            float maxY = (screenSize.y / 2) - rtSize.y;

            pos = new Vector2(Mathf.Clamp(pos.x, -maxX, maxX), Mathf.Clamp(pos.y, -maxY, maxY));

            return pos;
        }

        //Hold position to stay at the screen edge, works with different resolutions
        //public static Vector2 ClampPositionAtScreenEdge(Vector2 pos, Vector2 screenSize)
        //{
        //    //Get position relative to screen size
        //    Vector2 screenPos = new Vector2(pos.x + (screenSize.x / 2), pos.y + (screenSize.y / 2)); //from -0.5 : 0.5 to 0 : 1

        //    float distToXMax = MathUtils.MapTo01(screenSize.x - screenPos.x, 0, screenSize.x); //right
        //    float distToXMin = MathUtils.MapTo01(screenPos.x, 0, screenSize.x); //left

        //    float distToX = distToXMin < distToXMax ? distToXMin : distToXMax; //Get closest X


        //    float distToYMax = MathUtils.MapTo01(screenSize.y - screenPos.y, 0, screenSize.y); //bottom
        //    float distToYMin = MathUtils.MapTo01(screenPos.y, 0, screenSize.y); //top

        //    float distToY = distToYMin < distToYMax ? distToYMin : distToYMax; //Get closest Y


        //    Vector2 newPos;

        //    //Set position relative to start position (from 0 : 1 to -0.5 : 0.5)
        //    if (distToX < distToY)
        //    {
        //        newPos = new Vector2(
        //            distToXMin < distToXMax ? -(screenSize.x / 2) : screenSize.x / 2, 
        //            pos.y
        //        );
        //    }
        //    else
        //    {
        //        newPos = new Vector2(
        //            pos.x, 
        //            distToYMin < distToYMax ? -(screenSize.y / 2) : screenSize.y / 2
        //        );
        //    }

        //    return newPos;
        //}

        //Allows to Debug what is under mouse pointer
        public static PointerEventData ScreenPosToPointerData(Vector2 screenPos)
           => new(EventSystem.current) { position = screenPos };

        public static GameObject UIRaycast(PointerEventData pointerData)
        {
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            return results.Count < 1 ? null : results[0].gameObject;
        }
        public static List<RaycastResult> UIRaycastAll(PointerEventData pointerData)
        {
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            return results;
        }

        public static bool IsPointerOverUI(Vector2 screenPos)
        {
            var hitObject = UIRaycast(ScreenPosToPointerData(screenPos));
            return hitObject != null && hitObject.layer == LayerMask.NameToLayer("UI");
        }

        public static bool IsPointerOverGameObject(Vector2 screenPos)
        {
            PointerEventData eventDataCurrentPosition = new(EventSystem.current)
            {
                position = screenPos
            };

            List<RaycastResult> results = new();

            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

            return results.Count > 0;
        }

        //public static void DebugGameObjectUnderPointer()
        //{ 
        //    Debug.Log(UIRaycast(ScreenPosToPointerData(InputReader.InputActions.MenuControls.Point.ReadValue<Vector2>())));
        //}
        //public static void DebugAllGameObjectsUnderPointer()
        //{
        //    string str = "";

        //    var results = UIRaycastAll(ScreenPosToPointerData(InputReader.InputActions.MenuControls.Point.ReadValue<Vector2>()));

        //    foreach(var result in results)
        //    {
        //        str += $"{result.gameObject}; ";
        //    }

        //    Debug.Log(str);
        //}

        //public static bool PrintUnderPointer(out GameObject go)
        //{
        //    var screenpos = InputReader.InputActions.MenuControls.Point.ReadValue<Vector2>();
        //    var hitObject = UIRaycast(ScreenPosToPointerData(screenpos));

        //    if (hitObject != null && hitObject.layer == LayerMask.NameToLayer("UI"))
        //    {
        //        go = hitObject.gameObject;
        //        return true;
        //    }

        //    go = null;

        //    return false;
        //}


        public static Vector2 GetImagePreservedAspectSize(Image image)
        {
            if (image.sprite == null) return Vector2.zero;

            Vector2 rectTransformSize = image.rectTransform.sizeDelta;

            float aspectRatio = image.sprite.rect.width / image.sprite.rect.height;

            float preservedAspectWidth = rectTransformSize.y * aspectRatio;
            float preservedAspectHeight = rectTransformSize.y;

            return new Vector2(preservedAspectWidth, preservedAspectHeight); 
        }


        //public static Vector2 GetAnchoredPosFromAbsoluteScreenPos(RectTransform rt, Vector2 screenPos)
        //{
        //    RectTransform canvasRect = rt.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        //    Vector2 canvasSize = GetActualSize(canvasRect);

        //    return GetAnchoredPosFromAbsoluteScreenPos(rt, screenPos, canvasSize);
        //}
        //public static Vector2 GetAnchoredPosFromAbsoluteScreenPos(RectTransform rt, Vector2 screenPos, Vector2 canvasSize)
        //{
        //    //Get anchors
        //    float anchorX = (rt.anchorMax.x + rt.anchorMin.x) / 2;
        //    float anchorY = (rt.anchorMax.y + rt.anchorMin.y) / 2;

        //    //Scale screen pos to canvas resolution
        //    screenPos.x = MathUtils.Map(screenPos.x, 0, Screen.width, 0, canvasSize.x, true);
        //    screenPos.y = MathUtils.Map(screenPos.y, 0, Screen.height, 0, canvasSize.y, true);

        //    //Calculate with offset
        //    float scaledX = screenPos.x - (canvasSize.x * anchorX);
        //    float scaledY = screenPos.y - (canvasSize.y * anchorY);

        //    return new Vector2(scaledX, scaledY);
        //}


        public static bool CheckRectTransformsOverlap(RectTransform rectTransform1, RectTransform rectTransform2)
        {
            Rect rect1 = GetWorldRect(rectTransform1);
            Rect rect2 = GetWorldRect(rectTransform2);

            return CheckRectTransformsOverlap(rect1, rect2);
        }
        public static bool CheckRectTransformsOverlap(Rect rect, RectTransform rectTransform)
        {
            Rect rect2 = GetWorldRect(rectTransform);

            return CheckRectTransformsOverlap(rect, rect2);
        }
        public static bool CheckRectTransformsOverlap(Rect rect1, Rect rect2)
        {
            return rect1.Overlaps(rect2);
        }

        public static Rect GetWorldRect(RectTransform rectTransform)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            Vector2 min = new(float.MaxValue, float.MaxValue);
            Vector2 max = new(float.MinValue, float.MinValue);

            for (int i = 0; i < 4; i++)
            {
                min = Vector2.Min(min, (Vector2)corners[i]);
                max = Vector2.Max(max, (Vector2)corners[i]);
            }

            return new Rect(min, max - min);
        }
    }
}

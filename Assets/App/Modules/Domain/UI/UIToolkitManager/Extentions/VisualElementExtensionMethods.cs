using UnityEngine;
using UnityEngine.UIElements;
public static class VisualElementExtensionMethods
{
    public static void Rotate(this VisualElement item, float angleDegrees)
    {
        // Get the x/y location of the center note - these are constant unless the parent rescales; i.e., they don't change with rotation.
        float x0 = item.contentRect.center.x;
        float y0 = item.contentRect.center.y;

        // Convert Cartesian to Polar
        float r = Mathf.Sqrt(x0 * x0 + y0 * y0);
        float theta0 = Mathf.Atan2(y0, x0);

        // Calculate the location of the center of the VisualElement after rotating
        // Note: The rotation you want is *in addition* to the "default" polar angle from origin to the center
        float x = r * Mathf.Cos(theta0 + (Mathf.Deg2Rad * angleDegrees));
        float y = r * Mathf.Sin(theta0 + (Mathf.Deg2Rad * angleDegrees));

        // Actually do the requested rotation
        item.transform.rotation = Quaternion.Euler(0f, 0f, angleDegrees);

        // Finally, rotation happens about the upper-left corner of the VisualElement, so you need to shift the position
        // to get the rotated center to be coincident with the un-rotated center.
        float xDelta = x0 - x;
        float yDelta = y0 - y;
        item.transform.position = new Vector3(xDelta, yDelta, 0f);
    }
}

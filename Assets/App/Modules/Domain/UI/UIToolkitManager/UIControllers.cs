using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZFramework;

public class UIControllers
{
    public List<IUIController> Controllers = new List<IUIController>();

    public static UIControllers operator +(UIControllers container, IUIController controller)
    {
        container.Controllers.Add(controller);
        return container;
    }
}

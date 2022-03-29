using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFramework
{
    public interface ITask
    {
        Promise Begin();
    }

    public abstract class AppTask
    {
        public readonly Promise Promise = new Promise();
        public abstract IEnumerator Run();
    }
}

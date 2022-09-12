using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFramework
{
    public partial class App : MonoBehaviour
    {
        public struct Hook
        {
            private Action actions;

            public void Invoke()
            {
                if (actions != null)
                {
                    foreach (Action action in actions.GetInvocationList())
                    {
                        try
                        {
                            action.Invoke();
                        }
                        catch (Exception e)
                        {
                            App.LogException(action.Method.Name, e);
                        }
                    }
                }
            }

            public static Hook operator +(Hook hook, Action action)
            {
                hook.actions += action;
                return hook;
            }

            public static Hook operator -(Hook hook, Action action)
            {
                hook.actions -= action;
                return hook;
            }
        }

        public struct Hook<T>
        {
            private Action<T> actions;

            public void Invoke(T value)
            {
                if (actions != null)
                {
                    foreach (Action<T> action in actions.GetInvocationList())
                    {
                        try
                        {
                            action.Invoke(value);
                        }
                        catch (Exception e)
                        {
                            App.LogException(action.Method.Name, e);
                        }
                    }
                }
            }

            public static Hook<T> operator +(Hook<T> hook, Action<T> action)
            {
                hook.actions += action;
                return hook;
            }

            public static Hook<T> operator -(Hook<T> hook, Action<T> action)
            {
                hook.actions -= action;
                return hook;
            }
        }

        public struct Hook<T1, T2>
        {
            private Action<T1, T2> actions;

            public void Invoke(T1 v1, T2 v2)
            {
                if (actions != null)
                {
                    foreach (Action<T1, T2> action in actions.GetInvocationList())
                    {
                        try
                        {
                            action.Invoke(v1, v2);
                        }
                        catch (Exception e)
                        {
                            App.LogException(action.Method.Name, e);
                        }
                    }
                }
            }

            public static Hook<T1, T2> operator +(Hook<T1, T2> hook, Action<T1, T2> action)
            {
                hook.actions += action;
                return hook;
            }

            public static Hook<T1, T2> operator -(Hook<T1, T2> hook, Action<T1, T2> action)
            {
                hook.actions -= action;
                return hook;
            }
        }

        public struct Hook<T1, T2, T3>
        {
            private Action<T1, T2, T3> actions;

            public void Invoke(T1 v1, T2 v2, T3 v3)
            {
                if (actions != null)
                {
                    foreach (Action<T1, T2, T3> action in actions.GetInvocationList())
                    {
                        try
                        {
                            action.Invoke(v1, v2, v3);
                        }
                        catch (Exception e)
                        {
                            App.LogException(action.Method.Name, e);
                        }
                    }
                }
            }

            public static Hook<T1, T2, T3> operator +(Hook<T1, T2, T3> hook, Action<T1, T2, T3> action)
            {
                hook.actions += action;
                return hook;
            }

            public static Hook<T1, T2, T3> operator -(Hook<T1, T2, T3> hook, Action<T1, T2, T3> action)
            {
                hook.actions -= action;
                return hook;
            }
        }

        public struct Hook<T1, T2, T3, T4>
        {
            private Action<T1, T2, T3, T4> actions;

            public void Invoke(T1 v1, T2 v2, T3 v3, T4 v4)
            {
                if (actions != null)
                {
                    foreach (Action<T1, T2, T3, T4> action in actions.GetInvocationList())
                    {
                        try
                        {
                            action.Invoke(v1, v2, v3, v4);
                        }
                        catch (Exception e)
                        {
                            App.LogException(action.Method.Name, e);
                        }
                    }
                }
            }

            public static Hook<T1, T2, T3, T4> operator +(Hook<T1, T2, T3, T4> hook, Action<T1, T2, T3, T4> action)
            {
                hook.actions += action;
                return hook;
            }

            public static Hook<T1, T2, T3, T4> operator -(Hook<T1, T2, T3, T4> hook, Action<T1, T2, T3, T4> action)
            {
                hook.actions -= action;
                return hook;
            }
        }
    }
}
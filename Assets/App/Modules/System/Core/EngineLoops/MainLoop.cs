using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFramework
{
    /// <summary>
    /// Данный класс планировался для управления всеми циклами в движке, но не был сделан из-за отсутствия времени. Его функционал только отвечает за переключение PreInit и Init функций менеджеров.
    /// </summary>
    public class MainLoop
    {
        public enum State
        {
            PreInit = 0,
            Init = 10,
            PostInit = 20,
            Main = 30
        }

        public State CurState { get; private set; }

        public MainLoop()
        {
            CurState = State.PreInit;
        }

        public void NextInit()
        {
            if (CurState < State.Main) CurState += 10;
        }
        //public static FixedLoop FixedLoop { get { return Instance.GetComponent<FixedLoop>(); } }

        //public void BeforeInit() {

        //}

        //public void Init() {

        //}

        //public void AfterInit() {

        //}
    }
}
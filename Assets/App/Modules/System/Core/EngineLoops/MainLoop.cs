using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFramework
{
    public class MainLoop
    {
        public enum State
        {
            PreInit = 0,
            Init = 10,
            PostInit = 20,
            Main = 30
        }

        #region Fields

        // ---------------------------------------------------------------------------------------------------------
        // Public properties
        // ---------------------------------------------------------------------------------------------------------

        public State CurState { get; private set; }

        #endregion

        public MainLoop()
        {
            CurState = State.PreInit;
        }

        #region Methods

        // ---------------------------------------------------------------------------------------------------------
        // Public Methods
        // ---------------------------------------------------------------------------------------------------------

        public void NextInit()
        {
            if (CurState < State.Main)
                CurState += 10;
        }

        #endregion
    }
}